using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    internal static class RowNumberTuple
    {
        public static Expression Create(Expression projection, Expression rowNumberExpression)
        {
            var rowNumberTupleType = typeof(RowNumberTuple<>).MakeGenericType(projection.Type);

            return Expression.New(
                rowNumberTupleType.GetTypeInfo().DeclaredConstructors.Single(),
                new[]
                {
                    projection,
                    rowNumberExpression,
                },
                new[]
                {
                    rowNumberTupleType.GetRuntimeField("Projection"),
                    rowNumberTupleType.GetRuntimeField("RowNumber"),
                });
        }
    }

    internal struct RowNumberTuple<TProjection>
    {
        public RowNumberTuple(TProjection projection, int rowNumber)
        {
            Projection = projection;
            RowNumber = rowNumber;
        }

        [PathSegmentName(null)]
        public readonly TProjection Projection;

        [PathSegmentName("$rownumber")]
        public readonly int RowNumber;
    }
}
