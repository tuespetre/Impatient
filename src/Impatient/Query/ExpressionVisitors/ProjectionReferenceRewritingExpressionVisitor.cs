using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class ProjectionReferenceRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<string> currentPath = new Stack<string>();
        private readonly AliasedTableExpression targetTable;

        public ProjectionReferenceRewritingExpressionVisitor(AliasedTableExpression targetTable)
        {
            this.targetTable = targetTable;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case NewExpression newExpression:
                {
                    if (newExpression.Members == null)
                    {
                        return newExpression;
                    }

                    var arguments = newExpression.Arguments.ToArray();

                    for (var i = 0; i < newExpression.Members.Count; i++)
                    {
                        currentPath.Push(newExpression.Members[i].Name);
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
                        currentPath.Push(bindings[i].Member.Name);
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
                    var expression = Visit(defaultIfEmptyExpression.Expression);

                    var parts = currentPath.Reverse().Where(n => !n.StartsWith("<>")).Concat(Enumerable.Repeat("$empty", 1));

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

                case null:
                {
                    return null;
                }

                default:
                {
                    var parts = currentPath.Reverse().Where(n => !n.StartsWith("<>"));

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

                    var alias = string.Join(".", parts.DefaultIfEmpty("$c"));

                    return new SqlColumnExpression(targetTable, alias, node.Type);
                }
            }
        }
    }
}
