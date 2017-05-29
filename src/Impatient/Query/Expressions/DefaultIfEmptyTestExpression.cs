using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class DefaultIfEmptyTestExpression : AnnotationExpression
    {
        public DefaultIfEmptyTestExpression(Expression expression, AliasedTableExpression table) 
            : base(expression)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
        }

        public AliasedTableExpression Table { get; set; }

        protected override AnnotationExpression Recreate(Expression expression)
        {
            return new DefaultIfEmptyTestExpression(expression, Table);
        }
    }
}
