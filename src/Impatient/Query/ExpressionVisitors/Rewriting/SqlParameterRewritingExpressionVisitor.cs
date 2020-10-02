using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlParameterRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly List<ParameterExpression> targetParameters;

        public SqlParameterRewritingExpressionVisitor(IEnumerable<ParameterExpression> targetParameters)
        {
            this.targetParameters = targetParameters.ToList();
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case null:
                case LambdaExpression _:
                {
                    return node;
                }

                case ClientProjectionExpression clientProjectionExpression:
                {
                    var parameters = clientProjectionExpression.ResultLambda.Parameters;

                    var index = targetParameters.Count;

                    targetParameters.AddRange(parameters);

                    var surrogate = new RelationalQueryExpressionVisitor(this);

                    var result
                        = new ClientProjectionExpression(
                            clientProjectionExpression.ServerProjection,
                            Expression.Lambda(
                                surrogate.Visit(clientProjectionExpression.ResultLambda.Body),
                                parameters));

                    targetParameters.RemoveRange(index, parameters.Count);

                    return result;
                }

                default:
                {
                    if (node.Type.IsScalarType() && IsEligibleForParameterization(node))
                    {
                        return new SqlParameterExpression(node);
                    }

                    return base.Visit(node);
                }
            }
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return node;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitedLeft = Visit(node.Left);
            var visitedRight = Visit(node.Right);

            var left = visitedLeft.UnwrapInnerExpression();
            var right = visitedRight.UnwrapInnerExpression();

            var leftMapping = FindTypeMapping(left);
            var rightMapping = FindTypeMapping(right);

            var madeChange = false;

            if (rightMapping is not null)
            {
                if (left is SqlParameterExpression leftParameter && leftParameter.TypeMapping is null)
                {
                    left
                        = new SqlParameterExpression(
                            leftParameter.Expression.UnwrapInnerExpression(),
                            leftParameter.IsNullable,
                            rightMapping);

                    madeChange = true;
                }
                else if (left is ConstantExpression && rightMapping.SourceConversion is not null)
                {
                    left
                        = new SqlParameterExpression(
                            left,
                            left.Type.IsNullableType() || !left.Type.GetTypeInfo().IsValueType,
                            rightMapping);

                    madeChange = true;
                }
            }

            if (leftMapping is not null)
            {
                if (right is SqlParameterExpression rightParameter && rightParameter.TypeMapping is null)
                {
                    right
                        = new SqlParameterExpression(
                            rightParameter.Expression.UnwrapInnerExpression(),
                            rightParameter.IsNullable,
                            leftMapping);

                    madeChange = true;
                }
                else if (right is ConstantExpression && leftMapping.SourceConversion is not null)
                {
                    right
                        = new SqlParameterExpression(
                            right,
                            right.Type.IsNullableType() || !right.Type.GetTypeInfo().IsValueType,
                            leftMapping);

                    madeChange = true;
                }
            }

            return madeChange
                ? node.UpdateWithConversion(left, right)
                : node.Update(visitedLeft, node.Conversion, visitedRight);
        }

        private bool IsEligibleForParameterization(Expression node)
        {
            var countingVisitor = new ParameterAndExtensionCountingExpressionVisitor(targetParameters);

            countingVisitor.Visit(node);

            return countingVisitor.ParameterCount > 0 && countingVisitor.ExtensionCount == 0;
        }

        private static ITypeMapping FindTypeMapping(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression
                when sqlColumnExpression.TypeMapping is not null:
                {
                    return sqlColumnExpression.TypeMapping;
                }

                case SqlParameterExpression sqlParameterExpression
                when sqlParameterExpression.TypeMapping is not null:
                {
                    return sqlParameterExpression.TypeMapping;
                }

                case SqlAggregateExpression sqlAggregateExpression:
                {
                    return FindTypeMapping(sqlAggregateExpression.Expression);
                }

                case UnaryExpression unaryExpression
                when unaryExpression.NodeType == ExpressionType.Convert:
                {
                    return FindTypeMapping(unaryExpression.Operand);
                }

                default:
                {
                    return null;
                }
            }
        }

        private class ParameterAndExtensionCountingExpressionVisitor : ExpressionVisitor
        {
            private readonly IEnumerable<ParameterExpression> targetParameters;

            public int ParameterCount { get; private set; }

            public int ExtensionCount { get; private set; }

            public ParameterAndExtensionCountingExpressionVisitor(IEnumerable<ParameterExpression> targetParameters)
            {
                this.targetParameters = targetParameters;
            }

            public override Expression Visit(Expression node)
            {
                if (node is null)
                {
                    return node;
                }

                switch (node.NodeType)
                {
                    case ExpressionType.Parameter
                    when targetParameters.Contains(node):
                    {
                        ParameterCount++;
                        break;
                    }

                    case ExpressionType.Extension:
                    {
                        ExtensionCount++;
                        break;
                    }
                }

                return base.Visit(node);
            }
        }

        private class RelationalQueryExpressionVisitor : ExpressionVisitor
        {
            private readonly ExpressionVisitor visitor;

            public RelationalQueryExpressionVisitor(ExpressionVisitor visitor)
            {
                this.visitor = visitor;
            }

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case RelationalQueryExpression _:
                    {
                        return visitor.Visit(node);
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
