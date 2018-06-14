using Impatient.Extensions;
using Impatient.Query.Infrastructure;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class EnumHasFlagRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo enumHasFlagMethodInfo
            = ReflectionExtensions.GetMethodInfo(() => DayOfWeek.Friday.HasFlag(DayOfWeek.Friday));

        private readonly ITypeMappingProvider typeMappingProvider;

        public EnumHasFlagRewritingExpressionVisitor(ITypeMappingProvider typeMappingProvider)
        {
            this.typeMappingProvider = typeMappingProvider ?? throw new ArgumentNullException(nameof(typeMappingProvider));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (enumHasFlagMethodInfo.Equals(node.Method))
            {
                var mapping = typeMappingProvider.FindMapping(node.Object.Type);

                if (mapping != null && mapping.SourceType.IsNumericType())
                {
                    var underlyingType = Enum.GetUnderlyingType(node.Object.Type);

                    var flag 
                        = Expression.Convert(
                            Expression.Convert(
                                node.Arguments[0], 
                                node.Object.Type),
                            underlyingType);

                    return Expression.Equal(
                        Expression.And(
                            Expression.Convert(
                                node.Object, 
                                underlyingType),
                            flag), 
                        flag);
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
