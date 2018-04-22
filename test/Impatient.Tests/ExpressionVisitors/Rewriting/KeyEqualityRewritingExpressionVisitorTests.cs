using Impatient.Metadata;
using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Rewriting;
using Impatient.Query.ExpressionVisitors.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Tests.ExpressionVisitors.Rewriting
{
    [TestClass]
    public class KeyEqualityRewritingExpressionVisitorTests
    {
        private class MyClass1
        {
            public int Id { get; set; }

            public DateTime Prop1 { get; set; }

            public int Nav1Id { get; set; }

            public MyClass2 Nav1 { get; set; }
        }

        private class MyClass2
        {
            public int Id { get; set; }
        }

        [TestMethod]
        public void Key_equality_Where_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            from x2 in x
                            where x1 == x2
                            select true,
                output: x => from x1 in x
                             from x2 in x
                             where x1.Id == x2.Id
                             select true);
        }

#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
        [TestMethod]
        public void Key_equality_Where_from_parameter_to_null()
        {
            AssertTransformation(
                input: x => from x1 in x
                            where x1 == null
                            select true,
                output: x => from x1 in x
                             where ((int?)x1.Id) == null
                             select true);
        }
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'

        [TestMethod]
        public void Key_equality_Where_from_member_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            from x2 in x
                            let y = new { x1, x2 }
                            where y.x1 == y.x2
                            select true,
                output: x => from x1 in x
                             from x2 in x
                             let y = new { x1, x2 }
                             where y.x1.Id == y.x2.Id
                             select true);
        }

        [TestMethod]
        public void Key_equality_Where_from_navigation_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            from x2 in x
                            let y = new { x1, x2 }
                            where y.x1.Nav1 == y.x2.Nav1
                            select true,
                output: x => from x1 in x
                             from x2 in x
                             let y = new { x1, x2 }
                             where y.x1.Nav1Id == y.x2.Nav1Id
                             select true);
        }

        [TestMethod]
        public void Key_equality_Join_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            join x2 in x on x1 equals x2
                            select true,
                output: x => from x1 in x
                             join x2 in x on x1.Id equals x2.Id
                             select true);
        }

        [TestMethod]
        public void Key_equality_Join_from_member_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            let y1 = x1
                            join x2 in x on y1 equals x2
                            select true,
                output: x => from x1 in x
                             let y1 = x1
                             join x2 in x on y1.Id equals x2.Id
                             select true);
        }

        [TestMethod]
        public void Key_equality_Join_from_navigation_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            let y1 = x1
                            join x2 in x on y1.Nav1 equals x2.Nav1
                            select true,
                output: x => from x1 in x
                             let y1 = x1
                             join x2 in x on y1.Nav1Id equals x2.Nav1Id
                             select true);
        }

        [TestMethod]
        public void Key_equality_GroupJoin_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            join x2 in x on x1 equals x2 into x2s
                            select true,
                output: x => from x1 in x
                             join x2 in x on x1.Id equals x2.Id into x2s
                             select true);
        }

        [TestMethod]
        public void Key_equality_GroupJoin_from_member_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            let y1 = x1
                            join x2 in x on y1 equals x2 into x2s
                            select true,
                output: x => from x1 in x
                             let y1 = x1
                             join x2 in x on y1.Id equals x2.Id into x2s
                             select true);
        }

        [TestMethod]
        public void Key_equality_GroupJoin_from_navigation_from_parameter()
        {
            AssertTransformation(
                input: x => from x1 in x
                            let y1 = x1
                            join x2 in x on y1.Nav1 equals x2.Nav1 into x2s
                            select true,
                output: x => from x1 in x
                             let y1 = x1
                             join x2 in x on y1.Nav1Id equals x2.Nav1Id into x2s
                             select true);
        }

        private static void AssertTransformation<TResult>(
           Expression<Func<IQueryable<MyClass1>, TResult>> input,
           Expression<Func<IQueryable<MyClass1>, TResult>> output)
        {
            var myClass1KeyParameter = Expression.Parameter(typeof(MyClass1), "m1");

            var myClass1KeyDescriptor
                = new PrimaryKeyDescriptor(
                    typeof(MyClass1),
                    Expression.Lambda(
                        Expression.MakeMemberAccess(
                            myClass1KeyParameter,
                            typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Id))),
                        myClass1KeyParameter));

            var myClass2KeyParameter = Expression.Parameter(typeof(MyClass2), "m2");

            var myClass2KeyDescriptor
                = new PrimaryKeyDescriptor(
                    typeof(MyClass2),
                    Expression.Lambda(
                        Expression.MakeMemberAccess(
                            myClass2KeyParameter,
                            typeof(MyClass2).GetRuntimeProperty(nameof(MyClass2.Id))),
                        myClass2KeyParameter));

            var myClass1NavigationDescriptor
                = new NavigationDescriptor(
                    typeof(MyClass1),
                    typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Nav1)),
                    Expression.Lambda(
                            Expression.MakeMemberAccess(
                                myClass1KeyParameter,
                                typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Nav1Id))),
                            myClass1KeyParameter),
                    myClass2KeyDescriptor.KeySelector,
                    false,
                    Expression.Default(typeof(IQueryable<MyClass2>)));

            var visitor
                = new KeyEqualityRewritingExpressionVisitor(
                    new DescriptorSet(
                        primaryKeyDescriptors: new[]
                        {
                            myClass1KeyDescriptor,
                            myClass2KeyDescriptor,
                        },
                        navigationDescriptors: new[]
                        {
                            myClass1NavigationDescriptor,
                        }));

            var result = visitor.Visit(input.Body);

            var hasher = new HashingExpressionVisitor();

            hasher.Visit(result);

            var inputHash = hasher.HashCode;

            hasher.Reset();

            hasher.Visit(output.Body);

            var outputHash = hasher.HashCode;

            Assert.AreEqual(inputHash, outputHash, "Output expression trees do not match");
        }
    }
}
