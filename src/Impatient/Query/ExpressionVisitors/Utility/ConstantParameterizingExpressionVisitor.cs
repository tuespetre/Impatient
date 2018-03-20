using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// A <see cref="ExpressionVisitor"/> that discovers all non-literal constant
    /// expressions and replaces them with a <see cref="ParameterExpression"/>.
    /// </summary>
    public class ConstantParameterizingExpressionVisitor : ExpressionVisitor
    {
        public IDictionary<object, ParameterExpression> Mapping { get; } = new Dictionary<object, ParameterExpression>();

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsConstantLiteralType() || node.Value is null || node.Value is IQueryable)
            {
                return node;
            }

            if (!Mapping.TryGetValue(node.Value, out var parameter))
            {
                parameter = Expression.Parameter(node.Type, $"scope{Mapping.Count}");

                Mapping.Add(node.Value, parameter);
            }

            return parameter;
        }
    }
}
