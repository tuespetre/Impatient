using Impatient.Query.Expressions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class ContextServiceDelegateInjectionExpression : LateBoundProjectionLeafExpression
    {
        private static readonly MethodInfo createDelegateMethodInfo
            = typeof(MethodInfo).GetRuntimeMethod(nameof(MethodInfo.CreateDelegate), new[] { typeof(Type), typeof(object) });

        private readonly Type serviceType;
        private readonly MethodInfo methodInfo;

        public ContextServiceDelegateInjectionExpression(
            Type delegateType,
            Type serviceType,
            MethodInfo methodInfo)
        {
            Type = delegateType ?? throw new ArgumentNullException(nameof(delegateType));
            this.serviceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            this.methodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        }

        public override Type Type { get; }

        public override Expression Reduce()
        {
            return Convert(
                Call(
                    Constant(methodInfo),
                    createDelegateMethodInfo,
                    Constant(Type),
                    new ContextServiceInjectionExpression(serviceType)),
                Type);
        }
    }
}
