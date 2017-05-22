using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Impatient.Query.ExpressionVisitors
{
    public class ConstantParameterizingExpressionVisitor : ExpressionVisitor
    {
        private bool foundConstantThis;
        private int discoveredClosures = -1;
        private int other = -1;

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

                var typeInfo = type.GetTypeInfo();

                if (typeInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                    && typeInfo.Attributes.HasFlag(TypeAttributes.NestedPrivate))
                {
                    discoveredClosures++;

                    parameter = Expression.Parameter(type, $"closure{discoveredClosures}");

                    Mapping.Add(node.Value, parameter);

                    return parameter;
                }
                else if (!foundConstantThis)
                {
                    foundConstantThis = true;

                    parameter = Expression.Parameter(type, "this");

                    Mapping.Add(node.Value, parameter);

                    return parameter;
                }
                else
                {
                    other++;

                    parameter = Expression.Parameter(type, $"other{other}");

                    Mapping.Add(node.Value, parameter);

                    return parameter;
                }
            }

            return base.VisitConstant(node);
        }
    }
}
