using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class IncludeExpression : ExtraPropertiesExpression
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
            Names = new ReadOnlyCollection<string>(Enumerable.Range(0, paths.Length).Select(i => $"Include_{i}").ToArray());
        }

        public ReadOnlyCollection<Expression> Includes { get; }

        public ImmutableArray<ImmutableArray<INavigation>> Paths { get; }

        public override ReadOnlyCollection<string> Names { get; }

        public override ReadOnlyCollection<Expression> Properties => Includes;

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

        public override IEnumerable<MemberInfo> GetMemberPath(int index)
        {
            return Paths[index].Select(p => p.GetSemanticReadableMemberInfo());
        }

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = comparer.GetHashCode(Expression);

                for (var i = 0; i < Includes.Count; i++)
                {
                    hash = (hash * 16777619) ^ comparer.GetHashCode(Includes[i]);
                }

                return hash;
            }
        }

        public override ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties)
        {
            if (expression != Expression || !properties.SequenceEqual(Properties))
            {
                return new IncludeExpression(expression, new ReadOnlyCollection<Expression>(properties.ToArray()), Paths);
            }

            return this;
        }
    }
}
