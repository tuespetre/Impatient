using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class ListContainsToEnumerableContainsRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo enumerableContainsMethodInfo
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            var listType = node.Method.DeclaringType.FindGenericType(typeof(List<>));

            if (listType != null && node.Method.Equals(listType.GetMethod(nameof(List<object>.Contains))))
            {
                return Expression.Call(
                    enumerableContainsMethodInfo.MakeGenericMethod(listType.GetGenericArguments().Single()),
                    arguments.Prepend(@object));
            }

            return node.Update(@object, arguments);
        }
    }
}
