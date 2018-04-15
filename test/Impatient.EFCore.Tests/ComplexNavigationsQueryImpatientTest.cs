using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryTestBase<ImpatientTestStore, ComplexNavigationsQueryImpatientFixture>
    {
        public ComplexNavigationsQueryImpatientTest(ComplexNavigationsQueryImpatientFixture fixture) : base(fixture)
        {
            GetType().BaseType
                 .GetField("<ResultAsserter>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                 .SetValue(this, new ComplexNavigationsQueryResultAsserter());
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void GroupJoin_reference_to_group_in_OrderBy()
        {
            base.GroupJoin_reference_to_group_in_OrderBy();
        }

        public override void Multiple_complex_includes()
        {
            var expectedIncludes = new List<IExpectedInclude>
            {
                new ExpectedInclude<Level1>(l1 => l1.OneToOne_Optional_FK, "OneToOne_Optional_FK"),
                new ExpectedInclude<Level2>(l2 => l2.OneToMany_Optional, "OneToMany_Optional", navigationPath: "OneToOne_Optional_FK"),
                new ExpectedInclude<Level1>(l1 => l1.OneToMany_Optional, "OneToMany_Optional"),
                new ExpectedInclude<Level2>(l2 => l2.OneToOne_Optional_FK, "OneToOne_Optional_FK", navigationPath: "OneToMany_Optional")
            };

            Func<IQueryable<Level1>, IQueryable<Level1>> func = l1s =>
                l1s
                    .Include(e => e.OneToOne_Optional_FK)
                    .ThenInclude(e => e.OneToMany_Optional)
                    .Include(e => e.OneToMany_Optional)
                    .ThenInclude(e => e.OneToOne_Optional_FK);

            using (var context = CreateContext())
            {
                var actual = func(Set<Level1>(context)).ToList();
                var expected = func(ExpectedSet<Level1>()).ToList();

                /*if (elementSorter != null)
                {
                    actual = actual.OrderBy(elementSorter).ToList();
                    expected = expected.OrderBy(elementSorter).ToList();
                }

                if (clientProjection != null)
                {
                    actual = actual.Select(clientProjection).ToList();
                    expected = expected.Select(clientProjection).ToList();
                }*/

                ResultAsserter.AssertResult(expected, actual, expectedIncludes);
            }
        }

        private class ComplexNavigationsQueryResultAsserter : QueryResultAsserter
        {
            private readonly MethodInfo _assertElementMethodInfo;
            private readonly MethodInfo _assertCollectionMethodInfo;

            public ComplexNavigationsQueryResultAsserter()
            {
                _assertElementMethodInfo = GetType().GetTypeInfo().GetDeclaredMethod(nameof(AssertElement))
                                      ?? typeof(ComplexNavigationsQueryResultAsserter).GetTypeInfo().GetDeclaredMethod(nameof(AssertElement));

                _assertCollectionMethodInfo = GetType().GetTypeInfo().GetDeclaredMethod(nameof(AssertCollection))
                                              ?? typeof(ComplexNavigationsQueryResultAsserter).GetTypeInfo().GetDeclaredMethod(nameof(AssertCollection));

            }

            protected override void AssertCollection<TElement>(IEnumerable<TElement> expected, IEnumerable<TElement> actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                if (expected != null && actual != null)
                {
                    if ((object)expected is IEnumerable<Level1> expectedLevel1 && (object)actual is IEnumerable<Level1> actualLevel1)
                    {
                        var expectedListLevel1 = _path.Any() ? expectedLevel1.OrderBy(l1 => l1.Id).ToList() : expectedLevel1.ToList();
                        var actualListLevel1 = _path.Any() ? actualLevel1.OrderBy(l1 => l1.Id).ToList() : actualLevel1.ToList();

                        for (int i = 0; i < expectedListLevel1.Count; i++)
                        {
                            _fullPath.Push("[" + i + "]");
                            AssertLevel1(expectedListLevel1[i], actualListLevel1[i], expectedIncludes);
                            _fullPath.Pop();
                        }

                        return;
                    }

                    if ((object)expected is IEnumerable<Level2> expectedLevel2 && (object)actual is IEnumerable<Level2> actualLevel2)
                    {
                        var expectedListLevel2 = _path.Any() ? expectedLevel2.OrderBy(l2 => l2.Id).ToList() : expectedLevel2.ToList();
                        var actualListLevel2 = _path.Any() ? actualLevel2.OrderBy(l2 => l2.Id).ToList() : actualLevel2.ToList();

                        for (int i = 0; i < expectedListLevel2.Count; i++)
                        {
                            _fullPath.Push("[" + i + "]");
                            AssertLevel2(expectedListLevel2[i], actualListLevel2[i], expectedIncludes);
                            _fullPath.Pop();
                        }

                        return;
                    }

                    if ((object)expected is IEnumerable<Level3> expectedLevel3 && (object)actual is IEnumerable<Level3> actualLevel3)
                    {
                        var expectedListLevel3 = _path.Any() ? expectedLevel3.OrderBy(l3 => l3.Id).ToList() : expectedLevel3.ToList();
                        var actualListLevel3 = _path.Any() ? actualLevel3.OrderBy(l3 => l3.Id).ToList() : actualLevel3.ToList();

                        for (int i = 0; i < expectedListLevel3.Count; i++)
                        {
                            _fullPath.Push("[" + i + "]");
                            AssertLevel3(expectedListLevel3[i], actualListLevel3[i], expectedIncludes);
                            _fullPath.Pop();
                        }

                        return;
                    }

                    if ((object)expected is IEnumerable<Level4> expectedLevel4 && (object)actual is IEnumerable<Level4> actualLevel4)
                    {
                        List<Level4> expectedListLevel4 = _path.Any() ? expectedLevel4.OrderBy(l4 => l4.Id).ToList() : expectedLevel4.ToList();
                        List<Level4> actualListLevel4 = _path.Any() ? actualLevel4.OrderBy(l4 => l4.Id).ToList() : actualLevel4.ToList();

                        for (int i = 0; i < expectedListLevel4.Count; i++)
                        {
                            _fullPath.Push("[" + i + "]");
                            AssertLevel4(expectedListLevel4[i], actualListLevel4[i], expectedIncludes);
                            _fullPath.Pop();
                        }

                        return;
                    }
                }

                base.AssertCollection(expected, actual, expectedIncludes);
            }

            protected override void AssertElement<TElement>(TElement expected, TElement actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                if (expected != null && actual != null)
                {
                    Assert.Equal(expected.GetType(), actual.GetType());

                    if ((object)expected is Level1 expectedLevel1)
                    {
                        AssertLevel1(expectedLevel1, (Level1)(object)actual, expectedIncludes);

                        return;
                    }

                    if ((object)expected is Level2 expectedLevel2)
                    {
                        AssertLevel2(expectedLevel2, (Level2)(object)actual, expectedIncludes);

                        return;
                    }

                    if ((object)expected is Level3 expectedLevel3)
                    {
                        AssertLevel3(expectedLevel3, (Level3)(object)actual, expectedIncludes);

                        return;
                    }

                    if ((object)expected is Level4 expectedLevel4)
                    {
                        AssertLevel4(expectedLevel4, (Level4)(object)actual, expectedIncludes);

                        return;
                    }
                }

                base.AssertElement(expected, actual, expectedIncludes);
            }

            private void AssertLevel1(Level1 expected, Level1 actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
                Assert.Equal(expected.Date, actual.Date);

                ProcessIncludes(expected, actual, expectedIncludes);
            }

            private void AssertLevel2(Level2 expected, Level2 actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
                Assert.Equal(expected.Date, actual.Date);
                Assert.Equal(expected.Level1_Optional_Id, actual.Level1_Optional_Id);
                Assert.Equal(expected.Level1_Required_Id, actual.Level1_Required_Id);

                ProcessIncludes(expected, actual, expectedIncludes);
            }

            private void AssertLevel3(Level3 expected, Level3 actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
                Assert.Equal(expected.Level2_Optional_Id, actual.Level2_Optional_Id);
                Assert.Equal(expected.Level2_Required_Id, actual.Level2_Required_Id);

                ProcessIncludes(expected, actual, expectedIncludes);
            }

            private void AssertLevel4(Level4 expected, Level4 actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                Assert.Equal(expected.Id, actual.Id);
                Assert.Equal(expected.Name, actual.Name);
                Assert.Equal(expected.Level3_Optional_Id, actual.Level3_Optional_Id);
                Assert.Equal(expected.Level3_Required_Id, actual.Level3_Required_Id);

                ProcessIncludes(expected, actual, expectedIncludes);
            }

            public override void AssertResult(object expected, object actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                _path = new List<string>();
                _fullPath = new Stack<string>();
                _fullPath.Push("root");

                AssertObject(expected, actual, expectedIncludes);
            }

            protected override void AssertObject(object expected, object actual, IEnumerable<IExpectedInclude> expectedIncludes)
            {
                if (expected == null && actual == null)
                {
                    return;
                }

                Assert.Equal(expected == null, actual == null);

                var expectedType = expected.GetType();
                if (expectedType.GetTypeInfo().IsGenericType
                    && expectedType.GetTypeInfo().ImplementedInterfaces.Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
                {
                    var typeArgument = expectedType.GenericTypeArguments[0];
                    var assertCollectionMethodInfo = _assertCollectionMethodInfo.MakeGenericMethod(typeArgument);
                    assertCollectionMethodInfo.Invoke(this, new[] { expected, actual, expectedIncludes });
                }
                else
                {
                    var assertElementMethodInfo = _assertElementMethodInfo.MakeGenericMethod(expectedType);
                    assertElementMethodInfo.Invoke(this, new[] { expected, actual, expectedIncludes });
                }
            }
        }
    }

    public class ComplexNavigationsQueryImpatientFixture : ComplexNavigationsQueryFixtureBase<ImpatientTestStore>
    {
        private readonly DbContextOptions options;

        public ComplexNavigationsQueryImpatientFixture()
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
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-complex-navigations; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            using (var context = new ComplexNavigationsContext(options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    ComplexNavigationsModelInitializer.Seed(context, tableSplitting: false);
                }
            }
        }

        public override ComplexNavigationsContext CreateContext(ImpatientTestStore testStore)
        {
            var context = new ComplexNavigationsContext(options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore();
        }
    }
}
