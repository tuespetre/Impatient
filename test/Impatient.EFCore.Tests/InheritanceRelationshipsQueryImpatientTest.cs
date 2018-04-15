using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationships;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class InheritanceRelationshipsQueryImpatientTest : InheritanceRelationshipsQueryTestBase<ImpatientTestStore, InheritanceRelationshipsQueryImpatientFixture>
    {
        public InheritanceRelationshipsQueryImpatientTest(InheritanceRelationshipsQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_with_inheritance_on_derived_reverse()
        {
            base.Include_collection_with_inheritance_on_derived_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_with_inheritance_reverse()
        {
            base.Include_collection_with_inheritance_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_with_inheritance_with_filter_reverse()
        {
            base.Include_collection_with_inheritance_with_filter_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_without_inheritance_reverse()
        {
            base.Include_collection_without_inheritance_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_without_inheritance_with_filter_reverse()
        {
            base.Include_collection_without_inheritance_with_filter_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_with_filter_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived_with_filter_reverse()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived_with_filter1()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter1();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived_with_filter2()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived_with_filter4()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived1()
        {
            base.Include_reference_with_inheritance_on_derived1();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived2()
        {
            base.Include_reference_with_inheritance_on_derived2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_on_derived4()
        {
            base.Include_reference_with_inheritance_on_derived4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_reverse()
        {
            base.Include_reference_with_inheritance_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance1()
        {
            base.Include_reference_with_inheritance1();
        }

        public override void Changes_in_derived_related_entities_are_detected()
        {
            base.Changes_in_derived_related_entities_are_detected();
        }

        public override void Entity_can_make_separate_relationships_with_base_type_and_derived_type_both()
        {
            base.Entity_can_make_separate_relationships_with_base_type_and_derived_type_both();
        }

        public override void Include_reference_with_inheritance2()
        {
            base.Include_reference_with_inheritance2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_self_refence_with_inheritence()
        {
            base.Include_self_refence_with_inheritence();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_self_refence_with_inheritence_reverse()
        {
            base.Include_self_refence_with_inheritence_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_with_inheritance_with_filter1()
        {
            base.Include_reference_with_inheritance_with_filter1();
        }

        public override void Include_reference_with_inheritance_with_filter2()
        {
            base.Include_reference_with_inheritance_with_filter2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_without_inheritance()
        {
            base.Include_reference_without_inheritance();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_without_inheritance_reverse()
        {
            base.Include_reference_without_inheritance_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_without_inheritance_with_filter()
        {
            base.Include_reference_without_inheritance_with_filter();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_without_inheritance_with_filter_reverse()
        {
            base.Include_reference_without_inheritance_with_filter_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_with_inheritance1()
        {
            base.Include_collection_with_inheritance1();
        }

        public override void Include_collection_with_inheritance2()
        {
            base.Include_collection_with_inheritance2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_with_inheritance_with_filter1()
        {
            base.Include_collection_with_inheritance_with_filter1();
        }

        public override void Include_collection_with_inheritance_with_filter2()
        {
            base.Include_collection_with_inheritance_with_filter2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_without_inheritance()
        {
            base.Include_collection_without_inheritance();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_without_inheritance_with_filter()
        {
            base.Include_collection_without_inheritance_with_filter();
        }

        public override void Include_reference_with_inheritance_on_derived3()
        {
            base.Include_reference_with_inheritance_on_derived3();
        }

        public override void Include_reference_with_inheritance_on_derived_with_filter3()
        {
            base.Include_reference_with_inheritance_on_derived_with_filter3();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_without_inheritance_on_derived1()
        {
            base.Include_reference_without_inheritance_on_derived1();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_without_inheritance_on_derived2()
        {
            base.Include_reference_without_inheritance_on_derived2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_reference_without_inheritance_on_derived_reverse()
        {
            base.Include_reference_without_inheritance_on_derived_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_with_inheritance_on_derived1()
        {
            base.Include_collection_with_inheritance_on_derived1();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Include_collection_with_inheritance_on_derived2()
        {
            base.Include_collection_with_inheritance_on_derived2();
        }

        public override void Include_collection_with_inheritance_on_derived3()
        {
            base.Include_collection_with_inheritance_on_derived3();
        }

        public override void Include_collection_with_inheritance_on_derived4()
        {
            base.Include_collection_with_inheritance_on_derived4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_reference_reference1()
        {
            base.Nested_include_with_inheritance_reference_reference1();
        }
        
        public override void Nested_include_with_inheritance_reference_reference2()
        {
            base.Nested_include_with_inheritance_reference_reference2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_reference_reference3()
        {
            base.Nested_include_with_inheritance_reference_reference3();
        }

        public override void Nested_include_with_inheritance_reference_reference4()
        {
            base.Nested_include_with_inheritance_reference_reference4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_reference_reference_reverse()
        {
            base.Nested_include_with_inheritance_reference_reference_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_reference_collection1()
        {
            base.Nested_include_with_inheritance_reference_collection1();
        }

        public override void Nested_include_with_inheritance_reference_collection2()
        {
            base.Nested_include_with_inheritance_reference_collection2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_reference_collection3()
        {
            base.Nested_include_with_inheritance_reference_collection3();
        }

        public override void Nested_include_with_inheritance_reference_collection4()
        {
            base.Nested_include_with_inheritance_reference_collection4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_reference_collection_reverse()
        {
            base.Nested_include_with_inheritance_reference_collection_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_collection_reference1()
        {
            base.Nested_include_with_inheritance_collection_reference1();
        }

        public override void Nested_include_with_inheritance_collection_reference2()
        {
            base.Nested_include_with_inheritance_collection_reference2();
        }

        public override void Nested_include_with_inheritance_collection_reference3()
        {
            base.Nested_include_with_inheritance_collection_reference3();
        }

        public override void Nested_include_with_inheritance_collection_reference4()
        {
            base.Nested_include_with_inheritance_collection_reference4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_collection_reference_reverse()
        {
            base.Nested_include_with_inheritance_collection_reference_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_collection_collection1()
        {
            base.Nested_include_with_inheritance_collection_collection1();
        }

        public override void Nested_include_with_inheritance_collection_collection2()
        {
            base.Nested_include_with_inheritance_collection_collection2();
        }

        public override void Nested_include_with_inheritance_collection_collection3()
        {
            base.Nested_include_with_inheritance_collection_collection3();
        }

        public override void Nested_include_with_inheritance_collection_collection4()
        {
            base.Nested_include_with_inheritance_collection_collection4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_with_inheritance_collection_collection_reverse()
        {
            base.Nested_include_with_inheritance_collection_collection_reverse();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Nested_include_collection_reference_on_non_entity_base()
        {
            base.Nested_include_collection_reference_on_non_entity_base();
        }
    }

    public class InheritanceRelationshipsQueryImpatientFixture : InheritanceRelationshipsQueryRelationalFixture<ImpatientTestStore>
    {
        private readonly DbContextOptions options;

        public InheritanceRelationshipsQueryImpatientFixture()
        {
            var services = new ServiceCollection();

            new ImpatientDbContextOptionsExtension().ApplyServices(services);

            var provider
                = services
                    .AddEntityFrameworkSqlServer()
                    .AddImpatientEFCoreQueryCompiler()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            options
                = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(provider)
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-inheritancerelationships; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            using (var context = new InheritanceRelationshipsContext(options))
            {
                context.Database.EnsureCreated();

                InheritanceRelationshipsModelInitializer.Seed(context);
            }
        }

        public override InheritanceRelationshipsContext CreateContext(ImpatientTestStore testStore)
        {
            var context = new InheritanceRelationshipsContext(options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore();
        }
    }
}
