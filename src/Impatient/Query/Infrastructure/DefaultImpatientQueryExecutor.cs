using Impatient.Metadata;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    public class DefaultImpatientQueryExecutor : IImpatientQueryExecutor
    {
        public DefaultImpatientQueryExecutor(
            DescriptorSet descriptorSet,
            IImpatientQueryCache queryCache,
            IQueryProcessingContextFactory queryProcessingContextFactory,
            IQueryableInliningExpressionVisitorFactory queryableInliningExpressionVisitorFactory,
            IOptimizingExpressionVisitorProvider optimizingExpressionVisitorProvider,
            IComposingExpressionVisitorProvider composingExpressionVisitorProvider,
            ICompilingExpressionVisitorProvider compilingExpressionVisitorProvider,
            IDbCommandExecutorFactory dbCommandExecutorFactory)
        {
            DescriptorSet = descriptorSet;
            QueryCache = queryCache;
            QueryProcessingContextFactory = queryProcessingContextFactory;
            QueryableInliningExpressionVisitorFactory = queryableInliningExpressionVisitorFactory;
            OptimizingExpressionVisitorProvider = optimizingExpressionVisitorProvider;
            ComposingExpressionVisitorProvider = composingExpressionVisitorProvider;
            CompilingExpressionVisitorProvider = compilingExpressionVisitorProvider;
            DbCommandExecutorFactory = dbCommandExecutorFactory;
        }

        public DescriptorSet DescriptorSet { get; }

        public IImpatientQueryCache QueryCache { get; }

        public IQueryProcessingContextFactory QueryProcessingContextFactory { get; }

        public IQueryableInliningExpressionVisitorFactory QueryableInliningExpressionVisitorFactory { get; }

        public IOptimizingExpressionVisitorProvider OptimizingExpressionVisitorProvider { get; }

        public IComposingExpressionVisitorProvider ComposingExpressionVisitorProvider { get; }

        public ICompilingExpressionVisitorProvider CompilingExpressionVisitorProvider { get; }

        public IDbCommandExecutorFactory DbCommandExecutorFactory { get; }

        public object Execute(IQueryProvider provider, Expression expression)
        {
            try
            {
                var processingContext 
                    = QueryProcessingContextFactory
                        .CreateQueryProcessingContext(provider, DescriptorSet);

                var parameterMapping = processingContext.ParameterMapping;

                // Parameterize the expression by substituting any ConstantExpression
                // that is not a literal constant (such as a closure instance) with a ParameterExpression.

                var constantParameterizingVisitor = new ConstantParameterizingExpressionVisitor(parameterMapping);
                expression = constantParameterizingVisitor.Visit(expression);

                // Partially evaluate the expression. In addition to reducing evaluable nodes such 
                // as `new DateTime(2000, 01, 01)` down to ConstantExpressions, this visitor also expands 
                // IQueryable-producing expressions such as those found within calls to SelectMany
                // so that the resulting IQueryable's expression tree will be integrated into the 
                // current expression tree.

                expression
                    = QueryableInliningExpressionVisitorFactory
                        .Create(processingContext)
                        .Visit(expression);

                // Generate a hash code for the parameterized expression.
                // Because the expression is parameterized, the hash code will be identical
                // for expressions that are structurally equivalent apart from any closure instances.

                var comparer = ExpressionEqualityComparer.Instance;

                var hash = comparer.GetHashCode(expression);

                // Some parameters may have been eliminated during query inlining,
                // so we need to make sure all of our parameters' types are included
                // in the hash code to avoid errors related to mismatched closure types.
                unchecked
                {
                    foreach (var parameter in parameterMapping.Values)
                    {
                        hash = (hash * 16777619) ^ comparer.GetHashCode(parameter);
                    }
                }

                // TODO: have querycache accept the expression instead of the hash.
                // Then implement equals for the comparer.

                if (!QueryCache.TryGetValue(hash, out var compiled))
                {
                    var composingExpressionVisitors 
                        = ComposingExpressionVisitorProvider
                            .CreateExpressionVisitors(processingContext)
                            .ToArray();

                    var optimizingExpressionVisitors 
                        = OptimizingExpressionVisitorProvider
                            .CreateExpressionVisitors(processingContext)
                            .ToArray();

                    var compilingExpressionVisitors
                        = CompilingExpressionVisitorProvider
                            .CreateExpressionVisitors(processingContext)
                            .ToArray();

                    // Apply all optimizing visitors before each composing visitor and then apply all
                    // optimizing visitors one last time.

                    foreach (var optimizingVisitor in optimizingExpressionVisitors)
                    {
                        expression = optimizingVisitor.Visit(expression);
                    }

                    foreach (var composingVisitor in composingExpressionVisitors)
                    {
                        expression = composingVisitor.Visit(expression);

                        foreach (var optimizingVisitor in optimizingExpressionVisitors)
                        {
                            expression = optimizingVisitor.Visit(expression);
                        }
                    }

                    // Transform the expression by rewriting all composed query expressions into 
                    // executable expressions that make database calls and perform result materialization.

                    foreach (var compilingVisitor in compilingExpressionVisitors)
                    {
                        expression = compilingVisitor.Visit(expression);
                    }

                    // Compile the resulting expression into an executable delegate.

                    var parameters = new ParameterExpression[parameterMapping.Count + 1];

                    parameters[0] = processingContext.ExecutionContextParameter;

                    parameterMapping.Values.CopyTo(parameters, 1);

                    var parameterArray = Expression.Parameter(typeof(object[]));

                    compiled 
                        = Expression
                            .Lambda(
                                Expression.Convert(
                                    Expression.Invoke(
                                        Expression.Lambda(expression, parameters),
                                        parameters.Select((p, i) =>
                                            Expression.Convert(
                                            Expression.ArrayIndex(
                                                parameterArray,
                                                Expression.Constant(i)),
                                                p.Type))), 
                                    typeof(object)), 
                                parameterArray)
                            .Compile();

                    // Cache the compiled delegate.

                    QueryCache.Add(hash, compiled);
                }

                // Invoke the compiled delegate.

                var arguments = new object[parameterMapping.Count + 1];

                arguments[0] = DbCommandExecutorFactory.Create();

                parameterMapping.Keys.CopyTo(arguments, 1);

                return ((Func<object[], object>)compiled)(arguments);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        }
    }
}
