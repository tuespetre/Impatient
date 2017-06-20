using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace Impatient.Tests.ExpressionVisitors.Optimizing
{
    [TestClass]
    public class BooleanOptimizingExpressionVisitorTests
    {
        private static int number1 = 1;
        private static int number2 = 2;
        private static int number3 = 3;
        private static int number4 = 4;

        private class TestClass
        {
            public bool Prop { get; set; }
        }

        [TestMethod]
        public void Not_Constant_true()
        {
            AssertTransformation(
                input: x => !true,
                output: x => false);
        }

        [TestMethod]
        public void Not_Constant_false()
        {
            AssertTransformation(
                input: x => !false,
                output: x => true);
        }

        [TestMethod]
        public void Not_Not()
        {
            AssertTransformation(
                input: x => !(!(x.Prop)),
                output: x => x.Prop);
        }

        [TestMethod]
        public void Not_Not_Not()
        {
            AssertTransformation(
                input: x => !(!(!(x.Prop))),
                output: x => !(x.Prop));
        }

        [TestMethod]
        public void Not_Not_Not_Not()
        {
            AssertTransformation(
                input: x => !(!(!(!(x.Prop)))),
                output: x => x.Prop);
        }

        [TestMethod]
        public void Not_Binary_Equal()
        {
            AssertTransformation(
                input: x => !(number1 == number2),
                output: x => number1 != number2);
        }

        [TestMethod]
        public void Not_Binary_NotEqual()
        {
            AssertTransformation(
                input: x => !(number1 != number2),
                output: x => number1 == number2);
        }

        [TestMethod]
        public void Not_Binary_LessThan()
        {
            AssertTransformation(
                input: x => !(number1 < number2),
                output: x => number1 >= number2);
        }

        [TestMethod]
        public void Not_Binary_LessThanOrEqual()
        {
            AssertTransformation(
                input: x => !(number1 <= number2),
                output: x => number1 > number2);
        }

        [TestMethod]
        public void Not_Binary_GreaterThan()
        {
            AssertTransformation(
                input: x => !(number1 > number2),
                output: x => number1 <= number2);
        }

        [TestMethod]
        public void Not_Binary_GreaterThanOrEqual()
        {
            AssertTransformation(
                input: x => !(number1 >= number2),
                output: x => number1 < number2);
        }

        [TestMethod]
        public void Not_Binary_AndAlso()
        {
            AssertTransformation(
                input: x => !(x.Prop && x.Prop),
                output: x => !(x.Prop) || !(x.Prop));
        }

        [TestMethod]
        public void Not_Binary_OrElse()
        {
            AssertTransformation(
                input: x => !(x.Prop || x.Prop),
                output: x => !(x.Prop) && !(x.Prop));
        }

        [TestMethod]
        public void Not_Binary_complex1()
        {
            AssertTransformation(
                input: x => !(((x.Prop || x.Prop) && number1 <= number2) && (number3 < number4 || x.Prop == false)),
                output: x => ((!x.Prop && !x.Prop) || number1 > number2) || (number3 >= number4 && x.Prop));
        }

        private static void AssertTransformation(
            Expression<Func<TestClass, bool>> input,
            Expression<Func<TestClass, bool>> output)
        {
            var visitor = new BooleanOptimizingExpressionVisitor();

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
