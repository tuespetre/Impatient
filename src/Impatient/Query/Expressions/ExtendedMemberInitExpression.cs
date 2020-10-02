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
    public class ExtendedMemberInitExpression : Expression, ISemanticHashCodeProvider
    {
        public ExtendedNewExpression NewExpression { get; }

        public ReadOnlyCollection<Expression> Arguments { get; }

        public ReadOnlyCollection<MemberInfo> ReadableMembers { get; }

        public ReadOnlyCollection<MemberInfo> WritableMembers { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        public override Expression Reduce()
        {
            var reducedNewExpression = NewExpression.Reduce();

            if (reducedNewExpression.NodeType == ExpressionType.New)
            {
                return MemberInit(
                    (NewExpression)reducedNewExpression,
                    from i in Enumerable.Range(0, Arguments.Count)
                    let a = Arguments[i]
                    let m = WritableMembers[i]
                    // https://github.com/dotnet/runtime/issues/42966
                    let needsWrap = m.GetMemberType().Equals(typeof(object)) && !a.Type.Equals(typeof(object))
                    let b = needsWrap ? Convert(a, typeof(object)) : a
                    select Bind(m, b));
            }
            else
            {
                var variable = Variable(reducedNewExpression.Type, "instance");
                var expressions = new Expression[Arguments.Count + 2];

                expressions[0] = Assign(variable, reducedNewExpression);

                for (var i = 0; i < Arguments.Count; i++)
                {
                    expressions[i + 1] = Assign(MakeMemberAccess(variable, WritableMembers[i]), Arguments[i]);
                }

                expressions[Arguments.Count + 1] = Convert(variable, Type);

                return Block(new[] { variable }, expressions);
            }
        }

        public ExtendedMemberInitExpression(
            Type explicitType,
            ExtendedNewExpression newExpression,
            IEnumerable<Expression> arguments,
            IEnumerable<MemberInfo> readableMembers,
            IEnumerable<MemberInfo> writableMembers)
            : this(newExpression, arguments, readableMembers, writableMembers)
        {
            Type = explicitType;

            if (!Type.IsAssignableFrom(newExpression.Type))
            {
                throw new ArgumentException();
            }
        }

        public ExtendedMemberInitExpression(
            ExtendedNewExpression newExpression,
            IEnumerable<Expression> arguments,
            IEnumerable<MemberInfo> readableMembers,
            IEnumerable<MemberInfo> writableMembers)
        {
            arguments = arguments?.ToArray();
            readableMembers = readableMembers?.ToArray();
            writableMembers = writableMembers?.ToArray();

            Type = newExpression.Type;
            NewExpression = newExpression ?? throw new ArgumentNullException(nameof(newExpression));
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

                if (ReadableMembers[i] is null)
                {
                    throw new ArgumentException($"ReadableMembers cannot be null. Element {i} is null.");
                }

                if (WritableMembers[i] is null)
                {
                    throw new ArgumentException($"WritableMembers cannot be null. Element {i} is null.");
                }

                if (!ReadableMembers[i].GetMemberType().IsAssignableFrom(Arguments[i].Type))
                {
                    throw new ArgumentException($"Arguments and ReadableMembers must match. Elements at index {i} do not match.");
                }

                if (!WritableMembers[i].GetMemberType().IsAssignableFrom(Arguments[i].Type))
                {
                    throw new ArgumentException($"Arguments and WritableMembers must match. Elements at index {i} do not match.");
                }

                if (!ReadableMembers[i].DeclaringType.IsAssignableFrom(newExpression.Type))
                {
                    throw new ArgumentException($"ReadableMembers must be valid for the NewExpression's type. Element {i} is not valid.");
                }

                if (!WritableMembers[i].DeclaringType.IsAssignableFrom(newExpression.Type))
                {
                    throw new ArgumentException($"WritableMembers must be valid for the NewExpression's type. Element {i} is not valid.");
                }
            }
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            var newExpression = visitor.VisitAndConvert(NewExpression, nameof(VisitChildren));
            var arguments = visitor.Visit(Arguments);

            return Update(newExpression, arguments);
        }

        public ExtendedMemberInitExpression Update(ExtendedNewExpression newExpression, IEnumerable<Expression> arguments)
        {
            if (newExpression == null)
            {
                throw new ArgumentNullException(nameof(newExpression));
            }

            if (arguments == null)
            {
                throw new ArgumentNullException(nameof(arguments));
            }

            if (newExpression != NewExpression || arguments != Arguments)
            {
                return new ExtendedMemberInitExpression(Type, newExpression, arguments, ReadableMembers, WritableMembers);
            }

            return this;
        }

        public int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = comparer.GetHashCode(NewExpression);

                for (var i = 0; i < Arguments.Count; i++)
                {
                    hash = (hash * 16777619) ^ Arguments[i].GetHashCode();
                    hash = (hash * 16777619) ^ ReadableMembers[i].GetHashCode();
                    hash = (hash * 16777619) ^ WritableMembers[i].GetHashCode();
                }

                return hash;
            }
        }
    }
}
