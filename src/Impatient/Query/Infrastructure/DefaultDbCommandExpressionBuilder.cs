using Impatient.Extensions;
using Impatient.Query.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
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
        private readonly List<Expression> blockExpressions = new List<Expression>();
        private readonly List<Expression> dbParameterExpressions = new List<Expression>();
        private readonly Dictionary<int, int> parameterCache = new Dictionary<int, int>();

        private static readonly MethodInfo stringBuilderAppendMethodInfo
            = typeof(StringBuilder).GetRuntimeMethod(nameof(StringBuilder.Append), new[] { typeof(string) });

        private static readonly MethodInfo stringBuilderToStringMethodInfo
            = typeof(StringBuilder).GetRuntimeMethod(nameof(StringBuilder.ToString), new Type[0]);

        private static readonly PropertyInfo dbCommandCommandTextPropertyInfo
            = typeof(DbCommand).GetTypeInfo().GetDeclaredProperty(nameof(DbCommand.CommandText));

        private readonly ITypeMappingProvider typeMappingProvider;
        private readonly IQueryFormattingProvider queryFormattingProvider;

        public DefaultDbCommandExpressionBuilder(
            ITypeMappingProvider typeMappingProvider,
            IQueryFormattingProvider queryFormattingProvider)
        {
            this.typeMappingProvider = typeMappingProvider ?? throw new ArgumentNullException(nameof(typeMappingProvider));
            this.queryFormattingProvider = queryFormattingProvider ?? throw new ArgumentNullException(nameof(queryFormattingProvider));
        }

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

            return valueToSet;
        }

        public void AddParameter(SqlParameterExpression node)
        {
            var hash = ExpressionEqualityComparer.Instance.GetHashCode(node);

            if (parameterCache.TryGetValue(hash, out var cachedIndex))
            {
                Append(queryFormattingProvider.FormatParameterName($"p{cachedIndex}"));

                return;
            }

            var parameterName = queryFormattingProvider.FormatParameterName($"p{parameterIndex}");

            parameterCache[hash] = parameterIndex;
            parameterIndex++;

            Append(parameterName);

            EmitSql();

            var typeMapping = node.TypeMapping ?? typeMappingProvider.FindMapping(node.Type);

            if (typeMapping is null)
            {
                throw new InvalidOperationException($"Could not find a type mapping for a parameter value: {node}");
            }

            Expression value = node;

            if (typeMapping.SourceConversion is LambdaExpression conversion)
            {
                var expansion = value;

                var inputType = conversion.Parameters.Single().Type;

                if (expansion.Type != inputType)
                {
                    if (expansion.Type == typeMapping.TargetConversion.Parameters.Single().Type)
                    {
                        expansion = typeMapping.TargetConversion.ExpandParameters(expansion);
                    }
                    else
                    {
                        expansion = Expression.Convert(expansion, inputType);
                    }
                }

                value = conversion.ExpandParameters(expansion);
            }

            if (node.Type.IsNullableType() || !node.Type.GetTypeInfo().IsValueType)
            {
                value
                    = Expression.Condition(
                        Expression.Equal(node, Expression.Constant(null)),
                        Expression.Constant(null),
                        Expression.Convert(value, typeof(object)));
            }

            var expression
                = Expression.Call(
                    GetType().GetMethod(nameof(RuntimeAddParameter), BindingFlags.NonPublic | BindingFlags.Static),
                    dbCommandVariable,
                    Expression.Constant(parameterName),
                    Expression.Constant(typeMapping.DbType, typeof(DbType?)),
                    Expression.Lambda(typeof(Func<object>), Expression.Convert(value, typeof(object))));

            blockExpressions.Add(expression);
            dbParameterExpressions.Add(expression);
        }

        public void AddDynamicParameters(string fragment, Expression expression)
        {
            Debug.Assert(expression != null && typeof(IEnumerable).IsAssignableFrom(expression.Type));

            EmitSql();

            var sequenceType = expression.Type.GetSequenceType();

            var typeMapping = typeMappingProvider.FindMapping(sequenceType);

            if (typeMapping is null)
            {
                throw new InvalidOperationException($"Could not find a type mapping for a parameter value: {expression}");
            }

            Expression conversion = Expression.Default(typeof(Func<object, object>));

            if (typeMapping.SourceConversion is LambdaExpression lambdaConversion)
            {
                var oldParameter = lambdaConversion.Parameters.Single();
                var newParameter = Expression.Parameter(typeof(object), oldParameter.Name);

                conversion
                    = Expression.Lambda(
                        Expression.Convert(
                            lambdaConversion.Body.Replace(oldParameter, Expression.Convert(newParameter, oldParameter.Type)),
                            typeof(object)),
                        newParameter);
            };

            blockExpressions.Add(Expression.Call(
                GetType().GetMethod(nameof(RuntimeAddDynamicParameters), BindingFlags.NonPublic | BindingFlags.Static),
                dbCommandVariable,
                stringBuilderVariable,
                Expression.Constant(fragment),
                Expression.Lambda(typeof(Func<IEnumerable>), expression),
                Expression.Constant($"p{parameterIndex}"),
                Expression.Constant(queryFormattingProvider),
                Expression.Constant(typeMapping.DbType, typeof(DbType?)),
                conversion));

            parameterIndex++;
            containsParameterList = true;
        }

        public LambdaExpression Build()
        {
            EmitSql();

            var blockVariables = new List<ParameterExpression> { stringBuilderVariable };
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

        private static void RuntimeAddParameter(
            DbCommand dbCommand,
            string name,
            DbType? dbType,
            Func<object> value)
        {
            try
            {
                var parameter = dbCommand.CreateParameter();

                parameter.ParameterName = name;
                parameter.Value = value() ?? DBNull.Value;

                if (dbType.HasValue)
                {
                    parameter.DbType = dbType.Value;
                }

                dbCommand.Parameters.Add(parameter);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Error while applying parameters to the query", exception);
            }
        }

        private static void RuntimeAddDynamicParameters(
            DbCommand dbCommand,
            StringBuilder stringBuilder,
            string fragment,
            Func<IEnumerable> values,
            string name,
            IQueryFormattingProvider queryFormattingProvider,
            DbType? dbType,
            Func<object, object> conversion)
        {
            DbParameter parameter;
            string formattedName;
            IEnumerator enumerator = default;
            var index = 0;
            var addedParameter = false;
            var foundNull = false;

            try
            {
                enumerator = values().GetEnumerator();

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
                        formattedName = queryFormattingProvider.FormatParameterName($"{name}_{index++}");
                        stringBuilder.Append(formattedName);
                        parameter = dbCommand.CreateParameter();
                        parameter.ParameterName = formattedName;

                        if (conversion != null)
                        {
                            parameter.Value = conversion(enumerator.Current);
                        }
                        else
                        {
                            parameter.Value = enumerator.Current;
                        }

                        if (dbType.HasValue)
                        {
                            parameter.DbType = dbType.Value;
                        }

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
                            formattedName = queryFormattingProvider.FormatParameterName($"{name}_{index++}");

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

                            if (conversion != null)
                            {
                                parameter.Value = conversion(enumerator.Current);
                            }
                            else
                            {
                                parameter.Value = enumerator.Current;
                            }

                            if (dbType.HasValue)
                            {
                                parameter.DbType = dbType.Value;
                            }

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
            catch (Exception exception)
            {
                throw new InvalidOperationException("Error while applying parameters to the query", exception);
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
