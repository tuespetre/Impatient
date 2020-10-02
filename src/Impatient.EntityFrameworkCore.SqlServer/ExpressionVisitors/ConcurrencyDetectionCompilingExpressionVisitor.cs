using Impatient.Extensions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class ConcurrencyDetectionCompilingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            // Discard annotations
            node = node.UnwrapInnerExpression();

            var isReallySequence
                = node.Type.IsSequenceType()
                    && !(node is MethodCallExpression call
                        && call.Method.IsGenericMethod
                        && call.Method.GetGenericArguments()[0] == node.Type);

            if (isReallySequence)
            {
                var inner = node;
                var conversion = default(MethodInfo);

                if (node is MethodCallExpression methodCallExpression
                    && methodCallExpression.Type.DeclaringType == typeof(Enumerable))
                {
                    inner = methodCallExpression.Arguments.Single();
                    conversion = methodCallExpression.Method;
                }

                var result
                    = Expression.Call(
                        GetType()
                            .GetMethod(nameof(DetectEnumerableConcurrency), BindingFlags.NonPublic | BindingFlags.Static)
                            .MakeGenericMethod(node.Type.GetSequenceType()),
                        inner,
                        Expression.Convert(
                            ExecutionContextParameters.DbCommandExecutor,
                            typeof(EFCoreDbCommandExecutor)));

                if (conversion is not null)
                {
                    return Expression.Call(conversion, result);
                }

                return result;
            }
            else
            {
                return Expression.Call(
                    GetType()
                        .GetMethod(nameof(DetectElementConcurrency), BindingFlags.NonPublic | BindingFlags.Static)
                        .MakeGenericMethod(node.Type),
                    Expression.Lambda(node),
                    Expression.Convert(
                        ExecutionContextParameters.DbCommandExecutor,
                        typeof(EFCoreDbCommandExecutor)));
            }
        }

        private static IEnumerable<TSource> DetectEnumerableConcurrency<TSource>(
            IEnumerable<TSource> source,
            EFCoreDbCommandExecutor executor)
        {
            var detector = executor.CurrentDbContext.Context.GetService<IConcurrencyDetector>();

            using var enumerator = source.GetEnumerator();

            KeepOnKeepingOn:

            var keepKeepingOnKeepingOn = false;

            using (detector.EnterCriticalSection())
            {
                keepKeepingOnKeepingOn = enumerator.MoveNext();
            }

            if (keepKeepingOnKeepingOn)
            {
                yield return enumerator.Current;

                goto KeepOnKeepingOn;
            }
        }

        private static async IAsyncEnumerable<TSource> DetectAsyncEnumerableConcurrency<TSource>(
            IAsyncEnumerable<TSource> source,
            EFCoreDbCommandExecutor executor)
        {
            var detector = executor.CurrentDbContext.Context.GetService<IConcurrencyDetector>();

            await using var enumerator = source.GetAsyncEnumerator();

            KeepOnKeepingOn:

            var keepKeepingOnKeepingOn = false;

            using (detector.EnterCriticalSection())
            {
                keepKeepingOnKeepingOn = await enumerator.MoveNextAsync();
            }

            if (keepKeepingOnKeepingOn)
            {
                yield return enumerator.Current;

                goto KeepOnKeepingOn;
            }
        }

        private static TSource DetectElementConcurrency<TSource>(
            Func<TSource> source,
            EFCoreDbCommandExecutor executor)
        {
            var detector = executor.CurrentDbContext.Context.GetService<IConcurrencyDetector>();

            using (detector.EnterCriticalSection())
            {
                return source();
            }
        }
    }
}
