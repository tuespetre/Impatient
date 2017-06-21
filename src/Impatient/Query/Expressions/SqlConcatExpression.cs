using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlConcatExpression : SqlExpression
    {
        public SqlConcatExpression(IEnumerable<Expression> segments)
        {
            Segments = segments?.ToArray() ?? throw new ArgumentNullException(nameof(segments));
        }

        public IEnumerable<Expression> Segments { get; }

        public override Type Type => typeof(string);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var segments = Segments.Select(visitor.Visit).ToArray();

            if (!segments.SequenceEqual(Segments))
            {
                return new SqlConcatExpression(segments);
            }

            return this;
        }
    }
}
