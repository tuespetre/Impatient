using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SubqueryTableExpression : AliasedTableExpression
    {
        public SubqueryTableExpression(SelectExpression subquery, string alias) : base(alias, subquery.Type)
        {
            Subquery = subquery ?? throw new ArgumentNullException(nameof(subquery));
        }

        public SelectExpression Subquery { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var subquery = visitor.VisitAndConvert(Subquery, nameof(VisitChildren));

            if (subquery != Subquery)
            {
                return new SubqueryTableExpression(subquery, Alias);
            }

            return this;
        }
    }
}
