using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class QueryFilterFuncletizationImpatientTest : QueryFilterFuncletizationTestBase<QueryFilterFuncletizationImpatientFixture>
    {
        public QueryFilterFuncletizationImpatientTest(QueryFilterFuncletizationImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = "StackOverflow due to interdependence of query filters.")]
        public override void Using_Context_set_method_in_filter_works()
        {
            base.Using_Context_set_method_in_filter_works();
        }

        [Fact(Skip = "StackOverflow due to interdependence of query filters.")]
        public override void Using_DbSet_in_filter_works()
        {
            base.Using_DbSet_in_filter_works();
        }

        [Fact]
        [Trait("Impatient", "Overridden for exception difference")]
        public override void Local_variable_from_OnModelCreating_can_throw_exception()
        {
            using (var context = CreateContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Set<LocalVariableErrorFilter>().ToList());
            }
        }

        [Fact]
        [Trait("Impatient", "Overridden for exception difference")]
        public override void DbContext_property_chain_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                //Assert.Throws<NullReferenceException>(() => context.Set<PropertyChainFilter>().ToList());
                Assert.Throws<InvalidOperationException>(() => context.Set<PropertyChainFilter>().ToList());

                context.IndirectionFlag = new Indirection { Enabled = false };
                var entity = Assert.Single(context.Set<PropertyChainFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.IndirectionFlag = new Indirection { Enabled = true };
                entity = Assert.Single(context.Set<PropertyChainFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [Fact]
        [Trait("Impatient", "Overridden for exception difference")]
        public override void DbContext_property_method_call_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                //Assert.Throws<NullReferenceException>(() => context.Set<PropertyMethodCallFilter>().ToList());
                Assert.Throws<InvalidOperationException>(() => context.Set<PropertyMethodCallFilter>().ToList());

                context.IndirectionFlag = new Indirection();
                var entity = Assert.Single(context.Set<PropertyMethodCallFilter>().ToList());
                Assert.Equal(2, entity.Tenant);
            }
        }

        [Fact]
        [Trait("Impatient", "Overridden for exception difference")]
        public override void EntityTypeConfiguration_DbContext_property_chain_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                //Assert.Throws<NullReferenceException>(() => context.Set<EntityTypeConfigurationPropertyChainFilter>().ToList());
                Assert.Throws<InvalidOperationException>(() => context.Set<EntityTypeConfigurationPropertyChainFilter>().ToList());

                context.IndirectionFlag = new Indirection { Enabled = false };
                var entity = Assert.Single(context.Set<EntityTypeConfigurationPropertyChainFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.IndirectionFlag = new Indirection { Enabled = true };
                entity = Assert.Single(context.Set<EntityTypeConfigurationPropertyChainFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [Fact]
        [Trait("Impatient", "Overridden for exception difference")]
        public override void Extension_method_DbContext_property_chain_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                //Assert.Throws<NullReferenceException>(() => context.Set<ExtensionContextFilter>().ToList());
                Assert.Throws<InvalidOperationException>(() => context.Set<ExtensionContextFilter>().ToList());

                context.IndirectionFlag = new Indirection { Enabled = false };
                var entity = Assert.Single(context.Set<ExtensionContextFilter>().ToList());
                Assert.False(entity.IsEnabled);

                context.IndirectionFlag = new Indirection { Enabled = true };
                entity = Assert.Single(context.Set<ExtensionContextFilter>().ToList());
                Assert.True(entity.IsEnabled);
            }
        }

        [Fact]
        [Trait("Impatient", "Overridden for exception difference")]
        public override void Remote_method_DbContext_property_method_call_is_parameterized()
        {
            using (var context = CreateContext())
            {
                // This throws because IndirectionFlag is null
                //Assert.Throws<NullReferenceException>(() => context.Set<RemoteMethodParamsFilter>().ToList());
                Assert.Throws<InvalidOperationException>(() => context.Set<RemoteMethodParamsFilter>().ToList());

                context.IndirectionFlag = new Indirection();
                var entity = Assert.Single(context.Set<RemoteMethodParamsFilter>().ToList());
                Assert.Equal(2, entity.Tenant);
            }
        }
    }

    public class QueryFilterFuncletizationImpatientFixture : QueryFilterFuncletizationRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
