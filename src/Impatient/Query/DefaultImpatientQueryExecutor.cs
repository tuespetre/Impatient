using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query
{
    public class DefaultImpatientQueryExecutor : IImpatientQueryExecutor
    {
        public DefaultImpatientQueryExecutor(
            DescriptorSet descriptorSet,
            IImpatientQueryCache queryCache,
            IDbCommandExecutorFactory dbCommandExecutorFactory,
            IOptimizingExpressionVisitorProvider optimizingExpressionVisitorProvider,
            IComposingExpressionVisitorProvider composingExpressionVisitorProvider,
            ICompilingExpressionVisitorProvider compilingExpressionVisitorProvider,
            IQueryableInliningExpressionVisitorFactory queryInliningExpressionVisitorFactory)
        {
            DescriptorSet = descriptorSet;
            QueryCache = queryCache;
            DbCommandExecutorFactory = dbCommandExecutorFactory;
            OptimizingExpressionVisitorProvider = optimizingExpressionVisitorProvider;
            ComposingExpressionVisitorProvider = composingExpressionVisitorProvider;
            CompilingExpressionVisitorProvider = compilingExpressionVisitorProvider;
            QueryInliningExpressionVisitorFactory = queryInliningExpressionVisitorFactory;
        }

        public DescriptorSet DescriptorSet { get; }

        public IImpatientQueryCache QueryCache { get; }

        public IDbCommandExecutorFactory DbCommandExecutorFactory { get; }

        public IOptimizingExpressionVisitorProvider OptimizingExpressionVisitorProvider { get; }

        public IComposingExpressionVisitorProvider ComposingExpressionVisitorProvider { get; }

        public ICompilingExpressionVisitorProvider CompilingExpressionVisitorProvider { get; }

        public IQueryableInliningExpressionVisitorFactory QueryInliningExpressionVisitorFactory { get; }

        public object Execute(IQueryProvider provider, Expression expression)
        {
            try
            {
                var processingContext = new QueryProcessingContext(provider, DescriptorSet);
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
                    = QueryInliningExpressionVisitorFactory
                        .Create(processingContext)
                        .Visit(expression);

                // Generate a hash code for the parameterized expression.
                // Because the expression is parameterized, the hash code will be identical
                // for expressions that are structurally equivalent apart from any closure instances.

                var hashingVisitor = new HashingExpressionVisitor();
                hashingVisitor.Visit(expression);

                // Some parameters may have been eliminated during query inlining,
                // so we need to make sure all of our parameters' types are included
                // in the hash code to avoid errors related to mismatched closure types.
                foreach (var parameter in parameterMapping.Values)
                {
                    hashingVisitor.Combine(parameter.Type.GetHashCode());
                }

                if (!QueryCache.TryGetValue(hashingVisitor.HashCode, out var compiled))
                {
                    // Apply all optimizing visitors before each composing visitor and then apply all
                    // optimizing visitors one last time.

                    var composingExpressionVisitors 
                        = ComposingExpressionVisitorProvider
                            .CreateExpressionVisitors(processingContext)
                            .ToArray();

                    var optimizingExpressionVisitors 
                        = OptimizingExpressionVisitorProvider
                            .CreateExpressionVisitors(processingContext)
                            .ToArray();

                    expression
                        = composingExpressionVisitors
                            .SelectMany(c => optimizingExpressionVisitors.Append(c))
                            .Concat(optimizingExpressionVisitors)
                            .Aggregate(expression, (e, v) => v.Visit(e));

                    // Transform the expression by rewriting all composed query expressions into 
                    // executable expressions that make database calls and perform result materialization.

                    expression
                        = expression
                            .VisitWith(CompilingExpressionVisitorProvider
                                .CreateExpressionVisitors(processingContext));

                    // Compile the resulting expression into an executable delegate.

                    var parameters = new ParameterExpression[parameterMapping.Count + 1];

                    parameters[0] = processingContext.ExecutionContextParameter;

                    parameterMapping.Values.CopyTo(parameters, 1);

                    compiled = Expression.Lambda(expression, parameters).Compile();

                    // Cache the compiled delegate.

                    QueryCache.Add(hashingVisitor.HashCode, compiled);
                }

                // Invoke the compiled delegate.

                var arguments = new object[parameterMapping.Count + 1];

                arguments[0] = DbCommandExecutorFactory.Create();

                parameterMapping.Keys.CopyTo(arguments, 1);

                return compiled.DynamicInvoke(arguments);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        }
    }
}
