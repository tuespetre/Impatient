using Impatient.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// A <see cref="ExpressionVisitor"/> that discovers all non-literal constant
    /// expressions and replaces them with a <see cref="ParameterExpression"/>.
    /// </summary>
    public class ConstantParameterizingExpressionVisitor : ExpressionVisitor
    {
        private readonly IDictionary<object, ParameterExpression> mapping;

        public ConstantParameterizingExpressionVisitor(IDictionary<object, ParameterExpression> mapping)
        {
            this.mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.UnwrapNullableType().IsConstantLiteralType() 
                || node.Type.IsEnum()
                || node.Value is null 
                || node.Value is IQueryable)
            {
                return node;
            }

            if (!mapping.TryGetValue(node.Value, out var parameter))
            {
                parameter = Expression.Parameter(node.Type, $"scope{mapping.Count}");

                mapping.Add(node.Value, parameter);
            }

            return parameter;
        }
    }
}
