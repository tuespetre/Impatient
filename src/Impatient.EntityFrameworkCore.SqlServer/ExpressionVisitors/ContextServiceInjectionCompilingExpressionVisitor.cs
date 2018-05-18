using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class ContextServiceInjectionCompilingExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression executionContextParameter;

        public ContextServiceInjectionCompilingExpressionVisitor(ParameterExpression executionContextParameter)
        {
            this.executionContextParameter = executionContextParameter ?? throw new ArgumentNullException(nameof(executionContextParameter));
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (node is ContextServiceInjectionExpression contextServiceInjection)
            {
                return Expression.Call(
                    GetType()
                        .GetMethod(nameof(GetServiceForInjection), BindingFlags.Static | BindingFlags.NonPublic)
                        .MakeGenericMethod(node.Type),
                    Expression.Convert(executionContextParameter, typeof(EFCoreDbCommandExecutor)));
            }

            return base.VisitExtension(node);
        }

        private static TService GetServiceForInjection<TService>(EFCoreDbCommandExecutor executor)
        {
            var context = executor.CurrentDbContext.Context;

            if (context is TService service)
            {
                return service;
            }

            var infrastructure = executor.CurrentDbContext.Context.GetInfrastructure();

            service = infrastructure.GetRequiredService<TService>();

            return service;
        }
    }
}
