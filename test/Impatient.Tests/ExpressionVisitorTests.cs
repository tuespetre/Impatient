using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Generalized;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Impatient.Tests
{
    [TestClass]
    public class ExpressionVisitorTests
    {
        [TestMethod]
        public void BinaryExpression_ConstantLeft()
        {
            var left = Expression.Constant(true);
            var right = Expression.Parameter(typeof(bool));
            var binary = Expression.AndAlso(left, right);
            var visitor = new PartialEvaluatingExpressionVisitor();

            var result = visitor.Visit(binary);

            Assert.AreEqual(binary, result);
        }

        [TestMethod]
        public void BinaryExpression_ConstantRight()
        {
            var left = Expression.Parameter(typeof(bool));
            var right = Expression.Constant(true);
            var binary = Expression.AndAlso(left, right);
            var visitor = new PartialEvaluatingExpressionVisitor();

            var result = visitor.Visit(binary);

            Assert.AreEqual(binary, result);
        }

        [TestMethod]
        public void Hashing()
        {
            var hasher = new HashingExpressionVisitor();

            // No closure

            Expression<Func<string>> expr1 = () => "hello".ToString().Substring(0, 1);
            Expression<Func<string>> expr2 = () => "hello".ToString().Substring(0, 1);

            hasher.Visit(expr1);
            var hashCode1 = hasher.HashCode;
            hasher.Reset();

            hasher.Visit(expr2);
            var hashCode2 = hasher.HashCode;
            hasher.Reset();

            Assert.AreNotEqual(expr1, expr2);
            Assert.AreEqual(hashCode1, hashCode2);

            // Closure

            Expression<Func<string>> CreateExpressionWithClosure(int value)
            {
                Expression<Func<string>> expression = () => "hello".ToString().Substring(value, 1);

                return expression;
            }

            var expr3 = CreateExpressionWithClosure(0);
            var expr4 = CreateExpressionWithClosure(1);

            hasher.Visit(expr3);
            var hashCode3 = hasher.HashCode;
            hasher.Reset();

            hasher.Visit(expr4);
            var hashCode4 = hasher.HashCode;
            hasher.Reset();

            Assert.AreNotEqual(expr3, expr4);
            Assert.AreEqual(hashCode3, hashCode4);

            // Partially evaluated

            var evaluator = new PartialEvaluatingExpressionVisitor();

            Expression<Func<string>> expr5 = () => "hello".ToString().Substring(0, 1);
            var expr6 = CreateExpressionWithClosure(0);

            hasher.Visit(evaluator.Visit(expr5));
            var hashCode5 = hasher.HashCode;
            hasher.Reset();

            hasher.Visit(evaluator.Visit(expr6));
            var hashCode6 = hasher.HashCode;
            hasher.Reset();

            Assert.AreNotEqual(expr5, expr6);
            Assert.AreEqual(hashCode5, hashCode6);
        }

        [TestMethod]
        public void EscapedReferenceCounting()
        {
            Expression<Func<object, IEnumerable<object>, object>> projection = (o1, o2) => new { o1, o2, q = o2.ToArray() };

            var visitor = new EscapedReferenceCountingExpressionVisitor(projection.Parameters[1]);

            visitor.Visit(projection.Body);

            Assert.AreEqual(1, visitor.EscapedReferenceCount);
        }

        [TestMethod]
        public void QueryTrees()
        {
            var set1 = new List<object>().AsQueryable();
            var set2 = new List<object>().AsQueryable();
            var set3 = new List<object>().AsQueryable();

            var query1 = from s1 in set1
                         from s2 in set2
                         select new { s1, s2 } into s
                         select s.s2;

            var visitor1 = new QueryTreeFindingExpressionVisitor();
            visitor1.Visit(query1.Expression);

            Assert.AreEqual(1, visitor1.Trees.Count());
            Assert.AreEqual(1, visitor1.Trees.Values.Sum(tree => tree.Count()));

            var query2 = from s1 in set1.Where(x => true)
                         join s2 in set2 on s1 equals s2
                         select new { s1, s2 };

            var visitor2 = new QueryTreeFindingExpressionVisitor();
            visitor2.Visit(query2.Expression);

            Assert.AreEqual(1, visitor2.Trees.Count());
            Assert.AreEqual(1, visitor2.Trees.Values.Sum(tree => tree.Count()));

            var query3 = from s1 in set1.Where(x => true)
                         from s2 in set2.Where(x => true)
                         select new { s1, s2 };

            var visitor3 = new QueryTreeFindingExpressionVisitor();
            visitor3.Visit(query3.Expression);

            Assert.AreEqual(1, visitor3.Trees.Count());
            Assert.AreEqual(2, visitor3.Trees.Values.Sum(tree => tree.Count()));

            var query4 = from s1 in set1.Where(x => true)
                         from s2 in set2.Where(x => true)
                         from s3 in set3.Where(x => true)
                         select new { s1, s2 };

            var visitor4 = new QueryTreeFindingExpressionVisitor();
            visitor4.Visit(query4.Expression);

            Assert.AreEqual(1, visitor4.Trees.Count());
            Assert.AreEqual(3, visitor4.Trees.Values.Sum(tree => tree.Count()));

            var query5 = from s1 in set1.Where(x => true)
                         from s2 in set2.Where(x => true)
                         select new { s1, s2, s3s = set3.Where(x => true) };

            var visitor5 = new QueryTreeFindingExpressionVisitor();
            visitor5.Visit(query5.Expression);

            Assert.AreEqual(2, visitor5.Trees.Count());
            Assert.AreEqual(3, visitor5.Trees.Values.Sum(tree => tree.Count()));
        }

        [TestMethod]
        public void ClosureDiscovery()
        {
            var set1 = new List<object>().AsQueryable();

            var localVariable = new object();

            var query = from s1 in set1
                        where s1 == localVariable
                        select s1;

            var visitor = new ConstantParameterizingExpressionVisitor();

            visitor.Visit(query.Expression);

            Assert.AreEqual(1, visitor.Mapping.Count);
            Assert.IsTrue(visitor.Mapping.Keys.Single().GetType().GetTypeInfo().GetCustomAttribute<CompilerGeneratedAttribute>() != null);
        }
    }
}
