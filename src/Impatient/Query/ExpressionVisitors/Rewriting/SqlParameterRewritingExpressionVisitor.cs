using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting;

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
            case LambdaExpression:
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

        var madeChange = false;

        ProcessBinaryOperand(right, ref left, ref madeChange);
        ProcessBinaryOperand(left, ref right, ref madeChange);

        return madeChange
            ? node.UpdateWithConversion(left, right)
            : node.Update(visitedLeft, node.Conversion, visitedRight);
    }

    private void ProcessBinaryOperand(Expression thisNode, ref Expression thatNode, ref bool madeChange)
    {
        var thisMapping = FindTypeMapping(thisNode);

        if (thisMapping is null)
        {
            return;
        }

        if (thatNode is SqlParameterExpression parameter 
            && parameter.TypeMapping is null)
        {
            thatNode
                = new SqlParameterExpression(
                    parameter.Expression.UnwrapInnerExpression(),
                    parameter.IsNullable,
                    thisMapping);

            madeChange = true;
        }
        else if (thatNode is ConstantExpression 
            && thisMapping.SourceConversion is not null 
            && thatNode.Type.IsAssignableTo(thisMapping.TargetType))
        {
            thatNode
                = new SqlParameterExpression(
                    thatNode,
                    thatNode.Type.IsNullableType() || !thatNode.Type.GetTypeInfo().IsValueType,
                    thisMapping);

            madeChange = true;
        }
    }

    private bool IsEligibleForParameterization(Expression node)
    {
        if (node.NodeType is ExpressionType.Extension)
        {
            return false;
        }

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
                case RelationalQueryExpression:
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
