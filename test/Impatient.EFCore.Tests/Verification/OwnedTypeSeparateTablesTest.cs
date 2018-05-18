using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Verification
{
    public class OwnedTypeSeparateTablesTest : IClassFixture<OwnedTypeSeparateTablesTestFixture>
    {
        private readonly OwnedTypeSeparateTablesTestFixture fixture;

        public OwnedTypeSeparateTablesTest(OwnedTypeSeparateTablesTestFixture fixture)
        {
            fixture.Logger.Clear();

            this.fixture = fixture;
        }

        [Fact]
        public void Simple_owned_level1()
        {
            AssertQuery<Level1>(l1s => l1s.Include(l1 => l1.OneToOne_Required_PK), elementSorter: e => e.Id);

            AssertSql(
    @"SELECT [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Name] AS [Name], [t].[$empty] AS [OneToOne_Required_PK.$empty], [t].[Id] AS [OneToOne_Required_PK.Id], [t].[Date] AS [OneToOne_Required_PK.Date], [t].[Level1_Optional_Id] AS [OneToOne_Required_PK.Level1_Optional_Id], [t].[Level1_Required_Id] AS [OneToOne_Required_PK.Level1_Required_Id], [t].[Name] AS [OneToOne_Required_PK.Name], [t_0].[$empty] AS [OneToOne_Required_PK.OneToOne_Required_PK.$empty], [t_0].[Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.Id], [t_0].[Level2_Optional_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.Level2_Optional_Id], [t_0].[Level2_Required_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.Level2_Required_Id], [t_0].[Name] AS [OneToOne_Required_PK.OneToOne_Required_PK.Name], [t_1].[$empty] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.$empty], [t_1].[Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Id], [t_1].[Level3_Optional_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Level3_Optional_Id], [t_1].[Level3_Required_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Level3_Required_Id], [t_1].[Name] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Name]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT 0 AS [$empty], [l_0].[Id] AS [Id], [l_0].[OneToOne_Required_PK_Date] AS [Date], [l_0].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_0].[Level1_Required_Id] AS [Level1_Required_Id], [l_0].[Name] AS [Name]
    FROM [Level2] AS [l_0]
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT 0 AS [$empty], [l_1].[Id] AS [Id], [l_1].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_1].[Level2_Required_Id] AS [Level2_Required_Id], [l_1].[Name] AS [Name]
    FROM [Level3] AS [l_1]
) AS [t_0] ON [t].[Id] = [t_0].[Id]
LEFT JOIN (
    SELECT 0 AS [$empty], [l_2].[Id] AS [Id], [l_2].[Level3_Optional_Id] AS [Level3_Optional_Id], [l_2].[Level3_Required_Id] AS [Level3_Required_Id], [l_2].[Name] AS [Name]
    FROM [Level4] AS [l_2]
) AS [t_1] ON [t_0].[Id] = [t_1].[Id]");
        }

        [Fact]
        public void Simple_owned_level1_convention()
        {
            AssertQuery<Level1>(l1s => l1s, elementSorter: e => e.Id);

            AssertSql(
    @"SELECT [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Name] AS [Name], [t].[$empty] AS [OneToOne_Required_PK.$empty], [t].[Id] AS [OneToOne_Required_PK.Id], [t].[Date] AS [OneToOne_Required_PK.Date], [t].[Level1_Optional_Id] AS [OneToOne_Required_PK.Level1_Optional_Id], [t].[Level1_Required_Id] AS [OneToOne_Required_PK.Level1_Required_Id], [t].[Name] AS [OneToOne_Required_PK.Name], [t_0].[$empty] AS [OneToOne_Required_PK.OneToOne_Required_PK.$empty], [t_0].[Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.Id], [t_0].[Level2_Optional_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.Level2_Optional_Id], [t_0].[Level2_Required_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.Level2_Required_Id], [t_0].[Name] AS [OneToOne_Required_PK.OneToOne_Required_PK.Name], [t_1].[$empty] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.$empty], [t_1].[Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Id], [t_1].[Level3_Optional_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Level3_Optional_Id], [t_1].[Level3_Required_Id] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Level3_Required_Id], [t_1].[Name] AS [OneToOne_Required_PK.OneToOne_Required_PK.OneToOne_Required_PK.Name]
FROM [Level1] AS [l]
LEFT JOIN (
    SELECT 0 AS [$empty], [l_0].[Id] AS [Id], [l_0].[OneToOne_Required_PK_Date] AS [Date], [l_0].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_0].[Level1_Required_Id] AS [Level1_Required_Id], [l_0].[Name] AS [Name]
    FROM [Level2] AS [l_0]
) AS [t] ON [l].[Id] = [t].[Id]
LEFT JOIN (
    SELECT 0 AS [$empty], [l_1].[Id] AS [Id], [l_1].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_1].[Level2_Required_Id] AS [Level2_Required_Id], [l_1].[Name] AS [Name]
    FROM [Level3] AS [l_1]
) AS [t_0] ON [t].[Id] = [t_0].[Id]
LEFT JOIN (
    SELECT 0 AS [$empty], [l_2].[Id] AS [Id], [l_2].[Level3_Optional_Id] AS [Level3_Optional_Id], [l_2].[Level3_Required_Id] AS [Level3_Required_Id], [l_2].[Name] AS [Name]
    FROM [Level4] AS [l_2]
) AS [t_1] ON [t_0].[Id] = [t_1].[Id]");
        }

        private void AssertSql(params string[] expected) => fixture.Logger.AssertBaseline(expected);

        #region AssertQuery

        private void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> query,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
            => AssertQuery(query, query, elementSorter, elementAsserter, verifyOrdered);

        private void AssertQuery<TItem1>(
            Func<IQueryable<TItem1>, IQueryable<object>> efQuery,
            Func<IQueryable<TItem1>, IQueryable<object>> l2oQuery,
            Func<dynamic, object> elementSorter = null,
            Action<dynamic, dynamic> elementAsserter = null,
            bool verifyOrdered = false)
            where TItem1 : class
        {
            using (var context = CreateContext())
            {
                var actual = efQuery(Set<TItem1>(context)).ToArray();
                var expected = l2oQuery(ExpectedSet<TItem1>()).ToArray();
                TestHelpers.AssertResults(
                    expected,
                    actual,
                    elementSorter ?? (e => e),
                    elementAsserter ?? ((e, a) => Assert.Equal(e, a)),
                    verifyOrdered);
            }
        }

        protected ComplexNavigationsContext CreateContext() => fixture.CreateContext(fixture.CreateTestStore());

        protected IQueryable<T> ExpectedSet<T>()
        {
            if (typeof(T) == typeof(Level1))
            {
                return (IQueryable<T>)GetExpectedLevelOne();
            }

            if (typeof(T) == typeof(Level2))
            {
                return (IQueryable<T>)GetExpectedLevelTwo();
            }

            if (typeof(T) == typeof(Level3))
            {
                return (IQueryable<T>)GetExpectedLevelThree();
            }

            if (typeof(T) == typeof(Level4))
            {
                return (IQueryable<T>)GetExpectedLevelFour();
            }

            throw new NotImplementedException();
        }

        protected IQueryable<T> Set<T>(ComplexNavigationsContext context)
        {
            if (typeof(T) == typeof(Level1))
            {
                return (IQueryable<T>)GetLevelOne(context);
            }

            if (typeof(T) == typeof(Level2))
            {
                return (IQueryable<T>)GetLevelTwo(context);
            }

            if (typeof(T) == typeof(Level3))
            {
                return (IQueryable<T>)GetLevelThree(context);
            }

            if (typeof(T) == typeof(Level4))
            {
                return (IQueryable<T>)GetLevelFour(context);
            }

            throw new NotImplementedException();
        }

        protected virtual IQueryable<Level1> GetExpectedLevelOne()
            => ComplexNavigationsData.SplitLevelOnes.AsQueryable();

        protected virtual IQueryable<Level2> GetExpectedLevelTwo()
            => GetExpectedLevelOne().Select(t => t.OneToOne_Required_PK).Where(t => t != null);

        protected virtual IQueryable<Level3> GetExpectedLevelThree()
            => GetExpectedLevelTwo().Select(t => t.OneToOne_Required_PK).Where(t => t != null);

        protected virtual IQueryable<Level4> GetExpectedLevelFour()
            => GetExpectedLevelThree().Select(t => t.OneToOne_Required_PK).Where(t => t != null);

        protected virtual IQueryable<Level1> GetLevelOne(ComplexNavigationsContext context)
        {
            return context.LevelOne;
        }

        protected virtual IQueryable<Level2> GetLevelTwo(ComplexNavigationsContext context)
            => GetLevelOne(context).Select(t => t.OneToOne_Required_PK).Where(t => t != null);

        protected virtual IQueryable<Level3> GetLevelThree(ComplexNavigationsContext context)
            => GetLevelTwo(context).Select(t => t.OneToOne_Required_PK).Where(t => t != null);

        protected virtual IQueryable<Level4> GetLevelFour(ComplexNavigationsContext context)
            => GetLevelThree(context).Select(t => t.OneToOne_Required_PK).Where(t => t != null);

        #endregion
    }

    public class OwnedTypeSeparateTablesTestFixture : ComplexNavigationsOwnedQueryRelationalFixtureBase<ImpatientTestStore>
    {
        private const string connectionString = 
            "Server=.\\sqlexpress; " +
            "Database=efcore-impatient-complex-navigations-owned-separate-tables; " +
            "Trusted_Connection=true; " +
            "MultipleActiveResultSets=True";

        private readonly DbContextOptions options;

        public TestSqlLoggerFactory Logger { get; }

        public OwnedTypeSeparateTablesTestFixture()
        {
            var services = new ServiceCollection();

            var provider
                = services
                    .AddEntityFrameworkSqlServer()
                    .AddImpatientEFCoreQueryCompiler()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(Logger = new TestSqlLoggerFactory())
                    .BuildServiceProvider();

            options
                = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(provider)
                    .Options;

            using (var context = new ComplexNavigationsContext(
                new DbContextOptionsBuilder(options).UseSqlServer(connectionString).Options))
            {
                if (context.Database.EnsureCreated())
                {
                    ComplexNavigationsModelInitializer.Seed(context, tableSplitting: true);
                }
            }
        }

        public override ComplexNavigationsContext CreateContext(ImpatientTestStore testStore)
        {
            var context
                = new ComplexNavigationsContext(
                    new DbContextOptionsBuilder(options)
                        .UseSqlServer(testStore.Connection).Options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level1>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Level1>()
                .Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required)
                .Ignore(e => e.OneToMany_Optional)
                .Ignore(e => e.OneToOne_Optional_PK)
                .Ignore(e => e.OneToOne_Optional_FK)
                .Ignore(e => e.OneToOne_Required_FK)
                .OwnsOne(e => e.OneToOne_Required_PK, Configure);

            modelBuilder.Entity<ComplexNavigationField>().HasKey(e => e.Name);
            modelBuilder.Entity<ComplexNavigationString>().HasKey(e => e.DefaultText);
            modelBuilder.Entity<ComplexNavigationGlobalization>().HasKey(e => e.Text);
            modelBuilder.Entity<ComplexNavigationLanguage>().HasKey(e => e.Name);

            modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Label);
            modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Placeholder);

            modelBuilder.Entity<ComplexNavigationString>().HasMany(m => m.Globalizations);

            modelBuilder.Entity<ComplexNavigationGlobalization>().HasOne(g => g.Language);
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level1, Level2> l2)
        {
            l2.Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required)
                .Ignore(e => e.OneToMany_Required_Inverse)
                .Ignore(e => e.OneToMany_Optional)
                .Ignore(e => e.OneToMany_Optional_Inverse)
                .Ignore(e => e.OneToOne_Optional_PK_Inverse)
                .Ignore(e => e.OneToOne_Required_FK_Inverse)
                .Ignore(e => e.OneToOne_Optional_FK_Inverse)
                .Ignore(e => e.OneToOne_Optional_PK)
                .Ignore(e => e.OneToOne_Optional_FK)
                .Ignore(e => e.OneToOne_Required_FK);

            l2.HasForeignKey(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);

            l2.Property(e => e.Id).ValueGeneratedNever();

            /*l2.HasOne(e => e.OneToOne_Optional_PK_Inverse)
                .WithOne(e => e.OneToOne_Optional_PK)
                .HasPrincipalKey<Level1>(e => e.Id)
                .IsRequired(false);

            l2.HasOne(e => e.OneToOne_Required_FK_Inverse)
                .WithOne(e => e.OneToOne_Required_FK)
                .HasForeignKey<Level2>(e => e.Level1_Required_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            l2.HasOne(e => e.OneToOne_Optional_FK_Inverse)
                .WithOne(e => e.OneToOne_Optional_FK)
                .HasForeignKey<Level2>(e => e.Level1_Optional_Id)
                .IsRequired(false);*/

            l2.OwnsOne(e => e.OneToOne_Required_PK, Configure);

            l2.ToTable(nameof(Level2));
            l2.Property(l => l.Date).HasColumnName("OneToOne_Required_PK_Date");
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level2, Level3> l3)
        {
            l3.Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required)
                .Ignore(e => e.OneToMany_Required_Inverse)
                .Ignore(e => e.OneToMany_Optional)
                .Ignore(e => e.OneToMany_Optional_Inverse)
                .Ignore(e => e.OneToOne_Optional_PK_Inverse)
                .Ignore(e => e.OneToOne_Required_FK_Inverse)
                .Ignore(e => e.OneToOne_Optional_FK_Inverse)
                .Ignore(e => e.OneToOne_Optional_PK)
                .Ignore(e => e.OneToOne_Optional_FK)
                .Ignore(e => e.OneToOne_Required_FK);

            l3.HasForeignKey(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);

            l3.Property(e => e.Id).ValueGeneratedNever();

            /*l3.HasOne(e => e.OneToOne_Optional_PK_Inverse)
                .WithOne(e => e.OneToOne_Optional_PK)
                .HasPrincipalKey<Level2>(e => e.Id)
                .IsRequired(false);

            l3.HasOne(e => e.OneToOne_Required_FK_Inverse)
                .WithOne(e => e.OneToOne_Required_FK)
                .HasForeignKey<Level3>(e => e.Level2_Required_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            l3.HasOne(e => e.OneToOne_Optional_FK_Inverse)
                .WithOne(e => e.OneToOne_Optional_FK)
                .HasForeignKey<Level3>(e => e.Level2_Optional_Id)
                .IsRequired(false);*/

            l3.OwnsOne(e => e.OneToOne_Required_PK, Configure);

            l3.ToTable(nameof(Level3));
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level3, Level4> l4)
        {
            l4.Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required_Inverse)
                .Ignore(e => e.OneToMany_Optional_Inverse)
                .Ignore(e => e.OneToOne_Optional_PK_Inverse)
                .Ignore(e => e.OneToOne_Required_FK_Inverse)
                .Ignore(e => e.OneToOne_Optional_FK_Inverse);

            l4.HasForeignKey(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);

            l4.Property(e => e.Id).ValueGeneratedNever();

            /*l4.HasOne(e => e.OneToOne_Optional_PK_Inverse)
                .WithOne(e => e.OneToOne_Optional_PK)
                .HasPrincipalKey<Level3>()
                .IsRequired(false);

            l4.HasOne(e => e.OneToOne_Required_FK_Inverse)
                .WithOne(e => e.OneToOne_Required_FK)
                .HasForeignKey<Level4>(e => e.Level3_Required_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            l4.HasOne(e => e.OneToOne_Optional_FK_Inverse)
                .WithOne(e => e.OneToOne_Optional_FK)
                .HasForeignKey<Level4>(e => e.Level3_Optional_Id)
                .IsRequired(false);*/

            l4.ToTable(nameof(Level4));
        }
    }
}
