using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Generating
{
    public class MaterializerGeneratingExpressionVisitor : ExpressionVisitor
    {
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;
        private readonly IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider;

        public MaterializerGeneratingExpressionVisitor(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider)
        {
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
            this.readValueExpressionFactoryProvider = readValueExpressionFactoryProvider;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case SelectExpression selectExpression:
                {
                    var readerParameter
                        = Expression.Parameter(typeof(DbDataReader));

                    var visitor
                        = new MaterializerBuildingExpressionVisitor(
                            translatabilityAnalyzingExpressionVisitor,
                            readValueExpressionFactoryProvider.GetReadValueExpressionFactories(),
                            readerParameter);

                    var body = VisitProjection(selectExpression.Projection, visitor);

                    return Expression.Lambda(body, "MaterializeResult", new[] { readerParameter });
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private Expression VisitProjection(ProjectionExpression projection, MaterializerBuildingExpressionVisitor visitor)
        {
            switch (projection)
            {
                case ServerProjectionExpression serverProjectionExpression:
                {
                    return visitor.Visit(serverProjectionExpression.ResultLambda.Body);
                }

                case ClientProjectionExpression clientProjectionExpression:
                {
                    var server = clientProjectionExpression.ServerProjection;
                    var result = clientProjectionExpression.ResultLambda;

                    return Expression.Invoke(result, VisitProjection(server, visitor));
                }

                case CompositeProjectionExpression compositeProjectionExpression:
                {
                    var outer = VisitProjection(compositeProjectionExpression.OuterProjection, visitor);
                    var inner = VisitProjection(compositeProjectionExpression.InnerProjection, visitor);
                    var result = compositeProjectionExpression.ResultLambda;

                    return Expression.Invoke(result, outer, inner);
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private class MaterializerBuildingExpressionVisitor : ProjectionExpressionVisitor
        {
            private static readonly MethodInfo isDBNullMethodInfo
                = typeof(DbDataReader).GetTypeInfo().GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

            private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor;
            private readonly IEnumerable<IReadValueExpressionFactory> readValueExpressionFactories;
            private readonly ParameterExpression readerParameter;
            private int readerIndex;

            private readonly Dictionary<string, Expression> readValueExpressions = new Dictionary<string, Expression>();

            public MaterializerBuildingExpressionVisitor(
                TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor,
                IEnumerable<IReadValueExpressionFactory> readValueExpressionFactories,
                ParameterExpression readerParameter)
            {
                this.translatabilityVisitor = translatabilityVisitor;
                this.readValueExpressionFactories = readValueExpressionFactories;
                this.readerParameter = readerParameter;
            }

            protected override Expression VisitLeaf(Expression node)
            {
                if (translatabilityVisitor.Visit(node) is TranslatableExpression)
                {
                    var path = string.Join(".", GetNameParts());

                    if (!readValueExpressions.TryGetValue(path, out var readValueExpression))
                    {
                        var readValueExpressionFactory
                            = readValueExpressionFactories
                                .FirstOrDefault(f => f.CanReadExpression(node));

                        if (readValueExpressionFactory == null)
                        {
                            throw new NotSupportedException("Could not find an expression factory to read a value.");
                        }

                        readValueExpression
                            = readValueExpressionFactory
                                .CreateExpression(node, readerParameter, readerIndex++);

                        readValueExpressions[path] = readValueExpression;
                    }

                    return readValueExpression;
                }

                return node;
            }

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case DefaultIfEmptyExpression defaultIfEmptyExpression:
                    {
                        return Expression.Condition(
                            test: Expression.Call(
                                readerParameter,
                                isDBNullMethodInfo,
                                Expression.Constant(readerIndex++)),
                            ifTrue: Expression.Default(defaultIfEmptyExpression.Type),
                            ifFalse: Visit(defaultIfEmptyExpression.Expression));
                    }

                    case PolymorphicExpression polymorphicExpression:
                    {
                        var variables = new List<ParameterExpression>();
                        var expressions = new List<Expression>();

                        var rowValue = polymorphicExpression.Row;
                        var rowVariable = Expression.Variable(rowValue.Type, "row");
                        var rowParameterExpansion = (Expression)rowVariable;

                        if (rowValue is ExtraPropertiesExpression extraPropertiesExpression)
                        {
                            extraPropertiesExpression = VisitAndConvert(extraPropertiesExpression, nameof(Visit));

                            var properties = new List<Expression>();

                            for (var i = 0; i < extraPropertiesExpression.Names.Count; i++)
                            {
                                var propertyName = extraPropertiesExpression.Names[i];
                                var propertyValue = extraPropertiesExpression.Properties[i];
                                var propertyVariable = Expression.Variable(propertyValue.Type, propertyName);

                                variables.Add(propertyVariable);
                                properties.Add(propertyVariable);
                                expressions.Add(Expression.Assign(propertyVariable, propertyValue));
                            }

                            rowValue = Visit(extraPropertiesExpression.Expression);
                            rowVariable = Expression.Variable(rowValue.Type, "row");
                            rowParameterExpansion = extraPropertiesExpression.Update(rowVariable, properties);

                        }
                        else
                        {
                            rowValue = Visit(rowValue);
                        }

                        variables.Add(rowVariable);

                        expressions.Add(Expression.Assign(rowVariable, rowValue));

                        var result = Expression.Default(polymorphicExpression.Type) as Expression;

                        foreach (var descriptor in polymorphicExpression.Descriptors)
                        {
                            var test = descriptor.Test.ExpandParameters(rowParameterExpansion);
                            var materializer = descriptor.Materializer.ExpandParameters(rowParameterExpansion);
                            var expansion = Expression.Convert(materializer, polymorphicExpression.Type);

                            result = Expression.Condition(test, expansion, result, polymorphicExpression.Type);
                        }

                        expressions.Add(result);

                        return Expression.Block(variables, expressions);
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
