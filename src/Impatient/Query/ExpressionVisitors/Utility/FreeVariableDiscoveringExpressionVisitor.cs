using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class FreeVariableDiscoveringExpressionVisitor : ExpressionVisitor
    {
        private readonly HashSet<ParameterExpression> foundVariables
            = new HashSet<ParameterExpression>();

        private readonly HashSet<ParameterExpression> declaredVariables
            = new HashSet<ParameterExpression>()
            {
                ExecutionContextParameter.Instance
            };

        public IEnumerable<ParameterExpression> DiscoveredVariables => foundVariables.Except(declaredVariables);

        protected override Expression VisitParameter(ParameterExpression node)
        {
            foundVariables.Add(node);

            return base.VisitParameter(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            for (var i = 0; i < node.Parameters.Count; i++)
            {
                declaredVariables.Add(node.Parameters[i]);
            }

            return base.VisitLambda(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            for (var i = 0; i < node.Variables.Count; i++)
            {
                declaredVariables.Add(node.Variables[i]);
            }

            return base.VisitBlock(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            declaredVariables.Add(node.Variable);

            return base.VisitCatchBlock(node);
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            for (var i = 0; i < node.Variables.Count; i++)
            {
                declaredVariables.Add(node.Variables[i]);
            }

            return base.VisitRuntimeVariables(node);
        }
    }
}
