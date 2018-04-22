using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class RelationalNullSemanticsComposingExpressionVisitor : ExpressionVisitor
    {
        private readonly RelationalNullSemanticsTargetingExpressionVisitor targetingExpressionVisitor
            = new RelationalNullSemanticsTargetingExpressionVisitor();

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case QueryOptionsExpression queryOptionsExpression:
                {
                    if (queryOptionsExpression.UseRelationalNullSemantics)
                    {
                        return targetingExpressionVisitor.Visit(node);
                    }

                    return node;
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private class RelationalNullSemanticsTargetingExpressionVisitor : ExpressionVisitor
        {
            private readonly RelationalNullSemanticsApplyingExpressionVisitor applyingExpressionVisitor
                = new RelationalNullSemanticsApplyingExpressionVisitor();

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case RelationalQueryExpression relationalQueryExpression:
                    {
                        return applyingExpressionVisitor.Visit(relationalQueryExpression);
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }

        private class RelationalNullSemanticsApplyingExpressionVisitor : ExpressionVisitor
        {
            public override Expression Visit(Expression node)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    {
                        return new SqlColumnAndParameterNullabilityExpressionVisitor()
                            .Visit(base.Visit(node));
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }

        private class SqlColumnAndParameterNullabilityExpressionVisitor : ExpressionVisitor
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
                            isNullable: false);
                    }

                    case SqlParameterExpression sqlParameterExpression:
                    {
                        return new SqlParameterExpression(
                            sqlParameterExpression.Expression,
                            isNullable: false);
                    }

                    case RelationalQueryExpression relationalQueryExpression:
                    {
                        return relationalQueryExpression;
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
