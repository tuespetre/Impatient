using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class ContextServiceInjectionExpression : LateBoundProjectionLeafExpression
    {
        public ContextServiceInjectionExpression(Type serviceType)
        {
            Type = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }

        public override Type Type { get; }

        public override Expression Reduce()
        {
            return Call(
                GetType()
                    .GetMethod(nameof(GetServiceForInjection), BindingFlags.Static | BindingFlags.NonPublic)
                    .MakeGenericMethod(Type),
                Convert(ExecutionContextParameter.Instance, typeof(EFCoreDbCommandExecutor)));
        }

        private static TService GetServiceForInjection<TService>(EFCoreDbCommandExecutor executor)
        {
            var context = executor.CurrentDbContext.Context;

            if (context is TService service)
            {
                return service;
            }

            service = executor.CurrentDbContext.Context.GetInfrastructure().GetService<TService>();

            return service;
        }
    }
}
