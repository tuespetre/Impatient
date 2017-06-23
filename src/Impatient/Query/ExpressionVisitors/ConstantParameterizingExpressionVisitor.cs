using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class ConstantParameterizingExpressionVisitor : ExpressionVisitor
    {
        private int discoveredConstants = 0;

        public IDictionary<object, ParameterExpression> Mapping { get; }
            = new Dictionary<object, ParameterExpression>();

        private static readonly Type[] constantLiteralTypes = new Type[]
        {
            typeof(bool),
            typeof(decimal),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(int),
            typeof(uint),
            typeof(ulong),
            typeof(char),
            typeof(string),
        };

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var type = node.Value?.GetType();

            if (type != null
                && !constantLiteralTypes.Contains(type)
                && !(node.Value is IQueryable))
            {
                if (Mapping.TryGetValue(node.Value, out var parameter))
                {
                    return parameter;
                }

                parameter = Expression.Parameter(type, $"scope{discoveredConstants}");

                discoveredConstants++;

                Mapping.Add(node.Value, parameter);

                return parameter;
            }

            return base.VisitConstant(node);
        }
    }
}
