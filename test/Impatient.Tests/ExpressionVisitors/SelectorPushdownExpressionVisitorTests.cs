using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Tests.ExpressionVisitors
{
    [TestClass]
    public class SelectorPushdownExpressionVisitorTests
    {
        private class MyClass1
        {
            public DateTime Prop1 { get; set; }
        }

        [TestMethod]
        public void Works_with_First()
        {
            AssertTransformation(
                input: q => q.First().Prop1,
                output: q => q.Select(x => x.Prop1).First());
        }

        [TestMethod]
        public void Works_with_FirstOrDefault()
        {
            AssertTransformation(
                input: q => q.FirstOrDefault().Prop1,
                output: q => q.Select(x => x.Prop1).FirstOrDefault());
        }

        [TestMethod]
        public void Works_with_Last()
        {
            AssertTransformation(
                input: q => q.Last().Prop1,
                output: q => q.Select(x => x.Prop1).Last());
        }

        [TestMethod]
        public void Works_with_LastOrDefault()
        {
            AssertTransformation(
                input: q => q.LastOrDefault().Prop1,
                output: q => q.Select(x => x.Prop1).LastOrDefault());
        }

        [TestMethod]
        public void Works_with_Single()
        {
            AssertTransformation(
                input: q => q.Single().Prop1,
                output: q => q.Select(x => x.Prop1).Single());
        }

        [TestMethod]
        public void Works_with_SingleOrDefault()
        {
            AssertTransformation(
                input: q => q.SingleOrDefault().Prop1,
                output: q => q.Select(x => x.Prop1).SingleOrDefault());
        }

        [TestMethod]
        public void Works_with_multiple_member_accesses()
        {
            AssertTransformation(
                input: q => q.First().Prop1.TimeOfDay.Hours,
                output: q => q.Select(x => x.Prop1).Select(x => x.TimeOfDay).Select(x => x.Hours).First());
        }

        private static void AssertTransformation<TResult>(
           Expression<Func<IQueryable<MyClass1>, TResult>> input,
           Expression<Func<IQueryable<MyClass1>, TResult>> output)
        {
            var visitor = new SelectorPushdownExpressionVisitor();

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
