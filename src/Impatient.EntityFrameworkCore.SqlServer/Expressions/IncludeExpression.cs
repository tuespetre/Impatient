using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class IncludeExpression : AnnotationExpression
    {
        public IncludeExpression(
            Expression expression, 
            IEnumerable<Expression> includes, 
            IEnumerable<IEnumerable<INavigation>> paths) 
            : this(
                  expression, 
                  new ReadOnlyCollection<Expression>(includes.ToArray()), 
                  paths.Select(p => p.ToImmutableArray()).ToImmutableArray())
        {
        }

        public IncludeExpression(
            Expression expression, 
            ReadOnlyCollection<Expression> includes,
            ImmutableArray<ImmutableArray<INavigation>> paths) 
            : base(expression)
        {
            if (includes.Count != paths.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(paths));
            }

            Includes = includes;
            Paths = paths;
        }

        public ReadOnlyCollection<Expression> Includes { get; }

        public ImmutableArray<ImmutableArray<INavigation>> Paths { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);
            var includes = visitor.Visit(Includes);

            if (expression != Expression || !includes.SequenceEqual(Includes))
            {
                return new IncludeExpression(expression, includes, Paths);
            }

            return this;
        }

        public override int GetSemanticHashCode() => 0;
    }
}
