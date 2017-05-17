using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Impatient.Query.ExpressionVisitors
{
    public class ClosureParameterizingExpressionVisitor : ExpressionVisitor
    {
        private int discoveredClosures = -1;

        public IDictionary<object, ParameterExpression> Mapping { get; }
            = new Dictionary<object, ParameterExpression>();

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var type = node.Value?.GetType();

            if (type != null)
            {
                if (Mapping.TryGetValue(node.Value, out var parameter))
                {
                    return parameter;
                }

                var typeInfo = type.GetTypeInfo();

                if (typeInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                    && typeInfo.Attributes.HasFlag(TypeAttributes.NestedPrivate))
                {
                    discoveredClosures++;

                    parameter = Expression.Parameter(type, $"closure{discoveredClosures}");

                    Mapping.Add(node.Value, parameter);

                    return parameter;
                }
            }

            return base.VisitConstant(node);
        }
    }
}
