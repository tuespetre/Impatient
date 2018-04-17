using Impatient.Extensions;
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
        private StringBuilder archiveStringBuilder = new StringBuilder();
        private StringBuilder workingStringBuilder = new StringBuilder();

        private readonly ParameterExpression dbCommandVariable = Expression.Parameter(typeof(DbCommand), "command");
        private readonly ParameterExpression stringBuilderVariable = Expression.Parameter(typeof(StringBuilder), "builder");
        private readonly ParameterExpression dbParameterVariable = Expression.Parameter(typeof(DbParameter), "parameter");
        private readonly List<Expression> blockExpressions = new List<Expression>();
        private readonly List<Expression> dbParameterExpressions = new List<Expression>();

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

        private static Expression SetParameterValue(Expression parameter, Expression value)
        {
            var valueToSet = (Expression)Expression.Convert(value, typeof(object));

            if (value.Type.IsNullableType())
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
            var parameterName = formatter($"p{parameterIndex}");

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

            parameterIndex++;
        }

        public void AddParameterList(Expression node, Func<string, string> formatter)
        {
            var enumeratorVariable = Expression.Parameter(typeof(IEnumerator), "enumerator");
            var enumeratedVariable = Expression.Parameter(typeof(bool), "enumerated");
            var indexVariable = Expression.Parameter(typeof(int), "index");
            var parameterPrefixVariable = Expression.Parameter(typeof(string), "parameterPrefix");
            var parameterNameVariable = Expression.Parameter(typeof(string), "parameterName");
            var breakLabel = Expression.Label();

            var parameterListBlock
                = Expression.Block(
                    new[]
                    {
                        enumeratorVariable,
                        enumeratedVariable,
                        indexVariable,
                        parameterPrefixVariable,
                        parameterNameVariable
                    },
                    Expression.TryFinally(
                        body: Expression.Block(
                            Expression.Assign(
                                enumeratorVariable,
                                Expression.Call(
                                    node,
                                    enumerableGetEnumeratorMethodInfo)),
                            Expression.Assign(
                                parameterPrefixVariable,
                                Expression.Constant(formatter($"p{parameterIndex}_"))),
                            Expression.Loop(
                                @break: breakLabel,
                                body: Expression.Block(
                                    Expression.Assign(
                                        parameterNameVariable,
                                        Expression.Call(
                                            stringConcatObjectMethodInfo,
                                            parameterPrefixVariable,
                                            Expression.Convert(indexVariable, typeof(object)))),
                                    Expression.IfThenElse(
                                        Expression.Call(enumeratorVariable, enumeratorMoveNextMethodInfo),
                                        Expression.Assign(enumeratedVariable, Expression.Constant(true)),
                                        Expression.Break(breakLabel)),
                                    Expression.IfThen(
                                        Expression.GreaterThan(
                                            indexVariable, 
                                            Expression.Constant(0)),
                                        Expression.Call(
                                            stringBuilderVariable,
                                            stringBuilderAppendMethodInfo,
                                            Expression.Constant(", "))),
                                    Expression.Assign(
                                        indexVariable, 
                                        Expression.Increment(indexVariable)),
                                    Expression.Call(
                                        stringBuilderVariable,
                                        stringBuilderAppendMethodInfo,
                                        parameterNameVariable),
                                    Expression.Assign(
                                        dbParameterVariable,
                                        Expression.Call(
                                            dbCommandVariable,
                                            dbCommandCreateParameterMethodInfo)),
                                    Expression.Assign(
                                        Expression.MakeMemberAccess(
                                            dbParameterVariable,
                                            dbParameterParameterNamePropertyInfo),
                                        parameterNameVariable),
                                    SetParameterValue(
                                        dbParameterVariable,
                                        Expression.MakeMemberAccess(
                                            enumeratorVariable,
                                            enumeratorCurrentPropertyInfo)),
                                    Expression.Call(
                                        Expression.MakeMemberAccess(
                                            dbCommandVariable,
                                            dbCommandParametersPropertyInfo),
                                        dbParameterCollectionAddMethodInfo,
                                        dbParameterVariable)))),
                            @finally: Expression.Block(
                                Expression.IfThen(
                                    Expression.IsFalse(enumeratedVariable),
                                    Expression.Call(
                                        stringBuilderVariable,
                                        stringBuilderAppendMethodInfo,
                                        Expression.Constant("SELECT 1 WHERE 1 = 0"))),
                                Expression.IfThen(
                                    Expression.TypeIs(
                                        enumeratorVariable,
                                        typeof(IDisposable)),
                                    Expression.Call(
                                        Expression.Convert(
                                            enumeratorVariable,
                                            typeof(IDisposable)),
                                        disposableDisposeMethodInfo)))));

            EmitSql();
            blockExpressions.Add(parameterListBlock);
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

                blockExpressions.Add(
                    Expression.Call(
                        stringBuilderVariable,
                        stringBuilderAppendMethodInfo,
                        Expression.Constant(currentString)));

                archiveStringBuilder.Append(currentString);

                workingStringBuilder.Clear();
            }
        }
    }
}
