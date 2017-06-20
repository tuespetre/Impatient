using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Tests.ExpressionVisitors.Optimizing
{
    [TestClass]
    public class SelectorPushdownExpressionVisitorTests
    {
        private class MyClass1
        {
            public DateTime Prop1 { get; set; }
        }

        [TestMethod]
        public void Works_with_ElementAt()
        {
            AssertTransformation(
                input: q => q.ElementAt(0).Prop1,
                output: q => q.Select(x => x.Prop1).ElementAt(0));
        }

        [TestMethod]
        public void Works_with_ElementAtOrDefault()
        {
            AssertTransformation(
                input: q => q.ElementAtOrDefault(0).Prop1,
                output: q => q.Select(x => x.Prop1).ElementAtOrDefault(0));
        }

        [TestMethod]
        public void Works_with_First()
        {
            AssertTransformation(
                input: q => q.First().Prop1,
                output: q => q.Select(x => x.Prop1).First());
        }

        [TestMethod]
        public void Works_with_First_predicate()
        {
            AssertTransformation(
                input: q => q.First(x => x.Prop1.Day > 1).Prop1,
                output: q => q.Where(x => x.Prop1.Day > 1).Select(x => x.Prop1).First());
        }

        [TestMethod]
        public void Works_with_FirstOrDefault()
        {
            AssertTransformation(
                input: q => q.FirstOrDefault().Prop1,
                output: q => q.Select(x => x.Prop1).FirstOrDefault());
        }

        [TestMethod]
        public void Works_with_FirstOrDefault_predicate()
        {
            AssertTransformation(
                input: q => q.FirstOrDefault(x => x.Prop1.Day > 1).Prop1,
                output: q => q.Where(x => x.Prop1.Day > 1).Select(x => x.Prop1).FirstOrDefault());
        }

        [TestMethod]
        public void Works_with_Last()
        {
            AssertTransformation(
                input: q => q.Last().Prop1,
                output: q => q.Select(x => x.Prop1).Last());
        }

        [TestMethod]
        public void Works_with_Last_predicate()
        {
            AssertTransformation(
                input: q => q.Last(x => x.Prop1.Day > 1).Prop1,
                output: q => q.Where(x => x.Prop1.Day > 1).Select(x => x.Prop1).Last());
        }

        [TestMethod]
        public void Works_with_LastOrDefault()
        {
            AssertTransformation(
                input: q => q.LastOrDefault().Prop1,
                output: q => q.Select(x => x.Prop1).LastOrDefault());
        }

        [TestMethod]
        public void Works_with_LastOrDefault_predicate()
        {
            AssertTransformation(
                input: q => q.LastOrDefault(x => x.Prop1.Day > 1).Prop1,
                output: q => q.Where(x => x.Prop1.Day > 1).Select(x => x.Prop1).LastOrDefault());
        }

        [TestMethod]
        public void Works_with_Single()
        {
            AssertTransformation(
                input: q => q.Single().Prop1,
                output: q => q.Select(x => x.Prop1).Single());
        }

        [TestMethod]
        public void Works_with_Single_predicate()
        {
            AssertTransformation(
                input: q => q.Single(x => x.Prop1.Day > 1).Prop1,
                output: q => q.Where(x => x.Prop1.Day > 1).Select(x => x.Prop1).Single());
        }

        [TestMethod]
        public void Works_with_SingleOrDefault()
        {
            AssertTransformation(
                input: q => q.SingleOrDefault().Prop1,
                output: q => q.Select(x => x.Prop1).SingleOrDefault());
        }

        [TestMethod]
        public void Works_with_SingleOrDefault_predicate()
        {
            AssertTransformation(
                input: q => q.SingleOrDefault(x => x.Prop1.Day > 1).Prop1,
                output: q => q.Where(x => x.Prop1.Day > 1).Select(x => x.Prop1).SingleOrDefault());
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
