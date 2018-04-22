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
        private readonly IQueryProvider queryProvider;
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

        protected Expression Reparameterize(Expression expression)
        {
            return new ConstantParameterizingExpressionVisitor(parameterMapping).Visit(expression);
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

            if (typeof(IQueryable).IsAssignableFrom(visited.Type))
            {
                var evaluated = visited as ConstantExpression;

                if (evaluated == null)
                {
                    var expanded = replacingVisitor.Visit(visited);

                    if (expanded != visited)
                    {
                        var revisited = Visit(expanded);

                        evaluated = revisited as ConstantExpression;

                        if (evaluated == null)
                        {
                            return revisited;
                        }
                    }
                }

                if (evaluated?.Value is IQueryable queryable && queryable.Provider == queryProvider)
                {
                    var query = queryable.Expression;

                    switch (query)
                    {
                        case RelationalQueryExpression _:
                        case ConstantExpression constant when constant.Value == queryable:
                        {
                            return query;
                        }
                    }

                    return Visit(Reparameterize(query));
                }
            }

            return visited;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // This block ensures that IQueryables coming from chained 
            // member accesses rooted at a static member can be inlined.

            if (typeof(IQueryable).IsAssignableFrom(node.Type))
            {
                var memberStack = new Stack<MemberInfo>();

                memberStack.Push(node.Member);

                var expression = node.Expression;

                while (expression is MemberExpression inner)
                {
                    memberStack.Push(inner.Member);

                    expression = inner.Expression;
                }

                if (expression == null || expression is ConstantExpression)
                {
                    try
                    {
                        var value = (expression as ConstantExpression)?.Value;

                        foreach (var member in memberStack)
                        {
                            switch (member)
                            {
                                case PropertyInfo propertyInfo:
                                {
                                    value = propertyInfo.GetValue(value);
                                    break;
                                }

                                case FieldInfo fieldInfo:
                                {
                                    value = fieldInfo.GetValue(value);
                                    break;
                                }
                            }
                        }

                        return Expression.Constant(value);
                    }
                    catch
                    {
                        return base.VisitMember(node);
                    }
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            // This override ensures that materializers with parameterless constructors
            // are not botched by partial evaluation.

            var newExpression = VisitAndConvert(node.NewExpression, nameof(VisitMemberInit));
            var bindings = node.Bindings.Select(VisitMemberBinding);

            return node.Update(newExpression, bindings);
        }

        private class SelectiveExpressionReplacingExpressionVisitor : ExpressionReplacingExpressionVisitor
        {
            public SelectiveExpressionReplacingExpressionVisitor(IDictionary<Expression, Expression> mapping) : base(mapping)
            {
            }

            public SelectiveExpressionReplacingExpressionVisitor(Expression target, Expression replacement) : base(target, replacement)
            {
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
