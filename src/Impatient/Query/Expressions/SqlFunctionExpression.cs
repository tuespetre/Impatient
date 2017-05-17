using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlFunctionExpression : SqlExpression
    {
        public SqlFunctionExpression(string functionName, Type type, params Expression[] arguments)
        {
            FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
            Arguments = arguments?.ToArray() ?? throw new ArgumentNullException(nameof(arguments));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string FunctionName { get; }

        public IEnumerable<Expression> Arguments { get; }

        public override Type Type { get; }
    }
}
