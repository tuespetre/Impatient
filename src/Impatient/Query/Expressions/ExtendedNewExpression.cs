using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Expressions
{
    public class ExtendedNewExpression : Expression, ISemanticHashCodeProvider
    {
        public ConstructorInfo Constructor { get; }

        public ReadOnlyCollection<Expression> Arguments { get; }

        public ReadOnlyCollection<MemberInfo> ReadableMembers { get; }

        public ReadOnlyCollection<MemberInfo> WritableMembers { get; }

        public override Type Type => Constructor.DeclaringType;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        public override Expression Reduce() => ReduceToNewExpression();

        public NewExpression ReduceToNewExpression() => New(Constructor, Arguments);

        public ExtendedNewExpression(Type type)
        {
            var constructor = type.GetConstructor(Array.Empty<Type>());

            if (constructor is null)
            {
                throw new ArgumentException("Could not find a parameterless public constructor for the given type", nameof(type));
            }
        }

        public ExtendedNewExpression(
            ConstructorInfo constructor, 
            IEnumerable<Expression> arguments,
            IEnumerable<MemberInfo> readableMembers,
            IEnumerable<MemberInfo> writableMembers)
        {
            // TODO: Argument validation

            Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            Arguments = new ReadOnlyCollection<Expression>(arguments?.ToArray() ?? throw new ArgumentNullException(nameof(arguments)));
            ReadableMembers = new ReadOnlyCollection<MemberInfo>(readableMembers?.ToArray() ?? throw new ArgumentNullException(nameof(readableMembers)));
            WritableMembers = new ReadOnlyCollection<MemberInfo>(writableMembers?.ToArray() ?? throw new ArgumentNullException(nameof(writableMembers)));
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // TODO: Argument validation

            var arguments = visitor.Visit(Arguments);

            if (arguments != Arguments)
            {
                return new ExtendedNewExpression(Constructor, arguments, ReadableMembers, WritableMembers);
            }

            return this;
        }

        public virtual ExtendedNewExpression Update(IEnumerable<Expression> arguments)
        {
            // TODO: Argument validation

            if (!arguments.SequenceEqual(Arguments))
            {
                return new ExtendedNewExpression(Constructor, arguments, ReadableMembers, WritableMembers);
            }

            return this;
        }

        public virtual int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = NodeType.GetHashCode();

                for (var i = 0; i < Arguments.Count; i++)
                {
                    hash = (hash * 16777619) ^ Arguments[i].GetHashCode();
                    hash = (hash * 16777619) ^ ReadableMembers[i]?.GetHashCode() ?? 0;
                    hash = (hash * 16777619) ^ WritableMembers[i]?.GetHashCode() ?? 0;
                }

                return hash;
            }
        }
    }
}
