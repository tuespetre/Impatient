using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class ComplexNestedQueryAnnotatingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case ComplexNestedQueryExpression complexNestedQuery:
                {
                    return complexNestedQuery;
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQuery
                when !singleValueRelationalQuery.Type.IsScalarType()
                    && singleValueRelationalQuery.SelectExpression.Projection is ServerProjectionExpression:
                {
                    return new ComplexNestedQueryExpression(singleValueRelationalQuery, node.Type);
                }

                case MethodCallExpression methodCall
                when methodCall.Method.DeclaringType == typeof(Enumerable)
                    && (methodCall.Method.Name == nameof(Enumerable.ToArray)
                        || methodCall.Method.Name == nameof(Enumerable.ToList))
                    && methodCall.Arguments[0] is EnumerableRelationalQueryExpression enumerableRelationalQuery
                    && enumerableRelationalQuery.SelectExpression.Projection is ServerProjectionExpression:
                {
                    return new ComplexNestedQueryExpression(enumerableRelationalQuery, node.Type);
                }
            }

            return base.Visit(node);
        }
    }
}
