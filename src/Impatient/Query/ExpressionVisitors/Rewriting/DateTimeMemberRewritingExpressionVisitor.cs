using Impatient.Query.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class DateTimeMemberRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (node.Member.DeclaringType == typeof(DateTime))
            {
                switch (node.Member.Name)
                {
                    // Static members

                    case nameof(DateTime.Now):
                    {
                        return new SqlFunctionExpression("GETDATE", typeof(DateTime));
                    }

                    case nameof(DateTime.UtcNow):
                    {
                        return new SqlFunctionExpression("GETUTCDATE", typeof(DateTime));
                    }

                    // Instance members

                    case nameof(DateTime.Date):
                    {
                        return new SqlCastExpression(expression, "date", typeof(DateTime));
                    }

                    case nameof(DateTime.Day):
                    case nameof(DateTime.DayOfYear):
                    case nameof(DateTime.Hour):
                    case nameof(DateTime.Millisecond):
                    case nameof(DateTime.Minute):
                    case nameof(DateTime.Month):
                    case nameof(DateTime.Second):
                    case nameof(DateTime.Year):
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

            if (node.Method.DeclaringType == typeof(DateTime))
            {
                switch (node.Method.Name)
                {
                    case nameof(DateTime.AddDays):
                    case nameof(DateTime.AddHours):
                    case nameof(DateTime.AddMilliseconds):
                    case nameof(DateTime.AddMinutes):
                    case nameof(DateTime.AddMonths):
                    case nameof(DateTime.AddSeconds):
                    case nameof(DateTime.AddYears):
                    {
                        return new SqlFunctionExpression(
                            "DATEADD",
                            typeof(DateTime),
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
