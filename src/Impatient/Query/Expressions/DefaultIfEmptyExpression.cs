using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class DefaultIfEmptyExpression : MetaAliasExpression
    {
        public DefaultIfEmptyExpression(Expression expression, SqlAliasExpression aliasExpression) : base(expression, aliasExpression)
        {
        }

        protected override AnnotationExpression Recreate(Expression expression)
        {
            return new DefaultIfEmptyExpression(expression, AliasExpression);
        }
    }
}
