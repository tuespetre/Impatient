using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class QueryFilterRepointingExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression dbContextParameter;

        public QueryFilterRepointingExpressionVisitor(ParameterExpression dbContextParameter)
        {
            this.dbContextParameter = dbContextParameter;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Type.IsAssignableFrom(dbContextParameter.Type))
            {
                var inner = node.Expression;

                while (inner is MemberExpression memberExpression)
                {
                    inner = memberExpression.Expression;
                }

                if (inner is ConstantExpression)
                {
                    return dbContextParameter;
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsAssignableFrom(dbContextParameter.Type))
            {
                return dbContextParameter;
            }

            return base.VisitConstant(node);
        }
    }
}
