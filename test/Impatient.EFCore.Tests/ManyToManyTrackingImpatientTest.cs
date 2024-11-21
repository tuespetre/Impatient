using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Collections.ObjectModel;
using Xunit;

namespace Impatient.EFCore.Tests;

public class ManyToManyTrackingImpatientTest : ManyToManyTrackingRelationalTestBase<ManyToManyTrackingImpatientTest.Fixture>
{
    public ManyToManyTrackingImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.ListLoggerFactory.Clear();
    }

    protected override Dictionary<string, DeleteBehavior> CustomDeleteBehaviors { get; } = new()
    {
        { "EntityBranch.RootSkipShared", DeleteBehavior.ClientCascade },
        { "EntityBranch2.Leaf2SkipShared", DeleteBehavior.ClientCascade },
        { "EntityBranch2.SelfSkipSharedLeft", DeleteBehavior.ClientCascade },
        { "EntityOne.SelfSkipPayloadLeft", DeleteBehavior.ClientCascade },
        { "EntityTableSharing1.TableSharing2Shared", DeleteBehavior.ClientCascade },
        { "EntityTwo.SelfSkipSharedLeft", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityBranch.UnidirectionalEntityRoot", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityOne.SelfSkipPayloadLeft", DeleteBehavior.ClientCascade },
        { "UnidirectionalEntityTwo.SelfSkipSharedRight", DeleteBehavior.ClientCascade },
    };

    public new class Fixture : ManyToManyTrackingRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder
                .Entity<JoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                .IndexerProperty<string>("Payload")
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<JoinOneToThreePayloadFull>()
                .Property(e => e.Payload)
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<UnidirectionalJoinOneSelfPayload>()
                .Property(e => e.Payload)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder
                .SharedTypeEntity<Dictionary<string, object>>("UnidirectionalJoinOneToThreePayloadFullShared")
                .IndexerProperty<string>("Payload")
                .HasDefaultValue("Generated");

            modelBuilder
                .Entity<UnidirectionalJoinOneToThreePayloadFull>()
                .Property(e => e.Payload)
                .HasDefaultValue("Generated");
        }
    }

    public override async Task Can_insert_many_to_many(bool async)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].TwoSkip = CreateCollection<EntityTwo>();

                leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                leftEntities[0].TwoSkip.Add(rightEntities[1]); // 11 - 22
                leftEntities[0].TwoSkip.Add(rightEntities[2]); // 11 - 23

                rightEntities[0].OneSkip = CreateCollection<EntityOne>();

                rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                if (async)
                {
                    await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                    await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                }
                else
                {
                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                }

                ValidateFixup(context, leftEntities, rightEntities);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.TwoSkip);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

            Assert.Equal(3, leftEntities[0].TwoSkip.Count);
            Assert.Single(leftEntities[1].TwoSkip);
            Assert.Single(leftEntities[2].TwoSkip);

            Assert.Equal(3, rightEntities[0].OneSkip.Count);
            Assert.Single(rightEntities[1].OneSkip);
            Assert.Single(rightEntities[2].OneSkip);

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
        
        ICollection<TEntity> CreateCollection<TEntity>()
            => RequiresDetectChanges ? new List<TEntity>() : new ObservableCollection<TEntity>();
    }

    public override async Task Can_insert_many_to_many_self_shared(bool async)
    {
        List<int> leftKeys = null;
        List<int> rightKeys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = base.Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].SelfSkipSharedLeft = CreateCollection<EntityTwo>();

                leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[0]); // 11 - 21
                leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[1]); // 11 - 22
                leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[2]); // 11 - 23

                rightEntities[0].SelfSkipSharedRight = CreateCollection<EntityTwo>();

                rightEntities[0].SelfSkipSharedRight.Add(leftEntities[0]); // 21 - 11 (Dupe)
                rightEntities[0].SelfSkipSharedRight.Add(leftEntities[1]); // 21 - 12
                rightEntities[0].SelfSkipSharedRight.Add(leftEntities[2]); // 21 - 13

                if (async)
                {
                    await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                    await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                }
                else
                {
                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                }

                ValidateFixup(context, leftEntities, rightEntities);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                leftKeys = leftEntities.Select(e => e.Id).ToList();
                rightKeys = rightEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityTwo>()
                    .Where(e => leftKeys.Contains(e.Id) || rightKeys.Contains(e.Id))
                    .Include(e => e.SelfSkipSharedLeft);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(6, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityTwo>()
                    .Select(e => e.Entity)
                    .Where(e => leftKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                var rightEntities = context.ChangeTracker.Entries<EntityTwo>()
                    .Select(e => e.Entity)
                    .Where(e => rightKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityTwo> leftEntities, IList<EntityTwo> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(6, context.ChangeTracker.Entries<EntityTwo>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

            Assert.Equal(3, leftEntities[0].SelfSkipSharedLeft.Count);
            Assert.Single(leftEntities[1].SelfSkipSharedLeft);
            Assert.Single(leftEntities[2].SelfSkipSharedLeft);

            Assert.Equal(3, rightEntities[0].SelfSkipSharedRight.Count);
            Assert.Single(rightEntities[1].SelfSkipSharedRight);
            Assert.Single(rightEntities[2].SelfSkipSharedRight);

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }

        ICollection<TEntity> CreateCollection<TEntity>()
            => RequiresDetectChanges ? new List<TEntity>() : new ObservableCollection<TEntity>();
    }

    new protected static void VerifyRelationshipSnapshots(DbContext context, IEnumerable<object> entities)
    {
        var detectChanges = context.ChangeTracker.AutoDetectChangesEnabled;
        try
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var entity in entities)
            {
                var entityEntry = context.Entry(entity).GetInfrastructure();
                var entityType = entityEntry.EntityType;

                if (entityEntry.HasRelationshipSnapshot)
                {
                    foreach (var property in entityType.GetForeignKeys().SelectMany(e => e.Properties))
                    {
                        if (property.GetRelationshipIndex() >= 0)
                        {
                            Assert.Equal(entityEntry.GetRelationshipSnapshotValue(property), entityEntry[property]);
                        }
                    }

                    foreach (var navigation in entityType.GetNavigations()
                                 .Concat((IEnumerable<INavigationBase>)entityType.GetSkipNavigations()))
                    {
                        if (navigation.GetRelationshipIndex() >= 0)
                        {
                            var snapshot = entityEntry.GetRelationshipSnapshotValue(navigation);
                            var current = entityEntry[navigation];

                            if (navigation.IsCollection)
                            {
                                var currentCollection = ((IEnumerable<object>)current)?.ToList();
                                var snapshotCollection = ((IEnumerable<object>)snapshot)?.ToList();

                                if (snapshot == null)
                                {
                                    Assert.True(current == null || !currentCollection.Any());
                                }
                                else if (current == null)
                                {
                                    Assert.True(snapshot == null || !snapshotCollection.Any());
                                }
                                else
                                {
                                    Assert.Equal(snapshotCollection.Count, currentCollection.Count);

                                    foreach (var related in snapshotCollection)
                                    {
                                        try
                                        {
                                            Assert.Contains(currentCollection, c => ReferenceEquals(c, related));
                                        }
                                        catch
                                        {
                                            _ = related;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Assert.Same(snapshot, current);
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = detectChanges;
        }
    }
}
