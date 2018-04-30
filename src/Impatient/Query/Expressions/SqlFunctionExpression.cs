using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlFunctionExpression : SqlExpression
    {
        public SqlFunctionExpression(string functionName, Type type, params Expression[] arguments)
            : this(functionName, type, (IEnumerable<Expression>)arguments)
        {
        }

        public SqlFunctionExpression(string functionName, Type type, IEnumerable<Expression> arguments)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            Arguments = new ReadOnlyCollection<Expression>(arguments?.ToArray() ?? throw new ArgumentNullException(nameof(arguments)));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string FunctionName { get; }

        public ReadOnlyCollection<Expression> Arguments { get; }

        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var arguments = visitor.Visit(Arguments);

            if (!arguments.SequenceEqual(Arguments))
            {
                return new SqlFunctionExpression(FunctionName, Type, arguments);
            }

            return this;
        }

        public override int GetSemanticHashCode() => (IsNullable, FunctionName).GetHashCode();
    }
}
