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
        private static IEnumerable<object[]> QueryableMethods
        {
            get
            {
                foreach (var methodInfo in typeof(Queryable).GetTypeInfo().DeclaredMethods)
                {
                    if (methodInfo.Name == nameof(Queryable.AsQueryable) || !methodInfo.IsPublic || !methodInfo.IsStatic)
                    {
                        continue;
                    }
                    else if (methodInfo.IsGenericMethodDefinition)
                    {
                        var args = Enumerable.Repeat(typeof(object), methodInfo.GetGenericArguments().Length).ToArray();

                        yield return new object[] 
                        {
                            methodInfo.MakeGenericMethod(args)
                        };
                    }
                }
            }
        }

        [TestMethod]
        [DynamicData(nameof(QueryableMethods))]
        public void ImpatientExtensions_MatchQueryableMethod(MethodInfo methodInfo)
        {
            ImpatientExtensions.MatchQueryableMethod(methodInfo);
        }

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
        }

        [TestMethod]
        public void ClosureDiscovery()
        {
            var set1 = new List<object>().AsQueryable();

            var localVariable = new object();

            var query = from s1 in set1
                        where s1 == localVariable
                        select s1;

            var mapping = new Dictionary<object, ParameterExpression>();
            var visitor = new ConstantParameterizingExpressionVisitor(mapping);

            visitor.Visit(query.Expression);

            Assert.AreEqual(1, mapping.Count);
            Assert.IsTrue(mapping.Keys.Single().GetType().GetTypeInfo().GetCustomAttribute<CompilerGeneratedAttribute>() != null);
        }
    }
}
