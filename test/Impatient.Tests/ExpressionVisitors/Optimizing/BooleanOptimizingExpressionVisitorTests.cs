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
            public bool BooleanProperty { get; set; }

            public string StringProperty { get; set; }
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
                input: x => !(!(x.BooleanProperty)),
                output: x => x.BooleanProperty);
        }

        [TestMethod]
        public void Not_Not_Not()
        {
            AssertTransformation(
                input: x => !(!(!(x.BooleanProperty))),
                output: x => !(x.BooleanProperty));
        }

        [TestMethod]
        public void Not_Not_Not_Not()
        {
            AssertTransformation(
                input: x => !(!(!(!(x.BooleanProperty)))),
                output: x => x.BooleanProperty);
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
                input: x => !(x.BooleanProperty && x.BooleanProperty),
                output: x => !(x.BooleanProperty) || !(x.BooleanProperty));
        }

        [TestMethod]
        public void Not_Binary_OrElse()
        {
            AssertTransformation(
                input: x => !(x.BooleanProperty || x.BooleanProperty),
                output: x => !(x.BooleanProperty) && !(x.BooleanProperty));
        }

        [TestMethod]
        public void Not_Binary_complex1()
        {
            AssertTransformation(
                input: x => !(((x.BooleanProperty || x.BooleanProperty) && number1 <= number2) && (number3 < number4 || x.BooleanProperty == false)),
                output: x => ((!x.BooleanProperty && !x.BooleanProperty) || number1 > number2) || (number3 >= number4 && x.BooleanProperty));
        }

        [TestMethod]
        public void True_eq_true_eq_boolean()
        {
            AssertTransformation(
                input: x => true == (true == (x.BooleanProperty)),
                output: x => x.BooleanProperty);
        }

        [TestMethod]
        public void Property_Equal_Value_Equal_NullableBoolean()
        {
            AssertTransformation(
                input: x => (x.StringProperty == "string") == (bool?)true,
                output: x => (x.StringProperty == "string"));
        }

        [TestMethod]
        public void Left_Equal_NotRight()
        {
            bool left = true;
            bool right = true;

            AssertTransformation(
                input: x => left == !right,
                output: x => left != right);
        }

        [TestMethod]
        public void Left_Equal_NotNullableRight()
        {
            bool left = true;
            bool? right = true;

            AssertTransformation(
                input: x => left == !right,
                output: x => left == !right);
        }

        [TestMethod]
        public void NullableLeft_Equal_NotNullableRight()
        {
            bool? left = true;
            bool? right = true;

            AssertTransformation(
                input: x => left == !right,
                output: x => left == !right);
        }

        [TestMethod]
        public void NotLeft_Equal_NotRight()
        {
            bool left = true;
            bool right = true;

            AssertTransformation(
                input: x => !left == !right,
                output: x => left == right);
        }

        [TestMethod]
        public void NotLeft_Equal_Right()
        {
            bool left = true;
            bool right = true;

            AssertTransformation(
                input: x => !left == right,
                output: x => left != right);
        }

        [TestMethod]
        public void NotLeft_NotEqual_NotRight()
        {
            var left = true;
            var right = true;

            AssertTransformation(
                input: x => !left != !right,
                output: x => left != right);
        }

        [TestMethod]
        public void NotLeft_NotEqual_Right()
        {
            var left = true;
            var right = true;

            AssertTransformation(
                input: x => !left != right,
                output: x => left == right);
        }

        [TestMethod]
        public void Left_NotEqual_NotRight()
        {
            var left = true;
            var right = true;

            AssertTransformation(
                input: x => left != !right,
                output: x => left == right);
        }

        [TestMethod]
        public void Not_Left_Equal_NotRight()
        {
            bool left = true;
            bool right = true;

            AssertTransformation(
                input: x => !(left == !right),
                output: x => left == right);
        }

        [TestMethod]
        public void Not_Left_Equal_NotNullableRight()
        {
            bool left = true;
            bool? right = true;

            AssertTransformation(
                input: x => !(left == !right),
                output: x => left != !right);
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

            if (inputHash != outputHash)
            {
                Assert.Fail($"Output expression trees do not match.\r\nExpected: {output}\r\nActual: {result}");
            }
        }
    }
}
