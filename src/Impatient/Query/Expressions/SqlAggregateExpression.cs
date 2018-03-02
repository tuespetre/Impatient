using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlAggregateExpression : SqlExpression
    {
        public SqlAggregateExpression(string functionName, Expression expression, Type type)
            : this(functionName, expression, type, false)
        {
        }

        public SqlAggregateExpression(string functionName, Expression expression, Type type, bool isDistinct)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsDistinct = isDistinct;
        }

        public string FunctionName { get; }

        public bool IsDistinct { get; }

        public Expression Expression { get; }

        public override Type Type { get; }

        public SqlAggregateExpression AsDistinct()
        {
            return new SqlAggregateExpression(FunctionName, Expression, Type, true);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            if (expression != Expression)
            {
                return new SqlAggregateExpression(FunctionName, expression, Type, IsDistinct);
            }

            return this;
        }
    }
}
