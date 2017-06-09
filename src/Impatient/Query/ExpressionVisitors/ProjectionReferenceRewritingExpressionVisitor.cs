using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Enumerable;

namespace Impatient.Query.ExpressionVisitors
{
    public class ProjectionReferenceRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly SqlColumnNullabilityExpressionVisitor sqlColumnNullabilityExpressionVisitor
            = new SqlColumnNullabilityExpressionVisitor();

        private readonly Stack<string> currentPath = new Stack<string>();
        private readonly AliasedTableExpression targetTable;

        public ProjectionReferenceRewritingExpressionVisitor(AliasedTableExpression targetTable)
        {
            this.targetTable = targetTable;
        }

        public override Expression Visit(Expression node)
        {
            IEnumerable<string> GetNameParts()
            {
                return currentPath.Reverse().Where(n => n != null && !n.StartsWith("<>"));
            }

            switch (node)
            {
                case null:
                {
                    return null;
                }

                case NewExpression newExpression:
                {
                    if (newExpression.Members == null)
                    {
                        return newExpression;
                    }

                    var arguments = newExpression.Arguments.ToArray();

                    for (var i = 0; i < newExpression.Members.Count; i++)
                    {
                        currentPath.Push(newExpression.Members[i].GetPathSegmentName());
                        arguments[i] = Visit(arguments[i]);
                        currentPath.Pop();
                    }

                    return newExpression.Update(arguments);
                }

                case MemberInitExpression memberInitExpression:
                {
                    var newExpression = (NewExpression)Visit(memberInitExpression.NewExpression);
                    var bindings = memberInitExpression.Bindings.ToArray();

                    for (var i = 0; i < bindings.Length; i++)
                    {
                        currentPath.Push(bindings[i].Member.GetPathSegmentName());
                        bindings[i] = VisitMemberBinding(bindings[i]);
                        currentPath.Pop();
                    }

                    return memberInitExpression.Update(newExpression, bindings);
                }

                case GroupByResultExpression groupByResultExpression:
                {
                    var selectExpression = groupByResultExpression.SelectExpression;
                    var uniquifier = new TableUniquifyingExpressionVisitor();
                    var oldTables = selectExpression.Table.Flatten();

                    selectExpression = uniquifier.VisitAndConvert(selectExpression, nameof(Visit));

                    var newTables = selectExpression.Table.Flatten();

                    var replacingVisitor
                        = new ExpressionReplacingExpressionVisitor(
                            oldTables.Zip(newTables, ValueTuple.Create<Expression, Expression>)
                                .ToDictionary(t => t.Item1, t => t.Item2));

                    currentPath.Push("Key");

                    var outerKeySelector = Visit(groupByResultExpression.OuterKeySelector);
                    var innerKeySelector = replacingVisitor.Visit(groupByResultExpression.InnerKeySelector);
                    var elementSelector = replacingVisitor.Visit(groupByResultExpression.ElementSelector);

                    currentPath.Pop();

                    return new GroupedRelationalQueryExpression(
                        selectExpression.UpdateProjection(new ServerProjectionExpression(elementSelector)),
                        outerKeySelector,
                        innerKeySelector,
                        groupByResultExpression.InnerKeyLambda,
                        groupByResultExpression.Type);
                }

                case GroupedRelationalQueryExpression groupedRelationalQueryExpression:
                {
                    var selectExpression = groupedRelationalQueryExpression.SelectExpression;
                    var uniquifier = new TableUniquifyingExpressionVisitor();
                    var oldTables = selectExpression.Table.Flatten();

                    selectExpression = uniquifier.VisitAndConvert(selectExpression, nameof(Visit));

                    var newTables = selectExpression.Table.Flatten();

                    var replacingVisitor
                        = new ExpressionReplacingExpressionVisitor(
                            oldTables.Zip(newTables, ValueTuple.Create<Expression, Expression>)
                                .ToDictionary(t => t.Item1, t => t.Item2));

                    currentPath.Push("Key");

                    var outerKeySelector = Visit(groupedRelationalQueryExpression.OuterKeySelector);
                    var innerKeySelector = replacingVisitor.Visit(groupedRelationalQueryExpression.InnerKeySelector);

                    currentPath.Pop();

                    return new GroupedRelationalQueryExpression(
                        selectExpression,
                        outerKeySelector,
                        innerKeySelector,
                        groupedRelationalQueryExpression.InnerKeyLambda,
                        groupedRelationalQueryExpression.Type);
                }

                case DefaultIfEmptyExpression defaultIfEmptyExpression:
                {
                    var expression = sqlColumnNullabilityExpressionVisitor.Visit(Visit(defaultIfEmptyExpression.Expression));

                    var parts = GetNameParts().Concat(Repeat("$empty", 1));

                    var name = string.Join(".", parts);

                    var flag = new SqlColumnExpression(targetTable, name, typeof(bool?), true);

                    return new DefaultIfEmptyExpression(expression, flag);
                }

                case PolymorphicExpression polymorphicExpression:
                {
                    return new PolymorphicExpression(
                        polymorphicExpression.Type,
                        Visit(polymorphicExpression.Row), 
                        polymorphicExpression.Descriptors);
                }

                case AnnotationExpression annotationExpression:
                {
                    return VisitExtension(annotationExpression);
                }

                default:
                {
                    var parts = GetNameParts();

                    switch (node)
                    {
                        case SqlColumnExpression sqlColumnExpression:
                        {
                            parts = parts.DefaultIfEmpty(sqlColumnExpression.ColumnName);
                            break;
                        }

                        case SqlAliasExpression sqlAliasExpression:
                        {
                            parts = parts.DefaultIfEmpty(sqlAliasExpression.Alias);
                            break;
                        }
                    }

                    return new SqlColumnExpression(targetTable, string.Join(".", parts), node.Type);
                }
            }
        }

        private class SqlColumnNullabilityExpressionVisitor : ExpressionVisitor
        {
            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case SqlColumnExpression sqlColumnExpression:
                    {
                        return new SqlColumnExpression(
                            sqlColumnExpression.Table,
                            sqlColumnExpression.ColumnName,
                            sqlColumnExpression.Type,
                            isNullable: true);
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }
    }
}
