using Impatient.Extensions;
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

            Constructor = constructor;
        }

        public ExtendedNewExpression(
            ConstructorInfo constructor,
            IEnumerable<Expression> arguments,
            IEnumerable<MemberInfo> readableMembers,
            IEnumerable<MemberInfo> writableMembers)
        {
            Constructor = constructor ?? throw new ArgumentNullException(nameof(constructor));
            Arguments = new ReadOnlyCollection<Expression>(arguments?.ToArray() ?? throw new ArgumentNullException(nameof(arguments)));
            ReadableMembers = new ReadOnlyCollection<MemberInfo>(readableMembers?.ToArray() ?? throw new ArgumentNullException(nameof(readableMembers)));
            WritableMembers = new ReadOnlyCollection<MemberInfo>(writableMembers?.ToArray() ?? throw new ArgumentNullException(nameof(writableMembers)));

            if (ReadableMembers.Count != Arguments.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(readableMembers));
            }

            if (WritableMembers.Count != Arguments.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(writableMembers));
            }

            for (var i = 0; i < Arguments.Count; i++)
            {
                if (Arguments[i] is null)
                {
                    throw new ArgumentException($"Arguments cannot be null. Element {i} is null.");
                }

                if (!(ReadableMembers[i] is null))
                {
                    if (!(ReadableMembers[i].GetMemberType().IsAssignableFrom(Arguments[i].Type)))
                    {
                        throw new ArgumentException($"Arguments and ReadableMembers must match. Elements at index {i} do not match.");
                    }

                    if (!(ReadableMembers[i].DeclaringType.IsAssignableFrom(Constructor.DeclaringType)))
                    {
                        throw new ArgumentException($"ReadableMembers must be valid for the NewExpression's type. Element {i} is not valid.");
                    }
                }
                
                if (!(WritableMembers[i] is null))
                {
                    if (!(WritableMembers[i].GetMemberType().IsAssignableFrom(Arguments[i].Type)))
                    {
                        throw new ArgumentException($"Arguments and WritableMembers must match. Elements at index {i} do not match.");
                    }

                    if (!(WritableMembers[i].DeclaringType.IsAssignableFrom(Constructor.DeclaringType)))
                    {
                        throw new ArgumentException($"WritableMembers must be valid for the NewExpression's type. Element {i} is not valid.");
                    }
                }
            }
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor is null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            var arguments = visitor.Visit(Arguments);

            if (arguments != Arguments)
            {
                return new ExtendedNewExpression(Constructor, arguments, ReadableMembers, WritableMembers);
            }

            return this;
        }

        public virtual ExtendedNewExpression Update(IEnumerable<Expression> arguments)
        {
            if (arguments is null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

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
