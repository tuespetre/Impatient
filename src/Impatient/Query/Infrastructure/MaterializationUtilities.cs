using Impatient.Extensions;
using System;
using System.Data.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Impatient.Query.Infrastructure
{
    public static class MaterializationUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue ReadNonNullable<TValue>(DbDataReader reader, int index)
        {
            return (TValue)reader.GetValue(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue ReadNullable<TValue>(DbDataReader reader, int index)
        {
            var value = reader.GetValue(index);

            if (DBNull.Value.Equals(value))
            {
                return default;
            }
            else
            {
                return (TValue)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue ReadNonNullableEnum<TValue>(DbDataReader reader, int index)
        {
            return (TValue)Enum.ToObject(typeof(TValue), reader.GetValue(index));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TNullable ReadNullableEnum<TNullable, TValue>(DbDataReader reader, int index)
        {
            var value = reader.GetValue(index);

            if (DBNull.Value.Equals(value))
            {
                return default;
            }
            else
            {
                return (TNullable)Enum.ToObject(typeof(TValue), value);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut ReadNonNullableConversion<TIn, TOut>(DbDataReader reader, int index, Func<TIn, TOut> conversion)
        {
            var value = reader.GetValue(index);

            Debug.Assert(!DBNull.Value.Equals(value));

            return conversion((TIn)value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut ReadNullableConversion<TIn, TOut>(DbDataReader reader, int index, Func<TIn, TOut> conversion)
        {
            var value = reader.GetValue(index);

            if (DBNull.Value.Equals(value))
            {
                return default;
            }
            else
            {
                return conversion((TIn)value);
            }
        }

        private static readonly MethodInfo invokeMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition<object, object>(o => Invoke<object>(default));

        /// <summary>
        /// When compiling with DEBUG, this method will wrap the given <paramref name="expression"/>
        /// in a <see cref="LambdaExpression"/> with the given <paramref name="identifier"/> as its
        /// name and return a static call to <see cref="Invoke{TResult}(Func{TResult})"/> so that
        /// if an exception is thrown from the <paramref name="expression"/>, the stack trace will
        /// be able to help identify the <paramref name="expression"/> as the culprit.
        /// </summary>
        /// <param name="expression">The expression to wrap.</param>
        /// <param name="identifier">The identifier to name the <see cref="LambdaExpression"/>.</param>
        /// <returns>
        /// A call to <see cref="Invoke{TResult}(Func{TResult})"/> if DEBUG; 
        /// otherwise, the given <paramref name="expression"/>.
        /// </returns>
#if DEBUG
        public static Expression Invoke(Expression expression, string identifier)
        {
            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.DeclaringType == typeof(MaterializationUtilities))
            {
                // Shortcut. No need to wrap calls to ReadNullable, etc.
                return expression;
            }

            return Expression.Call(
                invokeMethodInfo.MakeGenericMethod(expression.Type),
                Expression.Lambda(expression, identifier, Array.Empty<ParameterExpression>()));
        }
#else
        public static Expression Invoke(Expression expression, string identifier)
        {
            return expression;
        }
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TResult Invoke<TResult>(Func<TResult> lambda) => lambda();
    }
}
