using Impatient.Extensions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Impatient.Query.Infrastructure
{
    public class DefaultDbCommandExpressionBuilder : IDbCommandExpressionBuilder
    {
        private int parameterIndex = 0;
        private int indentationLevel = 0;
        private bool containsParameterList = false;
        private Stack<StringBuilder> captureStack = new Stack<StringBuilder>();
        private StringBuilder archiveStringBuilder = new StringBuilder();
        private StringBuilder workingStringBuilder = new StringBuilder();

        private readonly ParameterExpression dbCommandVariable = Expression.Parameter(typeof(DbCommand), "command");
        private readonly ParameterExpression stringBuilderVariable = Expression.Parameter(typeof(StringBuilder), "builder");
        private readonly ParameterExpression dbParameterVariable = Expression.Parameter(typeof(DbParameter), "parameter");
        private readonly List<Expression> blockExpressions = new List<Expression>();
        private readonly List<Expression> dbParameterExpressions = new List<Expression>();
        private readonly Dictionary<int, int> parameterCache = new Dictionary<int, int>();

        private static readonly MethodInfo stringBuilderAppendMethodInfo
            = typeof(StringBuilder).GetRuntimeMethod(nameof(StringBuilder.Append), new[] { typeof(string) });

        private static readonly MethodInfo stringBuilderToStringMethodInfo
            = typeof(StringBuilder).GetRuntimeMethod(nameof(StringBuilder.ToString), new Type[0]);

        private static readonly MethodInfo stringConcatObjectMethodInfo
            = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) });

        private static readonly PropertyInfo dbCommandCommandTextPropertyInfo
            = typeof(DbCommand).GetTypeInfo().GetDeclaredProperty(nameof(DbCommand.CommandText));

        private static readonly PropertyInfo dbCommandParametersPropertyInfo
            = typeof(DbCommand).GetTypeInfo().GetDeclaredProperty(nameof(DbCommand.Parameters));

        private static readonly MethodInfo dbCommandCreateParameterMethodInfo
            = typeof(DbCommand).GetTypeInfo().GetDeclaredMethod(nameof(DbCommand.CreateParameter));

        private static readonly PropertyInfo dbParameterParameterNamePropertyInfo
            = typeof(DbParameter).GetTypeInfo().GetDeclaredProperty(nameof(DbParameter.ParameterName));

        private static readonly PropertyInfo dbParameterValuePropertyInfo
            = typeof(DbParameter).GetTypeInfo().GetDeclaredProperty(nameof(DbParameter.Value));

        private static readonly MethodInfo dbParameterCollectionAddMethodInfo
            = typeof(DbParameterCollection).GetTypeInfo().GetDeclaredMethod(nameof(DbParameterCollection.Add));

        private static readonly MethodInfo enumerableGetEnumeratorMethodInfo
            = typeof(IEnumerable).GetTypeInfo().GetDeclaredMethod(nameof(IEnumerable.GetEnumerator));

        private static readonly MethodInfo enumeratorMoveNextMethodInfo
            = typeof(IEnumerator).GetTypeInfo().GetDeclaredMethod(nameof(IEnumerator.MoveNext));

        private static readonly PropertyInfo enumeratorCurrentPropertyInfo
            = typeof(IEnumerator).GetTypeInfo().GetDeclaredProperty(nameof(IEnumerator.Current));

        private static readonly MethodInfo disposableDisposeMethodInfo
            = typeof(IDisposable).GetTypeInfo().GetDeclaredMethod(nameof(IDisposable.Dispose));

        public void Append(string sql)
        {
            workingStringBuilder.Append(sql);
        }

        public void AppendLine()
        {
            workingStringBuilder.AppendLine();
            workingStringBuilder.Append(string.Empty.PadLeft(indentationLevel * 4));
        }

        public void IncreaseIndent()
        {
            indentationLevel++;
        }

        public void DecreaseIndent()
        {
            indentationLevel--;
        }

        public void StartCapture()
        {
            EmitSql();

            captureStack.Push(archiveStringBuilder);

            archiveStringBuilder = new StringBuilder();
        }

        public string StopCapture()
        {
            EmitSql();

            var result = archiveStringBuilder.ToString();

            archiveStringBuilder = captureStack.Pop();

            return result;
        }

        private static Expression SetParameterValue(Expression parameter, Expression value)
        {
            var valueToSet = (Expression)Expression.Convert(value, typeof(object));

            if (value.Type.IsNullableType() || !value.Type.GetTypeInfo().IsValueType)
            {
                valueToSet
                    = Expression.Coalesce(
                        valueToSet,
                        Expression.Convert(
                            Expression.Constant(DBNull.Value),
                            typeof(object)));
            }

            return Expression.Assign(
                Expression.MakeMemberAccess(
                    parameter,
                    dbParameterValuePropertyInfo),
                valueToSet);
        }

        public void AddParameter(Expression node, Func<string, string> formatter)
        {
            var hasher = new HashingExpressionVisitor();

            hasher.Visit(node);

            if (parameterCache.TryGetValue(hasher.HashCode, out var cachedIndex))
            {
                Append(formatter($"p{cachedIndex}"));

                return;
            }

            var parameterName = formatter($"p{parameterIndex}");
            
            parameterCache.Add(hasher.HashCode, parameterIndex);
            parameterIndex++;

            Append(parameterName);

            EmitSql();

            var expressions = new Expression[]
            {
                Expression.Assign(
                    dbParameterVariable,
                    Expression.Call(
                        dbCommandVariable,
                        dbCommandCreateParameterMethodInfo)),
                Expression.Assign(
                    Expression.MakeMemberAccess(
                        dbParameterVariable,
                        dbParameterParameterNamePropertyInfo),
                    Expression.Constant(parameterName)),
                SetParameterValue(
                    dbParameterVariable,
                    node),
                Expression.Call(
                    Expression.MakeMemberAccess(
                        dbCommandVariable,
                        dbCommandParametersPropertyInfo),
                    dbParameterCollectionAddMethodInfo,
                    dbParameterVariable)
            };

            blockExpressions.AddRange(expressions);
            dbParameterExpressions.AddRange(expressions);
        }

        public void AddDynamicParameters(string fragment, Expression expression, Func<string, string> formatter)
        {
            EmitSql();

            blockExpressions.Add(Expression.Call(
                GetType().GetMethod(nameof(RuntimeAddDynamicParameters), BindingFlags.NonPublic | BindingFlags.Static),
                dbCommandVariable,
                stringBuilderVariable,
                Expression.Constant(fragment),
                expression,
                Expression.Constant($"p{parameterIndex}"),
                Expression.Constant(formatter)));

            parameterIndex++;
            containsParameterList = true;
        }

        public LambdaExpression Build()
        {
            EmitSql();

            var blockVariables = new List<ParameterExpression> { stringBuilderVariable, dbParameterVariable };
            var blockExpressions = this.blockExpressions;

            if (containsParameterList)
            {
                blockExpressions.Insert(0,
                    Expression.Assign(
                        stringBuilderVariable,
                        Expression.New(typeof(StringBuilder))));

                blockExpressions.Add(
                    Expression.Assign(
                        Expression.MakeMemberAccess(
                            dbCommandVariable,
                            dbCommandCommandTextPropertyInfo),
                        Expression.Call(
                            stringBuilderVariable,
                            stringBuilderToStringMethodInfo)));
            }
            else
            {
                blockVariables.Remove(stringBuilderVariable);

                blockExpressions.Clear();

                blockExpressions.Add(
                    Expression.Assign(
                        Expression.MakeMemberAccess(
                            dbCommandVariable,
                            dbCommandCommandTextPropertyInfo),
                        Expression.Constant(archiveStringBuilder.ToString())));

                blockExpressions.AddRange(dbParameterExpressions);

                if (dbParameterExpressions.Count == 0)
                {
                    blockVariables.Remove(dbParameterVariable);
                }
            }

            return Expression.Lambda(
                typeof(Action<DbCommand>),
                Expression.Block(blockVariables, blockExpressions),
                "PrepareDbCommand",
                new[] { dbCommandVariable });
        }

        private void EmitSql()
        {
            if (workingStringBuilder.Length > 0)
            {
                var currentString = workingStringBuilder.ToString();

                if (captureStack.Count == 0)
                {
                    blockExpressions.Add(
                        Expression.Call(
                            stringBuilderVariable,
                            stringBuilderAppendMethodInfo,
                            Expression.Constant(currentString)));
                }

                archiveStringBuilder.Append(currentString);

                workingStringBuilder.Clear();
            }
        }

        private static void RuntimeAddDynamicParameters(
            DbCommand dbCommand,
            StringBuilder stringBuilder,
            string fragment,
            IEnumerable values,
            string name,
            Func<string, string> formatter)
        {
            DbParameter parameter;
            string formattedName;
            IEnumerator enumerator = default;
            var index = 0;
            var addedParameter = false;
            var foundNull = false;

            try
            {
                enumerator = values.GetEnumerator();

                var insertPoint = stringBuilder.Length;

                stringBuilder.Append(fragment);

                if (enumerator.MoveNext())
                {
                    if (enumerator.Current == null)
                    {
                        foundNull = true;
                    }
                    else
                    {
                        if (!addedParameter)
                        {
                            stringBuilder.Append(" IN (");
                        }

                        addedParameter = true;
                        formattedName = formatter($"{name}_{index++}");
                        stringBuilder.Append(formattedName);
                        parameter = dbCommand.CreateParameter();
                        parameter.ParameterName = formattedName;
                        parameter.Value = enumerator.Current;
                        dbCommand.Parameters.Add(parameter);
                    }

                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current == null)
                        {
                            foundNull = true;
                        }
                        else
                        {
                            formattedName = formatter($"{name}_{index++}");

                            if (!addedParameter)
                            {
                                stringBuilder.Append($" IN ({formattedName}");
                            }
                            else
                            {
                                stringBuilder.Append($", {formattedName}");
                            }

                            addedParameter = true;
                            parameter = dbCommand.CreateParameter();
                            parameter.ParameterName = formattedName;
                            parameter.Value = enumerator.Current;
                            dbCommand.Parameters.Add(parameter);
                        }
                    }
                }

                if (!addedParameter)
                {
                    if (!foundNull)
                    {
                        stringBuilder.Append(" IN (NULL)");
                    }
                    else
                    {
                        stringBuilder.Append(" IS NULL");
                    }
                }
                else
                {
                    stringBuilder.Append(")");

                    if (foundNull)
                    {
                        stringBuilder.Insert(insertPoint, "(");
                        stringBuilder.Append(" OR ");
                        stringBuilder.Append(fragment);
                        stringBuilder.Append(" IS NULL");
                        stringBuilder.Append(")");
                    }
                }
            }
            finally
            {
                if (enumerator is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
