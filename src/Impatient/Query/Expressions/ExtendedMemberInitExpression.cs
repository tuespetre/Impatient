using Impatient.Extensions;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Expressions;

public class ExtendedMemberInitExpression : Expression, ISemanticHashCodeProvider
{
    public ExtendedNewExpression NewExpression { get; }

    public ReadOnlyCollection<Expression> Arguments { get; }

    public ReadOnlyCollection<MemberInfo> ReadableMembers { get; }

    public ReadOnlyCollection<MemberInfo> WritableMembers { get; }

    public PropertyInfo Indexer { get; }

    public ReadOnlyCollection<string> IndexerKeys { get; }

    public ReadOnlyCollection<Expression> IndexerValues { get; }

    public override Type Type { get; }

    public override ExpressionType NodeType => ExpressionType.Extension;

    public override bool CanReduce => true;

    public override Expression Reduce()
    {
        var reducedNewExpression = NewExpression.Reduce();

        if (reducedNewExpression.NodeType is ExpressionType.New && Indexer is null)
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
            var expressions = new Expression[Arguments.Count + IndexerKeys.Count + 2];

            expressions[0] = Assign(variable, reducedNewExpression);

            for (var i = 0; i < Arguments.Count; i++)
            {
                expressions[i + 1] = Assign(MakeMemberAccess(variable, WritableMembers[i]), Arguments[i]);
            }

            for (var i = 0; i < IndexerKeys.Count; i++)
            {
                expressions[i + 1 + Arguments.Count] = Assign(MakeIndex(variable, Indexer, [Constant(IndexerKeys[i])]), Convert(IndexerValues[i], Indexer.PropertyType));
            }

            expressions[Arguments.Count + IndexerKeys.Count + 1] = Convert(variable, Type);

            return Block([variable], expressions);
        }
    }

    public ExtendedMemberInitExpression(
        Type explicitType,
        ExtendedNewExpression newExpression,
        IEnumerable<Expression> arguments,
        IEnumerable<MemberInfo> readableMembers,
        IEnumerable<MemberInfo> writableMembers,
        PropertyInfo indexer,
        IEnumerable<string> indexerKeys,
        IEnumerable<Expression> indexerValues)
        : this(newExpression, arguments, readableMembers, writableMembers, indexer, indexerKeys, indexerValues)
    {
        if (!explicitType.IsAssignableFrom(newExpression.Type))
        {
            throw new ArgumentException("Type must be assignable from the ExtendedNewExpression's Type", nameof(explicitType));
        }

        Type = explicitType;
    }

    public ExtendedMemberInitExpression(
        ExtendedNewExpression newExpression,
        IEnumerable<Expression> arguments,
        IEnumerable<MemberInfo> readableMembers,
        IEnumerable<MemberInfo> writableMembers,
        PropertyInfo indexer,
        IEnumerable<string> indexerKeys,
        IEnumerable<Expression> indexerValues)
    {
        arguments = arguments?.ToArray();
        readableMembers = readableMembers?.ToArray();
        writableMembers = writableMembers?.ToArray();
        indexerKeys = indexerKeys?.ToArray();
        indexerValues = indexerValues?.ToArray();

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

        if (indexer is null)
        {
            if (indexerKeys.Any())
            {
                throw new ArgumentException("Indexer keys cannot be supplied without an indexer.");
            }

            if (indexerValues.Any())
            {
                throw new ArgumentException("Indexer values cannot be supplied without an indexer.");
            }

            IndexerKeys = new([]);
            IndexerValues = new([]);
        }
        else
        {
            Indexer = indexer;

            var indexParameters = indexer.GetIndexParameters();

            if (indexParameters.Length != 1 || indexParameters[0].ParameterType != typeof(string))
            {
                throw new ArgumentException("Indexer must be a property with one index parameter of type string.");
            }

            IndexerKeys = new(indexerKeys?.ToArray() ?? throw new ArgumentNullException(nameof(indexerKeys)));
            IndexerValues = new(indexerValues?.ToArray() ?? throw new ArgumentNullException(nameof(indexerValues)));

            if (IndexerKeys.Count != IndexerValues.Count)
            {
                throw new ArgumentException("Indexer keys and values must have the same cardinality.");
            }

            for (var i = 0; i < IndexerKeys.Count; i++)
            {
                if (IndexerKeys[i] is null)
                {
                    throw new ArgumentException($"IndexerKeys cannot be null. Element {i} is null.");
                }

                if (IndexerValues[i] is null)
                {
                    throw new ArgumentException($"IndexerValues cannot be null. Element {i} is null.");
                }

                if (!IndexerValues[i].Type.IsAssignableTo(indexer.PropertyType))
                {
                    throw new ArgumentException($"IndexerValues must be of valid type for the indexer. Element {i} is of invalid type.");
                }
            }
        }
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        ArgumentNullException.ThrowIfNull(visitor);

        var newExpression = visitor.VisitAndConvert(NewExpression, nameof(VisitChildren));
        var arguments = visitor.Visit(Arguments);
        var indexerValues = visitor.Visit(IndexerValues);

        return Update(newExpression, arguments, IndexerKeys, indexerValues);
    }

    public ExtendedMemberInitExpression Update(
        ExtendedNewExpression newExpression,
        IEnumerable<Expression> arguments,
        IEnumerable<string> indexerKeys,
        IEnumerable<Expression> indexerValues)
    {
        ArgumentNullException.ThrowIfNull(newExpression);
        ArgumentNullException.ThrowIfNull(arguments);

        if (newExpression != NewExpression
            || !arguments.SequenceEqual(Arguments)
            || !indexerKeys.SequenceEqual(IndexerKeys)
            || !indexerValues.SequenceEqual(IndexerValues))
        {
            return new ExtendedMemberInitExpression(
                Type,
                newExpression,
                arguments,
                ReadableMembers,
                WritableMembers,
                Indexer,
                indexerKeys,
                indexerValues);
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

            for (var i = 0; i < IndexerKeys.Count; i++)
            {
                hash = (hash * 16777619) ^ IndexerKeys[i].GetHashCode();
                hash = (hash * 16777619) ^ IndexerValues[i].GetHashCode();
            }

            return hash;
        }
    }
}
