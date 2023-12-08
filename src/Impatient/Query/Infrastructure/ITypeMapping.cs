using System;
using System.Data;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface ITypeMapping
    {
        /// <summary>
        /// The <see cref="Type"/> to be bound in the data model.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// The <see cref="Type"/> to be received from the underlying database provider.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// The <see cref="DbType"/> used by the underlying database provider.
        /// </summary>
        DbType? DbType { get; }

        /// <summary>
        /// A <see cref="string"/> to be used at the database when performing casts and so forth.
        /// </summary>
        string DbTypeName { get; }

        /// <summary>
        /// A <see cref="LambdaExpression"/> used to convert from the <see cref="SourceType"/> to the <see cref="TargetType"/>.
        /// </summary>
        LambdaExpression TargetConversion { get; }


        /// <summary>
        /// A <see cref="LambdaExpression"/> used to convert from the <see cref="TargetType"/> to the <see cref="SourceType"/>.
        /// </summary>
        LambdaExpression SourceConversion { get; }
    }
}
