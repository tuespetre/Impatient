using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class RelationalGroupingExpression : Expression
    {
        public RelationalGroupingExpression(
            EnumerableRelationalQueryExpression underlyingQuery, 
            Expression keySelector, 
            Expression elementSelector)
        {
            UnderlyingQuery = underlyingQuery ?? throw new ArgumentNullException(nameof(underlyingQuery));
            KeySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
            ElementSelector = elementSelector ?? throw new ArgumentNullException(nameof(elementSelector));

            Type = typeof(IGrouping<,>).MakeGenericType(keySelector.Type, elementSelector.Type);
        }

        public EnumerableRelationalQueryExpression UnderlyingQuery { get; }

        public Expression KeySelector { get; }

        public Expression ElementSelector { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var underlyingQuery = visitor.VisitAndConvert(UnderlyingQuery, nameof(VisitChildren));
            var keySelector = visitor.VisitAndConvert(KeySelector, nameof(VisitChildren));
            var elementSelector = visitor.VisitAndConvert(ElementSelector, nameof(VisitChildren));

            if (underlyingQuery != UnderlyingQuery || keySelector != KeySelector || elementSelector != ElementSelector)
            {
                return new RelationalGroupingExpression(underlyingQuery, keySelector, elementSelector);
            }

            return this;
        }
    }
}
