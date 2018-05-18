using System;
using System.Data;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class AdHocTypeMapping : ITypeMapping
    {
        public AdHocTypeMapping(
            Type targetType, 
            Type sourceType, 
            DbType? dbType, 
            string dbTypeName, 
            LambdaExpression targetConversion,
            LambdaExpression sourceConversion)
        {
            TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            DbType = dbType;
            DbTypeName = dbTypeName ?? throw new ArgumentNullException(nameof(dbTypeName));
            TargetConversion = targetConversion;
            SourceConversion = sourceConversion;
        }

        public Type TargetType { get; }

        public Type SourceType { get; }

        public DbType? DbType { get; }

        public string DbTypeName { get; }

        public LambdaExpression TargetConversion { get; }

        public LambdaExpression SourceConversion { get; }
    }
}
