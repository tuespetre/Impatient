using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlParameterRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterAndExtensionCountingExpressionVisitor countingVisitor
            = new ParameterAndExtensionCountingExpressionVisitor();

        public override Expression Visit(Expression node)
        {
            if (node is null || node is LambdaExpression)
            {
                return node;
            }

            if (node.Type.IsScalarType())
            {
                countingVisitor.Visit(node);

                if (countingVisitor.ParameterCount > 0 && countingVisitor.ExtensionCount == 0)
                {
                    return new SqlParameterExpression(node);
                }

                countingVisitor.ParameterCount = 0;
                countingVisitor.ExtensionCount = 0;
            }

            return base.Visit(node);
        }

        private class ParameterAndExtensionCountingExpressionVisitor : ExpressionVisitor
        {
            public int ParameterCount;
            public int ExtensionCount;

            public override Expression Visit(Expression node)
            {
                if (node == null)
                {
                    return node;
                }

                switch (node.NodeType)
                {
                    case ExpressionType.Parameter:
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
    }
}
