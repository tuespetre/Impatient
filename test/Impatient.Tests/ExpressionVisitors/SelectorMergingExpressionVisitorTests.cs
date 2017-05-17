using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Tests.ExpressionVisitors
{
    [TestClass]
    public class SelectorMergingExpressionVisitorTests
    {
        private static readonly IQueryable<object> set1 = new List<object>().AsQueryable();
        private static readonly IQueryable<object> set2 = new List<object>().AsQueryable();

        [TestMethod]
        public void Works_with_GroupBy()
        {
            // Key
            AssertTransformation(
                input: set1.GroupBy(x => x).Select(g => new { Max = g.Max(), Key = g.Key }),
                output: set1.GroupBy(x => x, (k, e) => new { Max = e.Max(), Key = k }));

            // Key, Element
            AssertTransformation(
                input: set1.GroupBy(x => x, x => x).Select(g => new { Max = g.Max(), Key = g.Key }),
                output: set1.GroupBy(x => x, x => x, (k, e) => new { Max = e.Max(), Key = k }));

            // Key, Result
            AssertTransformation(
                input: set1.GroupBy(x => x, (k, e) => new { k, e }).Select(g => new { Max = g.e.Max(), Key = g.k }),
                output: set1.GroupBy(x => x, (k, e) => new { Max = e.Max(), Key = k }));

            // Key, Element, Result
            AssertTransformation(
                input: set1.GroupBy(x => x, x => x, (k, e) => new { k, e }).Select(g => new { Max = g.e.Max(), Key = g.k }),
                output: set1.GroupBy(x => x, x => x, (k, e) => new { Max = e.Max(), Key = k }));

            // Escaped reference to grouping
            var query = set1.GroupBy(x => x).Select(g => g);
            var visitor = new SelectorMergingExpressionVisitor();
            var result = visitor.Visit(query.Expression);
            var groupByCall = (MethodCallExpression)result;
            var selector = ((LambdaExpression)((UnaryExpression)groupByCall.Arguments.Last()).Operand);

            Assert.IsInstanceOfType(selector.Body, typeof(NewExpression));
            Assert.AreEqual(typeof(IGrouping<,>), groupByCall.Method.GetGenericArguments().Last().GetGenericTypeDefinition());

            // Escaped reference to grouping, subsequently resolved
            AssertTransformation(
                input: set1.GroupBy(x => x).Select(g => new { g }).Select(g => new { Max = g.g.Max(), Key = g.g.Key }),
                output: set1.GroupBy(x => x, (k, e) => new { Max = e.Max(), Key = k }));
        }

        [TestMethod]
        public void Works_with_GroupJoin()
        {
            AssertTransformation(
                input: from x in set1
                       join y in set2 on x equals y into y
                       select new { x, y } into z
                       select new { a = z.x.ToString(), b = z.y.ToString() },
                output: from x in set1
                        join y in set2 on x equals y into y
                        select new { a = x.ToString(), b = y.ToString() });
        }

        [TestMethod]
        public void Works_with_Join()
        {
            AssertTransformation(
                input: from x in set1
                       join y in set2 on x equals y
                       select new { x, y } into z
                       select new { a = z.x.ToString(), b = z.y.ToString() },
                output: from x in set1
                        join y in set2 on x equals y
                        select new { a = x.ToString(), b = y.ToString() });

            AssertTransformation(
                input: from x in set1
                       join y in set2 on x equals y
                       let z = new { x, y }
                       select new { a = z.x.ToString(), b = z.y.ToString() },
                output: from x in set1
                        join y in set2 on x equals y
                        select new { a = x.ToString(), b = y.ToString() });
        }

        [TestMethod]
        public void Works_with_Select()
        {
            AssertTransformation(
                input: from x in set1
                       from y in set2
                       select new { x, y } into z
                       select new { a = z.x, b = z.y } into c
                       select new { d = c.a.ToString(), e = c.b.ToString() },
                output: from x in set1
                        from y in set2
                        select new { d = x.ToString(), e = y.ToString() });
        }

        [TestMethod]
        public void Works_with_SelectMany()
        {
            AssertTransformation(
                input: from x in set1
                       from y in set2
                       select new { x, y } into z
                       select new { a = z.x.ToString(), b = z.y.ToString() },
                output: from x in set1
                        from y in set2
                        select new { a = x.ToString(), b = y.ToString() });
        }

        [TestMethod]
        public void Works_with_Zip()
        {
            AssertTransformation(
                input: set1.Zip(set2, (x, y) => new { x, y }).Select(z => new { a = z.x.ToString(), b = z.y.ToString() }),
                output: set1.Zip(set2, (x, y) => new { a = x.ToString(), b = y.ToString() }));
        }

        private static void AssertTransformation(IQueryable input, IQueryable output)
        {
            var visitor = new SelectorMergingExpressionVisitor();

            var resultMethodCall = (MethodCallExpression)visitor.Visit(input.Expression);
            var expectedMethodCall = (MethodCallExpression)output.Expression;

            Assert.AreEqual(expectedMethodCall.Method, resultMethodCall.Method, "Output method calls do not match");

            var resultSelector = resultMethodCall.Arguments.Last();
            var expectedSelector = expectedMethodCall.Arguments.Last();

            var hasher = new HashingExpressionVisitor();

            hasher.Visit(resultSelector);

            var resultSelectorHash = hasher.HashCode;

            hasher.Reset();

            hasher.Visit(expectedSelector);

            var expectedSelectorHash = hasher.HashCode;

            Assert.AreEqual(expectedSelectorHash, resultSelectorHash, "Output selectors' expression trees do not match");
        }
    }
}
