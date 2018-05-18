using System;
using System.Data;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface ITypeMapping
    {
        Type TargetType { get; }

        Type SourceType { get; }

        DbType? DbType { get; }

        string DbTypeName { get; }

        LambdaExpression TargetConversion { get; }

        LambdaExpression SourceConversion { get; }
    }
}
