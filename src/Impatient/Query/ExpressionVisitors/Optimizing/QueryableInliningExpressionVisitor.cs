using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class QueryableInliningExpressionVisitor : PartialEvaluatingExpressionVisitor
    {
        protected readonly IQueryProvider queryProvider;
        private readonly IDictionary<object, ParameterExpression> parameterMapping;
        private readonly ExpressionVisitor replacingVisitor;

        public QueryableInliningExpressionVisitor(
            IQueryProvider queryProvider,
            IDictionary<object, ParameterExpression> parameterMapping)
        {
            this.queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
            this.parameterMapping = parameterMapping ?? throw new ArgumentNullException(nameof(parameterMapping));

            replacingVisitor
                = new SelectiveExpressionReplacingExpressionVisitor(
                    parameterMapping.ToDictionary(
                        kvp => kvp.Value as Expression,
                        kvp => Expression.Constant(kvp.Key) as Expression));
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            // The replacing visitor is run on the node only if it is an IQueryable
            // so that IQueryables coming from a closure can be properly swapped in
            // and taken for their expressions. If the replacing visitor were run on
            // all nodes, actual parameters (think `where customer.Id == <closure>.customerId`)
            // would be replaced, which messes up parameterization and query caching.

            var visited = base.Visit(node);

            if (visited.NodeType == ExpressionType.Constant)
            {
                return VisitConstant((ConstantExpression)visited);
            }

            if (typeof(IQueryable).IsAssignableFrom(visited.Type))
            {
                switch (visited)
                {
                    case MethodCallExpression methodCallExpression
                    when methodCallExpression.Method.IsQueryableOrEnumerableMethod():
                    {
                        return visited;
                    }

                    case RelationalQueryExpression _:
                    {
                        return visited;
                    }
                }

                var expanded = replacingVisitor.Visit(visited);

                if (expanded != visited)
                {
                    var revisited = Visit(expanded);

                    if (revisited.NodeType == ExpressionType.Constant)
                    {
                        return VisitConstant((ConstantExpression)revisited);
                    }

                    return revisited;
                }
            }

            return visited;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable && queryable.Provider == queryProvider)
            {
                return InlineQueryable(queryable);
            }

            return node;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is RelationalQueryExpression)
            {
                return node;
            }

            return base.VisitExtension(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            // This override ensures that materializers with parameterless constructors
            // are not botched by partial evaluation.

            var newExpression = node.NewExpression.Update(Visit(node.NewExpression.Arguments));
            var bindings = node.Bindings.Select(VisitMemberBinding);

            return node.Update(newExpression, bindings);
        }

        protected virtual Expression InlineQueryable(IQueryable queryable)
        {
            var query = queryable.Expression;

            if (query is RelationalQueryExpression || query is ConstantExpression)
            {
                return query;
            }

            return Visit(Reparameterize(query));
        }

        protected Expression Reparameterize(Expression expression)
        {
            return new ConstantParameterizingExpressionVisitor(parameterMapping).Visit(expression);
        }

        private class SelectiveExpressionReplacingExpressionVisitor : ExpressionReplacingExpressionVisitor
        {
            public SelectiveExpressionReplacingExpressionVisitor(IDictionary<Expression, Expression> mapping) : base(mapping)
            {
            }

            public SelectiveExpressionReplacingExpressionVisitor(Expression target, Expression replacement) : base(target, replacement)
            {
            }

            protected override Expression VisitExtension(Expression node)
            {
                if (node is RelationalQueryExpression)
                {
                    return node;
                }

                return base.VisitExtension(node);
            }

            #region no-ops

            protected override Expression VisitBlock(BlockExpression node)
            {
                return node;
            }

            protected override CatchBlock VisitCatchBlock(CatchBlock node)
            {
                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                return base.VisitConstant(node);
            }

            protected override Expression VisitDebugInfo(DebugInfoExpression node)
            {
                return node;
            }

            protected override Expression VisitDefault(DefaultExpression node)
            {
                return node;
            }

            protected override Expression VisitGoto(GotoExpression node)
            {
                return node;
            }

            protected override Expression VisitLabel(LabelExpression node)
            {
                return node;
            }

            protected override Expression VisitLambda<T>(Expression<T> node)
            {
                return node;
            }

            protected override Expression VisitLoop(LoopExpression node)
            {
                return node;
            }

            protected override LabelTarget VisitLabelTarget(LabelTarget node)
            {
                return node;
            }

            protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
            {
                return node;
            }

            protected override Expression VisitSwitch(SwitchExpression node)
            {
                return node;
            }

            protected override SwitchCase VisitSwitchCase(SwitchCase node)
            {
                return node;
            }

            protected override Expression VisitTry(TryExpression node)
            {
                return node;
            }

            #endregion
        }
    }
}
