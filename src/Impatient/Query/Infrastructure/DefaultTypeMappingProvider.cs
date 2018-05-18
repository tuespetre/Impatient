using Impatient.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class DefaultTypeMappingProvider : ITypeMappingProvider
    {
        public ITypeMapping FindMapping(Type clrType)
        {
            var unwrapped = clrType.UnwrapNullableType();

            foreach (var mapping in defaultMappings)
            {
                if (mapping.TargetType == unwrapped)
                {
                    return mapping;
                }
            }

            return null;
        }

        private static readonly List<ITypeMapping> defaultMappings 
            = new List<ITypeMapping>
            {
                // Types that are more or less native (for SQL Server)
                new BooleanTypeMapping(),
                new ByteTypeMapping(),
                new ShortTypeMapping(),
                new Int32TypeMapping(),
                new Int64TypeMapping(),
                new SingleTypeMapping(),
                new DoubleTypeMapping(),
                new DecimalTypeMapping(),
                new DateTimeTypeMapping(),
                new DateTimeOffsetTypeMapping(),
                new TimeSpanTypeMapping(),
                new GuidTypeMapping(),
                new StringTypeMapping(),
                new ByteArrayTypeMapping(),
                // Other types
            };

        #region default mappings

        private class BooleanTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(bool);

            public Type SourceType => typeof(bool);

            public DbType? DbType => System.Data.DbType.Boolean;

            public string DbTypeName => "bit";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class ByteTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(byte);

            public Type SourceType => typeof(byte);

            public DbType? DbType => System.Data.DbType.Byte;

            public string DbTypeName => "tinyint";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class ShortTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(short);

            public Type SourceType => typeof(short);

            public DbType? DbType => System.Data.DbType.Int16;

            public string DbTypeName => "smallint";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class Int32TypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(int);

            public Type SourceType => typeof(int);

            public DbType? DbType => System.Data.DbType.Int32;

            public string DbTypeName => "int";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class Int64TypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(long);

            public Type SourceType => typeof(long);

            public DbType? DbType => System.Data.DbType.Int64;

            public string DbTypeName => "bigint";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class SingleTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(float);

            public Type SourceType => typeof(float);

            public DbType? DbType => System.Data.DbType.Single;

            public string DbTypeName => "real";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class DoubleTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(double);

            public Type SourceType => typeof(double);

            public DbType? DbType => System.Data.DbType.Double;

            public string DbTypeName => "float";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class DecimalTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(decimal);

            public Type SourceType => typeof(decimal);

            public DbType? DbType => System.Data.DbType.Decimal;

            public string DbTypeName => "decimal";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class DateTimeTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(DateTime);

            public Type SourceType => typeof(DateTime);

            public DbType? DbType => System.Data.DbType.DateTime2;

            public string DbTypeName => "datetime2";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class DateTimeOffsetTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(DateTimeOffset);

            public Type SourceType => typeof(DateTimeOffset);

            public DbType? DbType => System.Data.DbType.DateTimeOffset;

            public string DbTypeName => "datetimeoffset";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class TimeSpanTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(TimeSpan);

            public Type SourceType => typeof(TimeSpan);

            public DbType? DbType => System.Data.DbType.Time;

            public string DbTypeName => "time";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class GuidTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(Guid);

            public Type SourceType => typeof(Guid);

            public DbType? DbType => System.Data.DbType.Guid;

            public string DbTypeName => "uniqueidentifier";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class StringTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(string);

            public Type SourceType => typeof(string);

            public DbType? DbType => System.Data.DbType.String;

            public string DbTypeName => "nvarchar(max)";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        private class ByteArrayTypeMapping : ITypeMapping
        {
            public Type TargetType => typeof(byte[]);

            public Type SourceType => typeof(byte[]);

            public DbType? DbType => System.Data.DbType.Binary;

            public string DbTypeName => "varbinary(max)";

            public LambdaExpression TargetConversion => null;

            public LambdaExpression SourceConversion => null;
        }

        #endregion
    }
}
