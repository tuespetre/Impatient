using Impatient.Query.ExpressionVisitors;
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
