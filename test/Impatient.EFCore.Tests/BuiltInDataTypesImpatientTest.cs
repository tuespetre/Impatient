using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class BuiltInDataTypesImpatientTest : BuiltInDataTypesTestBase<BuiltInDataTypesImpatientTest.BuiltInDataTypesImpatientFixture>
    {
        public BuiltInDataTypesImpatientTest(BuiltInDataTypesImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public override void Can_insert_and_read_back_all_nullable_data_types_with_values_set_to_null()
        {
            using (var context = CreateContext())
            {
                context.Set<BuiltInNullableDataTypes>().Add(
                    new BuiltInNullableDataTypes
                    {
                        Id = 100,
                        PartitionId = 100
                    });

                Assert.Equal(1, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var dt = context.Set<BuiltInNullableDataTypes>().Where(ndt => ndt.Id == 100).ToList().Single();

                Assert.Null(dt.TestString);
                Assert.Null(dt.TestByteArray);
                Assert.Null(dt.TestNullableInt16);
                Assert.Null(dt.TestNullableInt32);
                Assert.Null(dt.TestNullableInt64);
                Assert.Null(dt.TestNullableDouble);
                Assert.Null(dt.TestNullableDecimal);
                Assert.Null(dt.TestNullableDateTime);
                Assert.Null(dt.TestNullableDateTimeOffset);
                Assert.Null(dt.TestNullableTimeSpan);
                Assert.Null(dt.TestNullableSingle);
                Assert.Null(dt.TestNullableBoolean);
                Assert.Null(dt.TestNullableByte);
                Assert.Null(dt.TestNullableUnsignedInt16);
                Assert.Null(dt.TestNullableUnsignedInt32);
                Assert.Null(dt.TestNullableUnsignedInt64);
                Assert.Null(dt.TestNullableCharacter);
                Assert.Null(dt.TestNullableSignedByte);
                Assert.Null(dt.Enum64);
                Assert.Null(dt.Enum32);
                Assert.Null(dt.Enum16);
                Assert.Null(dt.Enum8);
                Assert.Null(dt.EnumU64);
                Assert.Null(dt.EnumU32);
                Assert.Null(dt.EnumU16);
                Assert.Null(dt.EnumS8);
            }
        }

        public class BuiltInDataTypesImpatientFixture : BuiltInDataTypesFixtureBase
        {
            public override bool StrictEquality => true;

            public override bool SupportsAnsi => true;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
            
            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedDataTypes>(
                    b =>
                    {
                        b.HasKey(e => e.Int);
                        b.Property(e => e.Int).ValueGeneratedNever();
                    });

                modelBuilder.Entity<MappedNullableDataTypes>(
                    b =>
                    {
                        b.HasKey(e => e.Int);
                        b.Property(e => e.Int).ValueGeneratedNever();
                    });

                modelBuilder.Entity<MappedDataTypesWithIdentity>();
                modelBuilder.Entity<MappedNullableDataTypesWithIdentity>();

                modelBuilder.Entity<MappedSizedDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>()
                    .Property(e => e.Id)
                    .ValueGeneratedNever();

                MakeRequired<MappedDataTypes>(modelBuilder);
                MakeRequired<MappedDataTypesWithIdentity>(modelBuilder);

                modelBuilder.Entity<MappedSizedDataTypes>();
                modelBuilder.Entity<MappedScaledDataTypes>();
                modelBuilder.Entity<MappedPrecisionAndScaledDataTypes>();
                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>();
                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>();
                modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>();
                modelBuilder.Entity<MappedSizedDataTypesWithIdentity>();
                modelBuilder.Entity<MappedScaledDataTypesWithIdentity>();
                modelBuilder.Entity<MappedPrecisionAndScaledDataTypesWithIdentity>();
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                var options = base.AddOptions(builder).ConfigureWarnings(
                    c => c
                        .Log(RelationalEventId.QueryClientEvaluationWarning)
                        .Log(SqlServerEventId.DecimalTypeDefaultWarning));

                new SqlServerDbContextOptionsBuilder(options).MinBatchSize(1);

                return options;
            }
        }

        [Flags]
        protected enum StringEnum16 : short
        {
            Value1 = 1,
            Value2 = 2,
            Value4 = 4
        }

        [Flags]
        protected enum StringEnumU16 : ushort
        {
            Value1 = 1,
            Value2 = 2,
            Value4 = 4
        }

        protected class MappedDataTypes
        {
            [Column(TypeName = "int")]
            public int Int { get; set; }

            [Column(TypeName = "bigint")]
            public long LongAsBigInt { get; set; }

            [Column(TypeName = "smallint")]
            public short ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte SByteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsBinaryVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char CharAsInt { get; set; }

            [Column(TypeName = "varchar(max)")]
            public StringEnum16 EnumAsVarcharMax { get; set; }

            [Column(TypeName = "nvarchar(20)")]
            public StringEnumU16 EnumAsNvarchar20 { get; set; }
#if !Test20
            [Column(TypeName = "sql_variant")]
            public object SqlVariantString { get; set; }

            [Column(TypeName = "sql_variant")]
            public object SqlVariantInt { get; set; }
#endif
        }

        protected class MappedSizedDataTypes
        {
            public int Id { get; set; }

            [Column(TypeName = "char(3)")]
            public string StringAsChar3 { get; set; }

            [Column(TypeName = "character(3)")]
            public string StringAsCharacter3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public string StringAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public string StringAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public string StringAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nchar(3)")]
            public string StringAsNchar3 { get; set; }

            [Column(TypeName = "national character(3)")]
            public string StringAsNationalCharacter3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public string StringAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public string StringAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public string StringAsNationalCharacterVarying3 { get; set; }

            [Column(TypeName = "binary(3)")]
            public byte[] BytesAsBinary3 { get; set; }

            [Column(TypeName = "varbinary(3)")]
            public byte[] BytesAsVarbinary3 { get; set; }

            [Column(TypeName = "binary varying(3)")]
            public byte[] BytesAsBinaryVarying3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public char? CharAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public char? CharAsAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public char? CharAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public char? CharAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public char? CharAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public char? CharAsNationalCharacterVarying3 { get; set; }
        }

        protected class MappedScaledDataTypes
        {
            public int Id { get; set; }

            [Column(TypeName = "float(3)")]
            public float FloatAsFloat3 { get; set; }

            [Column(TypeName = "double precision(3)")]
            public float FloatAsDoublePrecision3 { get; set; }

            [Column(TypeName = "float(25)")]
            public float FloatAsFloat25 { get; set; }

            [Column(TypeName = "double precision(25)")]
            public float FloatAsDoublePrecision25 { get; set; }

            [Column(TypeName = "datetimeoffset(3)")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset3 { get; set; }

            [Column(TypeName = "datetime2(3)")]
            public DateTime DateTimeAsDatetime23 { get; set; }

            [Column(TypeName = "decimal(3)")]
            public decimal DecimalAsDecimal3 { get; set; }

            [Column(TypeName = "dec(3)")]
            public decimal DecimalAsDec3 { get; set; }

            [Column(TypeName = "numeric(3)")]
            public decimal DecimalAsNumeric3 { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypes
        {
            public int Id { get; set; }

            [Column(TypeName = "decimal(5,2)")]
            public decimal DecimalAsDecimal52 { get; set; }

            [Column(TypeName = "dec(5,2)")]
            public decimal DecimalAsDec52 { get; set; }

            [Column(TypeName = "numeric(5,2)")]
            public decimal DecimalAsNumeric52 { get; set; }
        }

        protected class MappedNullableDataTypes
        {
            [Column(TypeName = "int")]
            public int? Int { get; set; }

            [Column(TypeName = "bigint")]
            public long? LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short? ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte? ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint? UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong? UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort? UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte? SbyteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool? BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal? DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal? DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double? DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float? FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double? DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime? DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset? DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime? DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime? DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime? DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan? TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsBinaryVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal? Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal? DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal? DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid? GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint? UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong? UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort? UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte? SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char? CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char? CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char? CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char? CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char? CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char? CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char? CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char? CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char? CharAsInt { get; set; }

            [Column(TypeName = "varchar(max)")]
            public StringEnum16? EnumAsVarcharMax { get; set; }

            [Column(TypeName = "nvarchar(20)")]
            public StringEnumU16? EnumAsNvarchar20 { get; set; }
#if !Test20
            [Column(TypeName = "sql_variant")]
            public object SqlVariantString { get; set; }

            [Column(TypeName = "sql_variant")]
            public object SqlVariantInt { get; set; }
#endif
        }

        protected class MappedDataTypesWithIdentity
        {
            public int Id { get; set; }

            [Column(TypeName = "int")]
            public int Int { get; set; }

            [Column(TypeName = "bigint")]
            public long LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort UShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte SbyteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double DoubleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsBinaryVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char CharAsInt { get; set; }

            [Column(TypeName = "varchar(max)")]
            public StringEnum16 EnumAsVarcharMax { get; set; }

            [Column(TypeName = "nvarchar(20)")]
            public StringEnumU16 EnumAsNvarchar20 { get; set; }
#if !Test20
            [Column(TypeName = "sql_variant")]
            public object SqlVariantString { get; set; }

            [Column(TypeName = "sql_variant")]
            public object SqlVariantInt { get; set; }
#endif
        }

        protected class MappedSizedDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            [Column(TypeName = "char(3)")]
            public string StringAsChar3 { get; set; }

            [Column(TypeName = "character(3)")]
            public string StringAsCharacter3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public string StringAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public string StringAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public string StringAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nchar(3)")]
            public string StringAsNchar3 { get; set; }

            [Column(TypeName = "national character(3)")]
            public string StringAsNationalCharacter3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public string StringAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public string StringAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public string StringAsNationalCharacterVarying3 { get; set; }

            [Column(TypeName = "binary(3)")]
            public byte[] BytesAsBinary3 { get; set; }

            [Column(TypeName = "varbinary(3)")]
            public byte[] BytesAsVarbinary3 { get; set; }

            [Column(TypeName = "binary varying(3)")]
            public byte[] BytesAsBinaryVarying3 { get; set; }

            [Column(TypeName = "varchar(3)")]
            public char? CharAsVarchar3 { get; set; }

            [Column(TypeName = "char varying(3)")]
            public char? CharAsAsCharVarying3 { get; set; }

            [Column(TypeName = "character varying(3)")]
            public char? CharAsCharacterVarying3 { get; set; }

            [Column(TypeName = "nvarchar(3)")]
            public char? CharAsNvarchar3 { get; set; }

            [Column(TypeName = "national char varying(3)")]
            public char? CharAsNationalCharVarying3 { get; set; }

            [Column(TypeName = "national character varying(3)")]
            public char? CharAsNationalCharacterVarying3 { get; set; }
        }

        protected class MappedScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            [Column(TypeName = "float(3)")]
            public float FloatAsFloat3 { get; set; }

            [Column(TypeName = "double precision(3)")]
            public float FloatAsDoublePrecision3 { get; set; }

            [Column(TypeName = "float(25)")]
            public float FloatAsFloat25 { get; set; }

            [Column(TypeName = "double precision(25)")]
            public float FloatAsDoublePrecision25 { get; set; }

            [Column(TypeName = "datetimeoffset(3)")]
            public DateTimeOffset DateTimeOffsetAsDatetimeoffset3 { get; set; }

            [Column(TypeName = "datetime2(3)")]
            public DateTime DateTimeAsDatetime23 { get; set; }

            [Column(TypeName = "decimal(3)")]
            public decimal DecimalAsDecimal3 { get; set; }

            [Column(TypeName = "dec(3)")]
            public decimal DecimalAsDec3 { get; set; }

            [Column(TypeName = "numeric(3)")]
            public decimal DecimalAsNumeric3 { get; set; }
        }

        protected class MappedPrecisionAndScaledDataTypesWithIdentity
        {
            public int Id { get; set; }
            public int Int { get; set; }

            [Column(TypeName = "decimal(5,2)")]
            public decimal DecimalAsDecimal52 { get; set; }

            [Column(TypeName = "dec(5,2)")]
            public decimal DecimalAsDec52 { get; set; }

            [Column(TypeName = "numeric(5,2)")]
            public decimal DecimalAsNumeric52 { get; set; }
        }

        protected class MappedNullableDataTypesWithIdentity
        {
            public int Id { get; set; }

            [Column(TypeName = "int")]
            public int? Int { get; set; }

            [Column(TypeName = "bigint")]
            public long? LongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public short? ShortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public byte? ByteAsTinyint { get; set; }

            [Column(TypeName = "int")]
            public uint? UintAsInt { get; set; }

            [Column(TypeName = "bigint")]
            public ulong? UlongAsBigint { get; set; }

            [Column(TypeName = "smallint")]
            public ushort? UshortAsSmallint { get; set; }

            [Column(TypeName = "tinyint")]
            public sbyte? SbyteAsTinyint { get; set; }

            [Column(TypeName = "bit")]
            public bool? BoolAsBit { get; set; }

            [Column(TypeName = "money")]
            public decimal? DecimalAsMoney { get; set; }

            [Column(TypeName = "smallmoney")]
            public decimal? DecimalAsSmallmoney { get; set; }

            [Column(TypeName = "float")]
            public double? DoubleAsFloat { get; set; }

            [Column(TypeName = "real")]
            public float? FloatAsReal { get; set; }

            [Column(TypeName = "double precision")]
            public double? DoubkleAsDoublePrecision { get; set; }

            [Column(TypeName = "date")]
            public DateTime? DateTimeAsDate { get; set; }

            [Column(TypeName = "datetimeoffset")]
            public DateTimeOffset? DateTimeOffsetAsDatetimeoffset { get; set; }

            [Column(TypeName = "datetime2")]
            public DateTime? DateTimeAsDatetime2 { get; set; }

            [Column(TypeName = "smalldatetime")]
            public DateTime? DateTimeAsSmalldatetime { get; set; }

            [Column(TypeName = "datetime")]
            public DateTime? DateTimeAsDatetime { get; set; }

            [Column(TypeName = "time")]
            public TimeSpan? TimeSpanAsTime { get; set; }

            [Column(TypeName = "varchar(max)")]
            public string StringAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public string StringAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public string StringAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public string StringAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public string StringAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public string StringAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public string StringAsText { get; set; }

            [Column(TypeName = "ntext")]
            public string StringAsNtext { get; set; }

            [Column(TypeName = "varbinary(max)")]
            public byte[] BytesAsVarbinaryMax { get; set; }

            [Column(TypeName = "binary varying(max)")]
            public byte[] BytesAsVaryingMax { get; set; }

            [Column(TypeName = "image")]
            public byte[] BytesAsImage { get; set; }

            [Column(TypeName = "decimal")]
            public decimal? Decimal { get; set; }

            [Column(TypeName = "dec")]
            public decimal? DecimalAsDec { get; set; }

            [Column(TypeName = "numeric")]
            public decimal? DecimalAsNumeric { get; set; }

            [Column(TypeName = "uniqueidentifier")]
            public Guid? GuidAsUniqueidentifier { get; set; }

            [Column(TypeName = "bigint")]
            public uint? UintAsBigint { get; set; }

            [Column(TypeName = "decimal(20,0)")]
            public ulong? UlongAsDecimal200 { get; set; }

            [Column(TypeName = "int")]
            public ushort? UShortAsInt { get; set; }

            [Column(TypeName = "smallint")]
            public sbyte? SByteAsSmallint { get; set; }

            [Column(TypeName = "varchar(max)")]
            public char? CharAsVarcharMax { get; set; }

            [Column(TypeName = "char varying(max)")]
            public char? CharAsAsCharVaryingMax { get; set; }

            [Column(TypeName = "character varying(max)")]
            public char? CharAsCharacterVaryingMax { get; set; }

            [Column(TypeName = "nvarchar(max)")]
            public char? CharAsNvarcharMax { get; set; }

            [Column(TypeName = "national char varying(max)")]
            public char? CharAsNationalCharVaryingMax { get; set; }

            [Column(TypeName = "national character varying(max)")]
            public char? CharAsNationalCharacterVaryingMax { get; set; }

            [Column(TypeName = "text")]
            public char? CharAsText { get; set; }

            [Column(TypeName = "ntext")]
            public char? CharAsNtext { get; set; }

            [Column(TypeName = "int")]
            public char? CharAsInt { get; set; }

            [Column(TypeName = "varchar(max)")]
            public StringEnum16? EnumAsVarcharMax { get; set; }

            [Column(TypeName = "nvarchar(20)")]
            public StringEnumU16? EnumAsNvarchar20 { get; set; }
#if !Test20
            [Column(TypeName = "sql_variant")]
            public object SqlVariantString { get; set; }

            [Column(TypeName = "sql_variant")]
            public object SqlVariantInt { get; set; }
#endif
        }
    }
}
