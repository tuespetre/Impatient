using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Enumerable;

namespace Impatient.Query.ExpressionVisitors.Utility
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
                    var uniquifier = new TableUniquifyingExpressionVisitor();

                    var oldSelectExpression = groupByResultExpression.SelectExpression;
                    var newSelectExpression = uniquifier.VisitAndConvert(oldSelectExpression, nameof(Visit));

                    var oldTables = oldSelectExpression.Table.Flatten().ToArray();
                    var newTables = newSelectExpression.Table.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    currentPath.Push("Key");

                    var outerKeySelector = Visit(groupByResultExpression.OuterKeySelector);
                    var innerKeySelector = updater.Visit(groupByResultExpression.InnerKeySelector);
                    var elementSelector = updater.Visit(groupByResultExpression.ElementSelector);

                    currentPath.Pop();

                    return new GroupedRelationalQueryExpression(
                        newSelectExpression.UpdateProjection(new ServerProjectionExpression(elementSelector)),
                        outerKeySelector,
                        innerKeySelector,
                        groupByResultExpression.InnerKeyLambda,
                        requiresDenullification: false,
                        type: groupByResultExpression.Type);
                }

                case GroupedRelationalQueryExpression groupedRelationalQueryExpression:
                {
                    var uniquifier = new TableUniquifyingExpressionVisitor();

                    var oldSelectExpression = groupedRelationalQueryExpression.SelectExpression;
                    var newSelectExpression = uniquifier.VisitAndConvert(oldSelectExpression, nameof(Visit));

                    var oldTables = oldSelectExpression.Table.Flatten().ToArray();
                    var newTables = newSelectExpression.Table.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    currentPath.Push("Key");

                    var outerKeySelector = Visit(groupedRelationalQueryExpression.OuterKeySelector);
                    var innerKeySelector = updater.Visit(groupedRelationalQueryExpression.InnerKeySelector);

                    currentPath.Pop();

                    return new GroupedRelationalQueryExpression(
                        newSelectExpression,
                        outerKeySelector,
                        innerKeySelector,
                        groupedRelationalQueryExpression.InnerKeyLambda,
                        groupedRelationalQueryExpression.RequiresDenullification,
                        groupedRelationalQueryExpression.Type);
                }

                case DefaultIfEmptyExpression defaultIfEmptyExpression:
                {
                    var expression = sqlColumnNullabilityExpressionVisitor.Visit(Visit(defaultIfEmptyExpression.Expression));

                    var parts = GetNameParts().Append("$empty");

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
                    var isNullable = !node.Type.GetTypeInfo().IsValueType;

                    switch (node)
                    {
                        case SqlColumnExpression sqlColumnExpression:
                        {
                            parts = parts.DefaultIfEmpty(sqlColumnExpression.ColumnName);
                            isNullable = sqlColumnExpression.IsNullable;
                            break;
                        }

                        case SqlAliasExpression sqlAliasExpression:
                        {
                            parts = parts.DefaultIfEmpty(sqlAliasExpression.Alias);
                            break;
                        }
                    }

                    return new SqlColumnExpression(
                        targetTable,
                        string.Join(".", parts),
                        node.Type,
                        isNullable);
                }
            }
        }

        private class SqlColumnNullabilityExpressionVisitor : ProjectionExpressionVisitor
        {
            protected override Expression VisitLeaf(Expression node)
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
                        return node;
                    }
                }
            }
        }
    }
}
