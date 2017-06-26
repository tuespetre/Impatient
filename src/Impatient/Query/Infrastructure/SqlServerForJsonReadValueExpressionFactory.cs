using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.Linq.Enumerable;

namespace Impatient.Query.Infrastructure
{
    public class SqlServerForJsonReadValueExpressionFactory : IReadValueExpressionFactory
    {
        #region reflection
        
        private static readonly MethodInfo dbDataReaderGetFieldValueMethodInfo
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

        private static readonly MethodInfo dbDataReaderIsDBNullMethodInfo
            = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

        private static readonly MethodInfo enumerableEmptyMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((object o) => Empty<object>());

        private static readonly MethodInfo enumerableToArrayMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.ToArray());

        private static readonly MethodInfo queryableAsQueryableMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable());

        private static readonly ConstructorInfo jsonTextReaderConstructorInfo
            = typeof(JsonTextReader).GetConstructor(new[] { typeof(StringReader) });

        private static readonly MethodInfo jsonTextReaderReadMethodInfo
            = typeof(JsonTextReader).GetRuntimeMethod(nameof(JsonTextReader.Read), new Type[0]);

        private static readonly PropertyInfo jsonTextReaderTokenTypePropertyInfo
            = typeof(JsonTextReader).GetRuntimeProperty(nameof(JsonTextReader.TokenType));

        private static readonly PropertyInfo jsonTextReaderValuePropertyInfo
            = typeof(JsonTextReader).GetRuntimeProperty(nameof(JsonTextReader.Value));

        private static readonly PropertyInfo jsonTextReaderDateParseHandlingPropertyInfo
            = typeof(JsonTextReader).GetRuntimeProperty(nameof(JsonTextReader.DateParseHandling));

        private static readonly ConstructorInfo stringReaderConstructorInfo
            = typeof(StringReader).GetConstructor(new[] { typeof(string) });

        private static readonly MethodInfo disposableDisposeMethodInfo
            = typeof(IDisposable).GetTypeInfo().GetDeclaredMethod(nameof(IDisposable.Dispose));

        private static readonly MethodInfo convertFromBase64StringMethodInfo
            = typeof(Convert).GetRuntimeMethod(nameof(Convert.FromBase64String), new[] { typeof(string) });

        private static readonly MethodInfo guidParseMethodInfo
            = typeof(Guid).GetRuntimeMethod(nameof(Guid.Parse), new[] { typeof(string) });

        private static readonly MethodInfo dateTimeParseMethodInfo
            = typeof(DateTime).GetRuntimeMethod(nameof(DateTime.Parse), new[] { typeof(string) });

        private static readonly MethodInfo dateTimeOffsetParseMethodInfo
            = typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.Parse), new[] { typeof(string) });

        private static readonly MethodInfo timeSpanParseMethodInfo
            = typeof(TimeSpan).GetRuntimeMethod(nameof(TimeSpan.Parse), new[] { typeof(string) });

        #endregion

        public bool CanReadExpression(Expression expression)
        {
            return !expression.Type.IsScalarType();
        }

        public Expression CreateExpression(Expression source, Expression reader, int index)
        {
            var jsonTextReaderVariable = Expression.Variable(typeof(JsonTextReader), "jsonTextReader");
            var resultVariable = Expression.Variable(source.Type, "result");

            var deserializerExpression
                = Expression.Block(
                    variables: new[] 
                    {
                        jsonTextReaderVariable
                    },
                    expressions: new Expression[]
                    {
                        Expression.Assign(
                            jsonTextReaderVariable,
                            Expression.New(
                                jsonTextReaderConstructorInfo,
                                Expression.New(
                                    stringReaderConstructorInfo,
                                    Expression.Call(
                                        reader,
                                        dbDataReaderGetFieldValueMethodInfo.MakeGenericMethod(typeof(string)),
                                        Expression.Constant(index))))),
                        Expression.TryFinally(
                            body: Expression.Block(
                                Expression.Assign(
                                    Expression.Property(jsonTextReaderVariable, jsonTextReaderDateParseHandlingPropertyInfo),
                                    Expression.Constant(DateParseHandling.None)),
                                new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReaderVariable)
                                    .Visit(source)),
                            @finally: Expression.Call(
                                Expression.Convert(jsonTextReaderVariable, typeof(IDisposable)),
                                disposableDisposeMethodInfo)),
                    });

            return Expression.Condition(
                Expression.Call(reader, dbDataReaderIsDBNullMethodInfo, Expression.Constant(index)),
                CreateDefaultValueExpression(source.Type),
                deserializerExpression);
        }

        private static Expression CreateDefaultValueExpression(Type type)
        {
            if (type.IsSequenceType())
            {
                if (type.IsArray)
                {
                    return Expression.NewArrayInit(type.GetElementType());
                }
                else if (type.GetTypeInfo().DeclaredConstructors.Any(c => c.GetParameters().Length == 0))
                {
                    return Expression.New(type);
                }
                else
                {
                    var sequenceType = type.GetSequenceType();

                    var defaultValue = Expression.Call(enumerableEmptyMethodInfo.MakeGenericMethod(sequenceType));

                    if (type.IsGenericType(typeof(IQueryable<>)))
                    {
                        return Expression.Call(queryableAsQueryableMethodInfo.MakeGenericMethod(sequenceType), defaultValue);
                    }

                    return defaultValue;
                }
            }
            else
            {
                return Expression.Default(type);
            }
        }

        private static Expression ExtractProjectionExpression(Expression node)
        {
            switch (node)
            {
                case SqlAliasExpression sqlAliasExpression:
                {
                    return ExtractProjectionExpression(sqlAliasExpression.Expression);
                }

                case SqlColumnExpression sqlColumnExpression
                when sqlColumnExpression.Table is SubqueryTableExpression subqueryTableExpression:
                {
                    return ExtractProjectionExpression(subqueryTableExpression.Subquery.Projection.Flatten().Body);
                }

                case RelationalQueryExpression relationalQueryExpression:
                {
                    return ExtractProjectionExpression(relationalQueryExpression.SelectExpression.Projection.Flatten().Body);
                }

                default:
                {
                    return node;
                }
            }
        }

        private static Expression CreateScalarTypeReadExpression(Type type, Expression jsonTextReader)
        {
            var readerValue = Expression.Property(jsonTextReader, jsonTextReaderValuePropertyInfo);

            Expression DefaultOrValue(Expression otherwise)
            {
                return Expression.Condition(
                    Expression.Equal(readerValue, Expression.Constant(null, typeof(object))), 
                    Expression.Default(type), 
                    Expression.Convert(otherwise, type));
            }

            Expression DefaultOrMethodCall(MethodInfo methodInfo)
            {
                return DefaultOrValue(Expression.Call(methodInfo, Expression.Convert(readerValue, typeof(string))));
            }

            if (type == typeof(string))
            {
                return Expression.Convert(readerValue, typeof(string));
            }
            else if (type == typeof(byte[]))
            {
                return DefaultOrMethodCall(convertFromBase64StringMethodInfo);
            }
            else if (type == typeof(byte) || type == typeof(byte?))
            {
                return DefaultOrValue(Expression.Convert(readerValue, typeof(long)));
            }
            else if (type == typeof(short) || type == typeof(short?))
            {
                return DefaultOrValue(Expression.Convert(readerValue, typeof(long)));
            }
            else if (type == typeof(int) || type == typeof(int?))
            {
                return DefaultOrValue(Expression.Convert(readerValue, typeof(long)));
            }
            else if (type == typeof(long) || type == typeof(long?))
            {
                return DefaultOrValue(readerValue);
            }
            else if (type == typeof(bool) || type == typeof(bool?))
            {
                return DefaultOrValue(readerValue);
            }
            else if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return DefaultOrValue(Expression.Convert(readerValue, typeof(double)));
            }
            else if (type == typeof(float) || type == typeof(float?))
            {
                return DefaultOrValue(Expression.Convert(readerValue, typeof(double)));
            }
            else if (type == typeof(double) || type == typeof(double?))
            {
                return DefaultOrValue(readerValue);
            }
            else if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return DefaultOrMethodCall(guidParseMethodInfo);
            }
            else if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return DefaultOrMethodCall(dateTimeParseMethodInfo);
            }
            else if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                return DefaultOrMethodCall(dateTimeOffsetParseMethodInfo);
            }
            else if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                return DefaultOrMethodCall(timeSpanParseMethodInfo);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private class ComplexTypeMaterializerBuildingExpressionVisitor : ProjectionExpressionVisitor
        {
            private readonly ParameterExpression jsonTextReader;
            private readonly Expression readExpression;
            private int depth = 0;

            public ComplexTypeMaterializerBuildingExpressionVisitor(ParameterExpression jsonTextReader)
            {
                this.jsonTextReader = jsonTextReader;

                readExpression = Expression.Call(jsonTextReader, jsonTextReaderReadMethodInfo);
            }

            protected override Expression VisitLeaf(Expression node)
            {
                if (node.Type.IsScalarType())
                {
                    var temporaryVariableExpression = Expression.Variable(node.Type);

                    return Expression.Block(
                        variables: new[] 
                        {
                            temporaryVariableExpression
                        },
                        expressions: new[]
                        {
                            readExpression, // Value
                            Expression.Assign(
                                temporaryVariableExpression, 
                                CreateScalarTypeReadExpression(node.Type, jsonTextReader)),
                            readExpression, // PropertyName | EndObject
                            temporaryVariableExpression,
                        });
                }
                else if (node.Type.IsSequenceType())
                {
                    var sequenceType = node.Type.GetSequenceType();

                    Expression materializerExpression;

                    if (sequenceType.IsScalarType())
                    {
                        materializerExpression = CreateScalarTypeReadExpression(sequenceType, jsonTextReader);
                    }
                    else
                    {
                        materializerExpression
                            = new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader)
                                .Visit(ExtractProjectionExpression(node));
                    }

                    if (sequenceType.IsScalarType() || sequenceType.IsSequenceType())
                    {
                        var temporaryVariableExpression = Expression.Variable(sequenceType);

                        materializerExpression
                            = Expression.Block(
                                variables: new[] 
                                {
                                    temporaryVariableExpression
                                },
                                expressions: new[]
                                {
                                    readExpression, // PropertyName
                                    readExpression, // Value
                                    Expression.Assign(temporaryVariableExpression, materializerExpression),
                                    readExpression, // EndObject
                                    temporaryVariableExpression,
                                });
                    }

                    var listVariable = Expression.Variable(typeof(List<>).MakeGenericType(sequenceType));
                    var listAddMethod = listVariable.Type.GetRuntimeMethod(nameof(List<object>.Add), new[] { sequenceType });
                    var breakLabelTarget = Expression.Label();

                    var loopBody
                        = Expression.IfThenElse(
                            test: Expression.Equal(
                                Expression.Property(jsonTextReader, jsonTextReaderTokenTypePropertyInfo),
                                Expression.Constant(JsonToken.EndArray)),
                            ifTrue: Expression.Break(breakLabelTarget),
                            ifFalse: Expression.Block(
                                expressions: new[]
                                {
                                    Expression.Call(listVariable, listAddMethod, materializerExpression),
                                    readExpression, // StartObject | EndArray
                                }));

                    return Expression.Block(
                        variables: new[] 
                        {
                            listVariable
                        },
                        expressions: new[]
                        {
                            Expression.Assign(listVariable, Expression.New(listVariable.Type)),
                            readExpression, // StartArray
                            readExpression, // StartObject
                            Expression.Loop(loopBody, breakLabelTarget),
                            readExpression, // EndObject | PropertyName
                            node.Type.IsArray
                                ? Expression.Call(
                                    enumerableToArrayMethodInfo.MakeGenericMethod(sequenceType), 
                                    listVariable)
                                : node.Type.IsGenericType(typeof(IQueryable<>))
                                    ? Expression.Call(
                                        queryableAsQueryableMethodInfo.MakeGenericMethod(sequenceType), 
                                        listVariable)
                                    : listVariable as Expression,
                        });
                }
                else
                {
                    return new ComplexTypeMaterializerBuildingExpressionVisitor(jsonTextReader)
                        .Visit(ExtractProjectionExpression(node));
                }
            }

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case NewExpression newExpression when IsNotLeaf(newExpression):
                    case MemberInitExpression memberInitExpression when IsNotLeaf(memberInitExpression):
                    {
                        depth++;

                        var visited = base.Visit(node);

                        depth--;

                        var temporaryVariableExpression = Expression.Variable(node.Type);

                        if (depth > 0)
                        {
                            return Expression.Block(
                                variables: new[] 
                                {
                                    temporaryVariableExpression
                                }, 
                                expressions: new[]
                                {
                                    readExpression, // StartObject
                                    readExpression, // PropertyName
                                    Expression.Assign(temporaryVariableExpression, visited),
                                    readExpression, // EndObject
                                    temporaryVariableExpression,
                                });
                        }
                        else
                        {
                            return Expression.Block(
                                variables: new[]
                                {
                                    temporaryVariableExpression
                                },
                                expressions: new[]
                                {
                                    readExpression, // PropertyName
                                    Expression.Assign(temporaryVariableExpression, visited),
                                    temporaryVariableExpression,
                                });
                        }
                    }

                    default:
                    {
                        return base.Visit(node);
                    }
                }
            }
        }
    }
}
