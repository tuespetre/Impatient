using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Impatient.EFCore.Tests;

public class F1ImpatientFixture : F1RelationalFixture<byte[]>
{
    protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

    public override TestHelpers TestHelpers => ImpatientTestHelpers.Instance;

    protected override void BuildModelExternal(ModelBuilder modelBuilder)
    {
        base.BuildModelExternal(modelBuilder);

        var converter = new BinaryVersionConverter();
        var comparer = new BinaryVersionComparer();

        modelBuilder.Entity<TitleSponsor>()
            .OwnsOne(
                s => s.Details, eb =>
                {
                    eb.Property(d => d.Space).HasColumnType("decimal(18,2)");
                });

        modelBuilder
            .Entity<Fan>()
            .Property(e => e.BinaryVersion)
            .HasConversion(converter, comparer)
            .IsRowVersion();

        modelBuilder
            .Entity<FanTpt>()
            .Property(e => e.BinaryVersion)
            .HasConversion(converter, comparer)
            .IsRowVersion();

        modelBuilder
            .Entity<FanTpc>()
            .Property(e => e.BinaryVersion)
            .HasConversion(converter, comparer)
            .IsRowVersion();

        modelBuilder
            .Entity<Circuit>()
            .Property(e => e.BinaryVersion)
            .HasConversion(converter, comparer)
            .IsRowVersion();

        modelBuilder
            .Entity<CircuitTpt>()
            .Property(e => e.BinaryVersion)
            .HasConversion(converter, comparer)
            .IsRowVersion();

        modelBuilder
            .Entity<CircuitTpc>()
            .Property(e => e.BinaryVersion)
            .HasConversion(converter, comparer)
            .IsRowVersion();
    }

    private class BinaryVersionConverter : ValueConverter<List<byte>, byte[]>
    {
        public BinaryVersionConverter()
            : base(
                v => v == null ? null : v.ToArray(),
                v => v == null ? null : v.ToList())
        {
        }
    }

    private class BinaryVersionComparer : ValueComparer<List<byte>>
    {
        public BinaryVersionComparer()
            : base(
                (l, r) => (l == null && r == null) || (l != null && r != null && l.SequenceEqual(r)),
                v => CalculateHashCode(v),
                v => v == null ? null : v.ToList())
        {
        }

        private static int CalculateHashCode(List<byte> source)
        {
            if (source == null)
            {
                return 0;
            }

            var hash = new HashCode();
            foreach (var el in source)
            {
                hash.Add(el);
            }

            return hash.ToHashCode();
        }
    }
}
