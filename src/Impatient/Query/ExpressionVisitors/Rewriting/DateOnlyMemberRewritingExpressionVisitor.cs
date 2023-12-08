using Impatient.Query.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class DateOnlyMemberRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (node.Member.DeclaringType == typeof(DateOnly))
            {
                switch (node.Member.Name)
                {
                    case nameof(DateOnly.Day):
                    case nameof(DateOnly.DayOfYear):
                    case nameof(DateOnly.Month):
                    case nameof(DateOnly.Year):
                    {
                        return new SqlFunctionExpression(
                            "DATEPART",
                            typeof(int),
                            new SqlFragmentExpression(node.Member.Name.ToLower()),
                            expression);
                    }
                }
            }

            return node.Update(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.DeclaringType == typeof(DateOnly))
            {
                switch (node.Method.Name)
                {
                    case nameof(DateOnly.AddDays):
                    case nameof(DateOnly.AddMonths):
                    case nameof(DateOnly.AddYears):
                    {
                        return new SqlFunctionExpression(
                            "DATEADD",
                            typeof(DateOnly),
                            new SqlFragmentExpression(
                                node.Method.Name
                                    .Substring(3, node.Method.Name.Length - 4)
                                    .ToLower()),
                            arguments.Single(),
                            @object);
                    }
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
