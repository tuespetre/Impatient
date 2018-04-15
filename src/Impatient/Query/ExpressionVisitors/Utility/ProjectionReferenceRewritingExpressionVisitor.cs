using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Enumerable;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class ProjectionReferenceRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<string> nameStack = new Stack<string>();
        private readonly AliasedTableExpression targetTable;

        public ProjectionReferenceRewritingExpressionVisitor(AliasedTableExpression targetTable)
        {
            this.targetTable = targetTable;
        }

        public override Expression Visit(Expression node)
        {
            IEnumerable<string> GetNameParts()
            {
                return nameStack.Reverse().Where(n => n != null && !n.StartsWith("<>"));
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
                        nameStack.Push(newExpression.Members[i].GetPathSegmentName());
                        arguments[i] = Visit(arguments[i]);
                        nameStack.Pop();
                    }

                    return newExpression.Update(arguments);
                }

                case MemberInitExpression memberInitExpression:
                {
                    var newExpression = (NewExpression)Visit(memberInitExpression.NewExpression);
                    var bindings = memberInitExpression.Bindings.ToArray();

                    for (var i = 0; i < bindings.Length; i++)
                    {
                        nameStack.Push(bindings[i].Member.GetPathSegmentName());
                        bindings[i] = VisitMemberBinding(bindings[i]);
                        nameStack.Pop();
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

                    nameStack.Push("Key");

                    var outerKeySelector = Visit(groupByResultExpression.OuterKeySelector);
                    var innerKeySelector = updater.Visit(groupByResultExpression.InnerKeySelector);
                    var elementSelector = updater.Visit(groupByResultExpression.ElementSelector);

                    nameStack.Pop();

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

                    nameStack.Push("Key");

                    var outerKeySelector = Visit(groupedRelationalQueryExpression.OuterKeySelector);
                    var innerKeySelector = updater.Visit(groupedRelationalQueryExpression.InnerKeySelector);

                    nameStack.Pop();

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
                    var expression
                        = new SqlColumnNullabilityExpressionVisitor()
                            .Visit(Visit(defaultIfEmptyExpression.Expression));

                    var name = string.Join(".", GetNameParts().Append("$empty"));

                    var flag = new SqlColumnExpression(targetTable, name, typeof(bool?), true);

                    return new DefaultIfEmptyExpression(expression, flag);
                }

                case PolymorphicExpression polymorphicExpression:
                {
                    var row = Visit(polymorphicExpression.Row);

                    // TODO: Visit nested polymorphic expressions.
                    return new PolymorphicExpression(
                        polymorphicExpression.Type,
                        row,
                        polymorphicExpression.Descriptors);
                }

                case ExtraPropertiesExpression extraPropertiesExpression:
                {
                    var properties = new Expression[extraPropertiesExpression.Properties.Count];

                    for (var i = 0; i < extraPropertiesExpression.Properties.Count; i++)
                    {
                        nameStack.Push(extraPropertiesExpression.Names[i]);

                        properties[i] = Visit(extraPropertiesExpression.Properties[i]);

                        nameStack.Pop();
                    }

                    var expression = Visit(extraPropertiesExpression.Expression);

                    return extraPropertiesExpression.Update(expression, properties);
                }

                case AnnotationExpression annotationExpression:
                {
                    return base.Visit(annotationExpression);
                }

                case UnaryExpression unaryExpression
                when unaryExpression.NodeType == ExpressionType.Convert:
                {
                    return unaryExpression.Update(Visit(unaryExpression.Operand));
                }

                default:
                {
                    var parts = GetNameParts();
                    var isNullable = node.Type.IsNullableType();

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
    }
}
