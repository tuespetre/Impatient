using Impatient.Extensions;
using System;
using System.Collections.Generic;

namespace Impatient.Query.Infrastructure
{
    public class DefaultTypeMapper : ITypeMapper
    {
        public ITypeMapping FindMapping(Type clrType)
        {
            var unwrapped = clrType.UnwrapNullableType();

            foreach (var mapping in defaultMappings)
            {
                if (mapping.ClrType == unwrapped)
                {
                    return mapping;
                }
            }

            return null;
        }

        private static readonly List<ITypeMapping> defaultMappings 
            = new List<ITypeMapping>
            {
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
            };

        #region default mappings

        private class BooleanTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(bool);

            public string DbType => "bit";
        }

        private class ByteTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(byte);

            public string DbType => "tinyint";
        }

        private class ShortTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(short);

            public string DbType => "smallint";
        }

        private class Int32TypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(int);

            public string DbType => "int";
        }

        private class Int64TypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(long);

            public string DbType => "bigint";
        }

        private class SingleTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(float);

            public string DbType => "real";
        }

        private class DoubleTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(double);

            public string DbType => "float";
        }

        private class DecimalTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(decimal);

            public string DbType => "decimal";
        }

        private class DateTimeTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(DateTime);

            public string DbType => "datetime2";
        }

        private class DateTimeOffsetTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(DateTimeOffset);

            public string DbType => "datetimeoffset";
        }

        private class TimeSpanTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(TimeSpan);

            public string DbType => "time";
        }

        private class GuidTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(Guid);

            public string DbType => "uniqueidentifier";
        }

        private class StringTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(string);

            public string DbType => "nvarchar(max)";
        }

        private class ByteArrayTypeMapping : ITypeMapping
        {
            public Type ClrType => typeof(byte[]);

            public string DbType => "varbinary(max)";
        }

        #endregion
    }
}
