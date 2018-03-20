using Impatient.Metadata;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class IncludeComposingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo queryableSelectMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((IQueryable<object> o) => o.Select(x => x));

        private static readonly MethodInfo enumerableSelectMethodInfo
            = ImpatientExtensions.GetGenericMethodDefinition((IEnumerable<object> o) => o.Select(x => x));

        private readonly DescriptorSet descriptorSet;

        public IncludeComposingExpressionVisitor(DescriptorSet descriptorSet)
        {
            this.descriptorSet = descriptorSet;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case MethodCallExpression call
                when IsIncludeMethod(call.Method):
                {
                    var path = new Stack<MemberInfo>();
                    var paths = new List<Stack<MemberInfo>> { path };
                    var inner = default(Expression);

                    do
                    {
                        // TODO: Implement string-based Include/ThenInclude (yuck)
                        path.Push(GetMemberInfo(call.Arguments[1].UnwrapLambda()));

                        if (!IsThenIncludeMethod(call.Method))
                        {
                            path = new Stack<MemberInfo>();
                            paths.Add(path);
                        }

                        inner = call.Arguments[0];
                        call = inner as MethodCallExpression;
                    }
                    while (call != null && IsIncludeMethod(call.Method));

                    if (path.Count == 0)
                    {
                        paths.Remove(path);
                    }

                    var parameter = Expression.Parameter(inner.Type.GetSequenceType());

                    // TODO: Formalize these assertions
                    var queryExpression = (EnumerableRelationalQueryExpression)inner;
                    var selectExpression = queryExpression.SelectExpression;
                    var projectionExpression = (ServerProjectionExpression)selectExpression.Projection;

                    var materializer 
                        = BuildMemberInitExpression(
                            parameter, 
                            projectionExpression.ResultLambda.Body, 
                            paths.AsEnumerable());

                    var genericSelectMethodInfo
                        = inner.Type.GetGenericTypeDefinition() == typeof(IQueryable<>)
                            ? queryableSelectMethodInfo
                            : enumerableSelectMethodInfo;

                    return Expression.Call(
                        genericSelectMethodInfo.MakeGenericMethod(parameter.Type, parameter.Type),
                        inner,
                        Expression.Lambda(materializer, parameter));
                }
                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private MemberInitExpression BuildMemberInitExpression(
            Expression accessor, 
            Expression expansion, 
            IEnumerable<IEnumerable<MemberInfo>> paths)
        {
            // TODO: Include any constructor arguments.
            var newExpression = Expression.New(accessor.Type);

            var bindings = new List<MemberBinding>();

            if (expansion.UnwrapAnnotations() is MemberInitExpression memberInitExpression)
            {
                newExpression = memberInitExpression.NewExpression;

                bindings.AddRange(from b in memberInitExpression.Bindings
                                  let m = Expression.MakeMemberAccess(accessor, b.Member)
                                  select Expression.Bind(b.Member, m));
            }

            // The paths are passed in reverse order so that joins appear in 'semantic' order.
            foreach (var pathset in paths.GroupBy(p => p.First(), p => p.Skip(1)).Reverse())
            {
                var navigation = descriptorSet.NavigationDescriptors.Single(n => n.Member == pathset.Key);
                var expression = Expression.MakeMemberAccess(accessor, pathset.Key) as Expression;

                if (pathset.Any(p => p.Any()))
                {
                    // TODO: Formalize these assertions
                    var queryExpression = (EnumerableRelationalQueryExpression)navigation.Expansion;
                    var selectExpression = queryExpression.SelectExpression;
                    var projectionExpression = (ServerProjectionExpression)selectExpression.Projection;

                    if (pathset.Key.GetMemberType().IsSequenceType())
                    {
                        var parameter = Expression.Parameter(pathset.Key.GetMemberType().GetSequenceType());

                        var materializer = BuildMemberInitExpression(parameter, projectionExpression.ResultLambda.Body, pathset);

                        expression = Expression.Call(
                            enumerableSelectMethodInfo.MakeGenericMethod(parameter.Type, parameter.Type),
                            expression,
                            Expression.Lambda(materializer, parameter));
                    }
                    else
                    {
                        expression = BuildMemberInitExpression(expression, projectionExpression.ResultLambda.Body, pathset);
                    }
                }

                bindings.Add(Expression.Bind(pathset.Key, expression));
            }
            
            return Expression.MemberInit(newExpression, bindings);
        }

        private static bool IsIncludeMethod(MethodInfo method)
        {
            return method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && method.Name.EndsWith(nameof(EntityFrameworkQueryableExtensions.Include));
        }

        private static bool IsThenIncludeMethod(MethodInfo method)
        {
            return method.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && method.Name.Equals(nameof(EntityFrameworkQueryableExtensions.ThenInclude));
        }

        private static MemberInfo GetMemberInfo(LambdaExpression lambda)
        {
            if (lambda.Body is MemberExpression memberExpression)
            {
                if (memberExpression.Expression == lambda.Parameters.Single())
                {
                    return memberExpression.Member;
                }

                if (memberExpression.Expression is UnaryExpression unaryExpression
                    && unaryExpression.Operand == lambda.Parameters.Single()
                    && (unaryExpression.NodeType == ExpressionType.Convert
                        || unaryExpression.NodeType == ExpressionType.TypeAs))
                {
                    return memberExpression.Member;
                }
            }

            // TODO: something better
            throw new InvalidOperationException();
        }
    }
}
