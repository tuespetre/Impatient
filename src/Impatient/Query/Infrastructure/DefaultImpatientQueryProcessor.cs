using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    public class DefaultImpatientQueryProcessor : IImpatientQueryProcessor
    {
        private readonly IImpatientQueryCache queryCache;
        private readonly IQueryProcessingContextFactory queryProcessingContextFactory;
        private readonly IQueryableInliningExpressionVisitorFactory queryableInliningExpressionVisitorFactory;
        private readonly IOptimizingExpressionVisitorProvider optimizingExpressionVisitorProvider;
        private readonly IComposingExpressionVisitorProvider composingExpressionVisitorProvider;
        private readonly ICompilingExpressionVisitorProvider compilingExpressionVisitorProvider;
        private readonly IDbCommandExecutorFactory dbCommandExecutorFactory;

        public DefaultImpatientQueryProcessor(
            IImpatientQueryCache queryCache,
            IQueryProcessingContextFactory queryProcessingContextFactory,
            IQueryableInliningExpressionVisitorFactory queryableInliningExpressionVisitorFactory,
            IOptimizingExpressionVisitorProvider optimizingExpressionVisitorProvider,
            IComposingExpressionVisitorProvider composingExpressionVisitorProvider,
            ICompilingExpressionVisitorProvider compilingExpressionVisitorProvider,
            IDbCommandExecutorFactory dbCommandExecutorFactory)
        {
            this.queryCache = queryCache ?? throw new ArgumentNullException(nameof(queryCache));
            this.queryProcessingContextFactory = queryProcessingContextFactory ?? throw new ArgumentNullException(nameof(queryProcessingContextFactory));
            this.queryableInliningExpressionVisitorFactory = queryableInliningExpressionVisitorFactory ?? throw new ArgumentNullException(nameof(queryableInliningExpressionVisitorFactory));
            this.optimizingExpressionVisitorProvider = optimizingExpressionVisitorProvider ?? throw new ArgumentNullException(nameof(optimizingExpressionVisitorProvider));
            this.composingExpressionVisitorProvider = composingExpressionVisitorProvider ?? throw new ArgumentNullException(nameof(composingExpressionVisitorProvider));
            this.compilingExpressionVisitorProvider = compilingExpressionVisitorProvider ?? throw new ArgumentNullException(nameof(compilingExpressionVisitorProvider));
            this.dbCommandExecutorFactory = dbCommandExecutorFactory ?? throw new ArgumentNullException(nameof(dbCommandExecutorFactory));
        }

        public object Execute(IQueryProvider provider, Expression expression)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            try
            {
                var context = queryProcessingContextFactory.CreateQueryProcessingContext(provider);

                var parameterized = ParameterizeQuery(expression, context);

                var inlined = InlineQuery(parameterized, context);

                var hash = ComputeHash(inlined, context);

                var compiled
                    = queryCache.GetOrAdd(
                        hash,
                        arg => arg.self.CompileDelegate(
                            arg.self.ApplyVisitors(
                                arg.inlined, 
                                arg.context),
                            arg.context),
                        (self: this, inlined, context));

                var arguments = new object[context.ParameterMapping.Count + 1];

                arguments[0] = dbCommandExecutorFactory.Create();

                context.ParameterMapping.Keys.CopyTo(arguments, 1);

                return ((Func<object[], object>)compiled)(arguments);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        }

        private Expression ParameterizeQuery(Expression expression, QueryProcessingContext context)
        {
            // Parameterize the expression by substituting any ConstantExpression
            // that is not a literal constant (such as a closure instance) with a ParameterExpression.

           return new ConstantParameterizingExpressionVisitor(context.ParameterMapping).Visit(expression);
        }

        private Expression InlineQuery(Expression expression, QueryProcessingContext context)
        {
            // Partially evaluate the expression. In addition to reducing evaluable nodes such 
            // as `new DateTime(2000, 01, 01)` down to ConstantExpressions, this visitor also expands 
            // IQueryable-producing expressions such as those found within calls to SelectMany
            // so that the resulting IQueryable's expression tree will be integrated into the 
            // current expression tree.

            return queryableInliningExpressionVisitorFactory.Create(context).Visit(expression);
        }

        private int ComputeHash(Expression expression, QueryProcessingContext context)
        {
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
                foreach (var parameter in context.ParameterMapping.Values)
                {
                    hash = (hash * 16777619) ^ comparer.GetHashCode(parameter);
                }
            }

            return hash;
        }

        private Expression ApplyVisitors(Expression expression, QueryProcessingContext context)
        {
            var composingExpressionVisitors
                = composingExpressionVisitorProvider
                    .CreateExpressionVisitors(context)
                    .ToArray();

            var optimizingExpressionVisitors
                = optimizingExpressionVisitorProvider
                    .CreateExpressionVisitors(context)
                    .ToArray();

            var compilingExpressionVisitors
                = compilingExpressionVisitorProvider
                    .CreateExpressionVisitors(context)
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

            return expression;
        }

        private Func<object[], object> CompileDelegate(Expression expression, QueryProcessingContext context)
        {
            var parameters = new ParameterExpression[context.ParameterMapping.Count + 1];

            parameters[0] = ExecutionContextParameters.DbCommandExecutor;

            context.ParameterMapping.Values.CopyTo(parameters, 1);

            var parameterArray = Expression.Parameter(typeof(object[]));

            // Wrap the actual lambda in a static invocation.
            // This is faster than just compiling it and calling DynamicInvoke.

            return Expression
                .Lambda<Func<object[], object>>(
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
        }
    }
}
