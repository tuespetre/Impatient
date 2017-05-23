using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Tests.ExpressionVisitors
{
    [TestClass]
    public class OperatorSplittingExpressionVisitorTests
    {
        #region selectors

        [TestMethod]
        public void Works_with_Average_decimal()
        {
            AssertTransformation<object, decimal>(
                input: q => q.Average(x => new decimal()),
                output: q => q.Select(x => new decimal()).Average());
        }

        [TestMethod]
        public void Works_with_Average_double()
        {
            AssertTransformation<object, double>(
                input: q => q.Average(x => new double()),
                output: q => q.Select(x => new double()).Average());
        }

        [TestMethod]
        public void Works_with_Average_int()
        {
            AssertTransformation<object, double>(
                input: q => q.Average(x => new int()),
                output: q => q.Select(x => new int()).Average());
        }

        [TestMethod]
        public void Works_with_Average_long()
        {
            AssertTransformation<object, double>(
                input: q => q.Average(x => new long()),
                output: q => q.Select(x => new long()).Average());
        }

        [TestMethod]
        public void Works_with_Average_float()
        {
            AssertTransformation<object, float>(
                input: q => q.Average(x => new float()),
                output: q => q.Select(x => new float()).Average());
        }

        [TestMethod]
        public void Works_with_Average_decimal_nullable()
        {
            AssertTransformation<object, decimal?>(
                input: q => q.Average(x => new decimal?()),
                output: q => q.Select(x => new decimal?()).Average());
        }

        [TestMethod]
        public void Works_with_Average_double_nullable()
        {
            AssertTransformation<object, double?>(
                input: q => q.Average(x => new double?()),
                output: q => q.Select(x => new double?()).Average());
        }

        [TestMethod]
        public void Works_with_Average_int_nullable()
        {
            AssertTransformation<object, double?>(
                input: q => q.Average(x => new int?()),
                output: q => q.Select(x => new int?()).Average());
        }

        [TestMethod]
        public void Works_with_Average_long_nullable()
        {
            AssertTransformation<object, double?>(
                input: q => q.Average(x => new long?()),
                output: q => q.Select(x => new long?()).Average());
        }

        [TestMethod]
        public void Works_with_Average_float_nullable()
        {
            AssertTransformation<object, float?>(
                input: q => q.Average(x => new float?()),
                output: q => q.Select(x => new float?()).Average());
        }

        [TestMethod]
        public void Works_with_Max()
        {
            AssertTransformation<object, object>(
                input: q => q.Max(x => x),
                output: q => q.Select(x => x).Max());
        }

        [TestMethod]
        public void Works_with_Min()
        {
            AssertTransformation<object, object>(
                input: q => q.Min(x => x),
                output: q => q.Select(x => x).Min());
        }

        [TestMethod]
        public void Works_with_Sum_decimal()
        {
            AssertTransformation<object, decimal>(
                input: q => q.Sum(x => new decimal()),
                output: q => q.Select(x => new decimal()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_double()
        {
            AssertTransformation<object, double>(
                input: q => q.Sum(x => new double()),
                output: q => q.Select(x => new double()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_int()
        {
            AssertTransformation<object, int>(
                input: q => q.Sum(x => new int()),
                output: q => q.Select(x => new int()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_long()
        {
            AssertTransformation<object, long>(
                input: q => q.Sum(x => new long()),
                output: q => q.Select(x => new long()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_float()
        {
            AssertTransformation<object, float>(
                input: q => q.Sum(x => new float()),
                output: q => q.Select(x => new float()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_decimal_nullable()
        {
            AssertTransformation<object, decimal?>(
                input: q => q.Sum(x => new decimal?()),
                output: q => q.Select(x => new decimal?()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_double_nullable()
        {
            AssertTransformation<object, double?>(
                input: q => q.Sum(x => new double?()),
                output: q => q.Select(x => new double?()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_int_nullable()
        {
            AssertTransformation<object, int?>(
                input: q => q.Sum(x => new int?()),
                output: q => q.Select(x => new int?()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_long_nullable()
        {
            AssertTransformation<object, long?>(
                input: q => q.Sum(x => new long?()),
                output: q => q.Select(x => new long?()).Sum());
        }

        [TestMethod]
        public void Works_with_Sum_float_nullable()
        {
            AssertTransformation<object, float?>(
                input: q => q.Sum(x => new float?()),
                output: q => q.Select(x => new float?()).Sum());
        }

        #endregion

        #region predicates

        [TestMethod]
        public void Works_with_Count()
        {
            AssertTransformation<object, int>(
                input: q => q.Count(x => true),
                output: q => q.Where(x => true).Count());
        }

        [TestMethod]
        public void Works_with_First()
        {
            AssertTransformation<object, object>(
                input: q => q.First(x => true),
                output: q => q.Where(x => true).First());
        }

        [TestMethod]
        public void Works_with_FirstOrDefault()
        {
            AssertTransformation<object, object>(
                input: q => q.FirstOrDefault(x => true),
                output: q => q.Where(x => true).FirstOrDefault());
        }

        [TestMethod]
        public void Works_with_Last()
        {
            AssertTransformation<object, object>(
                input: q => q.Last(x => true),
                output: q => q.Where(x => true).Last());
        }

        [TestMethod]
        public void Works_with_LastOrDefault()
        {
            AssertTransformation<object, object>(
                input: q => q.LastOrDefault(x => true),
                output: q => q.Where(x => true).LastOrDefault());
        }

        [TestMethod]
        public void Works_with_LongCount()
        {
            AssertTransformation<object, long>(
                input: q => q.LongCount(x => true),
                output: q => q.Where(x => true).LongCount());
        }

        [TestMethod]
        public void Works_with_Single()
        {
            AssertTransformation<object, object>(
                input: q => q.Single(x => true),
                output: q => q.Where(x => true).Single());
        }

        [TestMethod]
        public void Works_with_SingleOrDefault()
        {
            AssertTransformation<object, object>(
                input: q => q.SingleOrDefault(x => true),
                output: q => q.Where(x => true).SingleOrDefault());
        }

        #endregion

        private static void AssertTransformation<TSource, TResult>(
            Expression<Func<IQueryable<TSource>, TResult>> input,
            Expression<Func<IQueryable<TSource>, TResult>> output)
        {
            var visitor = new OperatorSplittingExpressionVisitor();

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
