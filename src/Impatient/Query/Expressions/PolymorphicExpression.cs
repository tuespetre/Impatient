using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Expressions
{
    public class PolymorphicExpression : Expression
    {
        public sealed class TypeDescriptor : IEquatable<TypeDescriptor>
        {
            public TypeDescriptor(Type type, LambdaExpression test, LambdaExpression materializer)
            {
                Type = type ?? throw new ArgumentNullException(nameof(type));
                Test = test ?? throw new ArgumentNullException(nameof(test));
                Materializer = materializer ?? throw new ArgumentNullException(nameof(materializer));
            }

            public Type Type { get; }

            public LambdaExpression Test { get; }

            public LambdaExpression Materializer { get; }

            public bool Equals(TypeDescriptor other)
            {
                return other != null
                    && other.Type == Type
                    && other.Test == Test
                    && other.Materializer == Materializer;
            }
        }

        public PolymorphicExpression(
            Type type,
            Expression row,
            IEnumerable<TypeDescriptor> descriptors)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Row = row ?? throw new ArgumentNullException(nameof(row));
            Descriptors = descriptors ?? throw new ArgumentNullException(nameof(descriptors));
        }

        public Expression Row { get; }

        public IEnumerable<TypeDescriptor> Descriptors { get; }

        public PolymorphicExpression Upcast(Type type)
        {
            return new PolymorphicExpression(type, Row, Descriptors);
        }

        public PolymorphicExpression Filter(Type type)
        {
            if (!Type.IsAssignableFrom(type))
            {
                return new PolymorphicExpression(
                    type,
                    Row,
                    Enumerable.Empty<TypeDescriptor>());
            }

            return new PolymorphicExpression(
                type,
                Row,
                Descriptors
                    .Where(d => type.IsAssignableFrom(d.Type))
                    .ToArray());
        }

        public Expression Unwrap(Type type)
        {
            if (!Type.IsAssignableFrom(type))
            {
                return Constant(null, type);
            }

            var descriptor = Descriptors.FirstOrDefault(d => type.IsAssignableFrom(d.Type));

            if (descriptor != null)
            {
                return descriptor.Materializer.ExpandParameters(Row);
            }

            return Constant(null, type);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var row = visitor.Visit(Row);

            var descriptors = (from d in Descriptors
                               let test = visitor.VisitAndConvert(d.Test, nameof(VisitChildren))
                               let materializer = visitor.VisitAndConvert(d.Materializer, nameof(VisitChildren))
                               select new TypeDescriptor(d.Type, test, materializer)).ToArray();

            if (row != Row || !Descriptors.SequenceEqual(descriptors))
            {
                return new PolymorphicExpression(Type, row, descriptors);
            }

            return this;
        }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;
    }
}
