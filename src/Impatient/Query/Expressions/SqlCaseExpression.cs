using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlCaseExpression : SqlExpression
    {
        public SqlCaseExpression(IEnumerable<Expression> whens, IEnumerable<Expression> thens, Expression @else, Type type)
        {
            ArgumentNullException.ThrowIfNull(whens);
            ArgumentNullException.ThrowIfNull(thens);

            Whens = whens.ToArray();
            Thens = thens.ToArray();

            if (!Whens.Any() || Whens.Count() != Thens.Count())
            {
                throw new InvalidOperationException();
            }

            Else = @else;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public IEnumerable<Expression> Whens { get; }

        public IEnumerable<Expression> Thens { get; }

        public Expression Else { get; }

        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var whens = Whens.Select(visitor.Visit).ToArray();
            var thens = Thens.Select(visitor.Visit).ToArray();
            var @else = visitor.Visit(Else);

            if (!whens.SequenceEqual(Whens) || !thens.SequenceEqual(Thens) || @else != Else)
            {
                return new SqlCaseExpression(whens, thens, @else, Type);
            }

            return this;
        }
    }
}
