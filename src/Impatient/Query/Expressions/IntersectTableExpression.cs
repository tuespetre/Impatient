using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class IntersectTableExpression : SetOperatorTableExpression
    {
        public IntersectTableExpression(SelectExpression set1, SelectExpression set2)
            : base(set1, set2)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var set1 = visitor.VisitAndConvert(Set1, nameof(VisitChildren));
            var set2 = visitor.VisitAndConvert(Set2, nameof(VisitChildren));

            if (set1 != Set1 || set2 != Set2)
            {
                return new IntersectTableExpression(set1, set2);
            }

            return this;
        }
    }
}
