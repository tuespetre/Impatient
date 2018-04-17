using Impatient.Extensions;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class NullableMemberRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression != null && node.Expression.Type.IsNullableType())
            {
                switch (node.Member.Name)
                {
                    case nameof(Nullable<int>.Value):
                    {
                        return Expression.Convert(
                            node.Expression,
                            Nullable.GetUnderlyingType(
                                node.Expression.Type));
                    }

                    case nameof(Nullable<int>.HasValue):
                    {
                        return Expression.NotEqual(
                            node.Expression,
                            Expression.Constant(
                                null,
                                node.Expression.Type));
                    }
                }
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object != null && node.Object.Type.IsNullableType())
            {
                switch (node.Method.Name)
                {
                    case nameof(Nullable<int>.GetValueOrDefault):
                    {
                        if (node.Arguments.Count == 1)
                        {
                            return Expression.Coalesce(
                                node.Object,
                                node.Arguments[0]);
                        }
                        else
                        {
                            return Expression.Coalesce(
                                node.Object,
                                Expression.Constant(
                                    Activator.CreateInstance(
                                        Nullable.GetUnderlyingType(
                                            node.Object.Type))));
                        }
                    }
                }
            }

            return base.VisitMethodCall(node);
        }
    }
}
