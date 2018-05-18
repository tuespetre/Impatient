using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class OptimisticConcurrencyImpatientTest : OptimisticConcurrencyTestBase<F1ImpatientFixture>
    {
        public OptimisticConcurrencyImpatientTest(F1ImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public override Task Concurrency_issue_where_a_complex_type_nested_member_is_the_concurrency_token_can_be_handled()
        {
            return ConcurrencyTestAsync(
                c => c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude = 47.642576,
                c => c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude = 47.642576,
                (c, ex) =>
                {
                    Assert.IsType<DbUpdateConcurrencyException>(ex);

                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Location>(entry.Entity);
                    entry.Reload();
                },
                c =>
                    Assert.Equal(47.642576, c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude));
        }
        protected override async Task ConcurrencyTestAsync(
           Action<F1Context> storeChange, Action<F1Context> clientChange,
           Action<F1Context, DbUpdateException> resolver, Action<F1Context> validator)
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            clientChange(context);

                            using (var innerContext = CreateF1Context())
                            {
                                UseTransaction(innerContext.Database, transaction);
                                storeChange(innerContext);
                                await innerContext.SaveChangesAsync();

                                var updateException = await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());

                                resolver(context, updateException);

                                using (var validationContext = CreateF1Context())
                                {
                                    UseTransaction(validationContext.Database, transaction);
                                    if (validator != null)
                                    {
                                        await context.SaveChangesAsync();

                                        validator(validationContext);
                                    }
                                }
                            }
                        }
                    });
            }
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
