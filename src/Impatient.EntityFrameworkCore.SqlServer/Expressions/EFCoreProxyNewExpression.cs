using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class EFCoreProxyNewExpression : ExtendedNewExpression
    {
        private readonly Expression factoryInstance;
        private readonly MethodInfo factoryMethod;
        private readonly Expression[] factoryArguments;

        public EFCoreProxyNewExpression(
            Expression factoryInstance,
            MethodInfo factoryMethod,
            IEnumerable<Expression> factoryArguments,
            ConstructorInfo constructor,
            IEnumerable<Expression> arguments, 
            IEnumerable<MemberInfo> readableMembers, 
            IEnumerable<MemberInfo> writableMembers) 
            : base(constructor, arguments, readableMembers, writableMembers)
        {
            this.factoryInstance = factoryInstance;
            this.factoryMethod = factoryMethod ?? throw new ArgumentNullException(nameof(factoryMethod));
            this.factoryArguments = factoryArguments?.ToArray() ?? throw new ArgumentNullException(nameof(factoryArguments));
        }

        public override Expression Reduce()
        {
            var arguments = new Expression[factoryArguments.Length + 1];

            factoryArguments.CopyTo(arguments, 0);

            arguments[factoryArguments.Length]
                = NewArrayInit(
                    typeof(object),
                    Arguments.Select(a => Convert(a, typeof(object))));

            if (factoryInstance is null)
            {
                return Convert(Call(factoryMethod, arguments), Type);
            }
            else
            {
                return Convert(Call(factoryInstance, factoryMethod, arguments), Type);
            }
        }

        public override ExtendedNewExpression Update(IEnumerable<Expression> arguments)
        {
            if (!arguments.SequenceEqual(Arguments))
            {
                return new EFCoreProxyNewExpression(
                    factoryInstance,
                    factoryMethod,
                    factoryArguments,
                    Constructor,
                    arguments,
                    ReadableMembers,
                    WritableMembers);
            }

            return this;
        }
    }
}
