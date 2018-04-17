using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlParameterRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterAndExtensionCountingExpressionVisitor countingVisitor;
        private readonly List<ParameterExpression> targetParameters;

        public SqlParameterRewritingExpressionVisitor(IEnumerable<ParameterExpression> targetParameters)
        {
            this.targetParameters = targetParameters.ToList();
            countingVisitor = new ParameterAndExtensionCountingExpressionVisitor(targetParameters);
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
                    if (node.Type.IsScalarType())
                    {
                        var countingVisitor = new ParameterAndExtensionCountingExpressionVisitor(targetParameters);

                        countingVisitor.Visit(node);

                        if (countingVisitor.ParameterCount > 0 && countingVisitor.ExtensionCount == 0)
                        {
                            return new SqlParameterExpression(node);
                        }
                    }

                    return base.Visit(node);
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
                if (node == null)
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
