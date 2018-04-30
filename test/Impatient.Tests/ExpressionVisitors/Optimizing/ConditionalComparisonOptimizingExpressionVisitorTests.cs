using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace Impatient.Tests.ExpressionVisitors.Optimizing
{
    [TestClass]
    public class ConditionalComparisonOptimizingExpressionVisitorTests
    {
        [TestMethod]
        public void Condition_NotEqualNull_IfTrue_NewObject_IfFalse_Null_Equal_Null()
        {
            AssertTransformation(
                t => (t.ReferenceTypeProperty != null ? new { } : null) == null,
                t => t.ReferenceTypeProperty == null);
        }

        [TestMethod]
        public void Condition_EqualNull_IfTrue_Null_Else_ReferenceTypeExpression_Equal_Constant()
        {
            AssertTransformation(
                t => (t == null ? null : t.ReferenceTypeProperty) == "string",
                t => t != null && t.ReferenceTypeProperty == "string");
        }

        [TestMethod]
        public void ConditionEqualCondition_SameTest_DifferentIfTrue_DifferentIfFalse()
        {
            var test = new[] { true };
            var ifTrue = new[] { 0, 1 };
            var ifFalse = new[] { 0, 1 };

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) == (test[0] ? ifTrue[1] : ifFalse[1]),
                t => (test[0] ? ifTrue[0] == ifTrue[1] : ifFalse[0] == ifFalse[1]));

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) != (test[0] ? ifTrue[1] : ifFalse[1]),
                t => (test[0] ? ifTrue[0] != ifTrue[1] : ifFalse[0] != ifFalse[1]));
        }

        [TestMethod]
        public void ConditionEqualCondition_DifferentTest_SameIfTrue_SameIfFalse()
        {
            var test = new[] { true, false };
            var ifTrue = new[] { 0 };
            var ifFalse = new[] { 0 };

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) == (test[1] ? ifTrue[0] : ifFalse[0]),
                t => test[0] == test[1]);

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) != (test[1] ? ifTrue[0] : ifFalse[0]),
                t => test[0] != test[1]);
        }

        [TestMethod]
        public void ConditionEqualCondition_DifferentTest_SameIfTrue_DifferentIfFalse()
        {
            var test = new[] { true, false };
            var ifTrue = new[] { 0 };
            var ifFalse = new[] { 0, 1 };

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) == (test[1] ? ifTrue[0] : ifFalse[1]),
                t => (test[0] && test[1]) || ((test[0] && ifTrue[0] == ifFalse[1]) || ((test[1] && ifTrue[0] == ifFalse[0]) || ifFalse[0] == ifFalse[1])));

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) != (test[1] ? ifTrue[0] : ifFalse[1]),
                t => (test[0] != test[1] && ((test[0] && ifTrue[0] != ifFalse[1]) || (test[1] && ifTrue[0] != ifFalse[0]))) || (!test[0] && (!test[1] && ifFalse[0] != ifFalse[1])));
        }

        [TestMethod]
        public void ConditionEqualCondition_DifferentTest_DifferentIfTrue_SameIfFalse()
        {
            var test = new[] { true, false };
            var ifTrue = new[] { 0, 1 };
            var ifFalse = new[] { 0 };

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) == (test[1] ? ifTrue[1] : ifFalse[0]),
                t => (!test[0] && !test[1]) || ((test[0] && ifTrue[0] == ifFalse[0]) || ((test[1] && ifTrue[1] == ifFalse[0]) || ifTrue[0] == ifTrue[1])));

            AssertTransformation(
                t => (test[0] ? ifTrue[0] : ifFalse[0]) != (test[1] ? ifTrue[1] : ifFalse[0]),
                t => (test[0] != test[1] && ((test[0] && ifTrue[0] != ifFalse[0]) || (test[1] && ifTrue[1] != ifFalse[0]))) || (!test[0] && (!test[1] && ifTrue[0] != ifTrue[1])));
        }

        private static void AssertTransformation<T>(
            Expression<Func<TestClass, T>> input,
            Expression<Func<TestClass, T>> output)
        {
            AssertTransformation(input.Body, output.Body);
        }

        private static void AssertTransformation(
            Expression input,
            Expression output)
        {
            var visitor = new ConditionalComparisonOptimizingExpressionVisitor();

            var result = visitor.Visit(input);

            var inputHash = ExpressionEqualityComparer.Instance.GetHashCode(result);

            var outputHash = ExpressionEqualityComparer.Instance.GetHashCode(output);

            if (inputHash != outputHash)
            {
                Assert.Fail($"Output expression trees do not match.\r\nExpected: {output}\r\nActual: {result}");
            }
        }

        private class TestClass
        {
            public int ValueTypeProperty { get; set; }

            public string ReferenceTypeProperty { get; set; }

            public bool? NullableTypeProperty { get; set; }

            public string StringProperty { get; set; }
        }
    }
}
