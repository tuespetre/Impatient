using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class EnumerableQueryEqualityRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo queryableAnyMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IQueryable<object> q) => q.Any());

        private static readonly MethodInfo enumerableAnyMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> q) => q.Any());

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                {
                    var left = Visit(node.Left);
                    var right = Visit(node.Right);

                    var leftQuery = left.UnwrapInnerExpression() as EnumerableRelationalQueryExpression;
                    var rightQuery = right.UnwrapInnerExpression() as EnumerableRelationalQueryExpression;
                    
                    if (leftQuery != null && rightQuery != null)
                    {
                        // TODO: Maybe return SequenceEqual?
                    }
                    else if (leftQuery != null || rightQuery != null)
                    {
                        var query = leftQuery ?? rightQuery;
                        var operand = leftQuery == null ? left : right;

                        if (operand.IsNullConstant())
                        {
                            // We return a call to Any instead of a SqlExistsExpression
                            // because the query might be (and probably will be) a 
                            // GroupedRelationalQueryExpression, in which case pulling
                            // the SelectExpression out would be incorrect.

                            var genericMethodInfo
                                = query.Type.IsGenericType(typeof(IQueryable<>))
                                    ? queryableAnyMethodInfo
                                    : enumerableAnyMethodInfo;

                            var result
                                = (Expression)Expression.Call(
                                    genericMethodInfo.MakeGenericMethod(query.SelectExpression.Type),
                                    query);

                            if (node.NodeType == ExpressionType.Equal)
                            {
                                result = Expression.Not(result);
                            }

                            return result;
                        }
                    }

                    return node.Update(left, node.Conversion, right);
                }

                default:
                {
                    return base.VisitBinary(node);
                }
            }
        }
    }
}
