using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class ShadowPropertyCompilingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel model;
        private ParameterExpression executionContextParameter;

        public ShadowPropertyCompilingExpressionVisitor(
            IModel model,
            ParameterExpression executionContextParameter)
        {
            this.model = model;
            this.executionContextParameter = executionContextParameter;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.IsEFPropertyMethod())
            {
                var expression = arguments[0];
                var propertyNameArgument = arguments[1];

                var propertyName = default(string);

                if (propertyNameArgument.UnwrapConversions() is ConstantExpression constantExpression)
                {
                    propertyName = (string)constantExpression.Value;
                }
                else
                {
                    throw new NotSupportedException(
                        $"Impatient does not support expressions of type " +
                        $"{propertyNameArgument.GetType()} for the " +
                        $"property name argument to EF.Property.");
                }

                var entityType = model.GetEntityTypes(arguments[0].Type).FirstOrDefault();

                if (entityType != null)
                {
                    var innerExpression = arguments[0];
                    var path = new List<MemberInfo>();
                    var currentType = entityType;

                    while (innerExpression is MemberExpression memberExpression
                        && currentType.HasDefiningNavigation())
                    {
                        var definingNavigation 
                            = currentType.DefiningEntityType.FindNavigation(
                                currentType.DefiningNavigationName);

                        if (definingNavigation?.MemberInfo != memberExpression.Member)
                        {
                            return node.Update(@object, arguments);
                        }

                        currentType = definingNavigation.DeclaringEntityType;

                        path.Insert(0, memberExpression.Member);

                        innerExpression = memberExpression.Expression.UnwrapAnnotationsAndConversions();
                    }

                    var entry
                        = (Expression)Expression.Call(
                            Expression.MakeMemberAccess(
                                Expression.MakeMemberAccess(
                                    Expression.Convert(
                                        executionContextParameter,
                                        typeof(EFCoreDbCommandExecutor)),
                                    typeof(EFCoreDbCommandExecutor).GetProperty(nameof(EFCoreDbCommandExecutor.CurrentDbContext))),
                                typeof(ICurrentDbContext).GetProperty(nameof(ICurrentDbContext.Context))),
                            typeof(DbContext).GetMethods().Single(m => !m.IsGenericMethod && m.Name == nameof(DbContext.Entry)),
                            innerExpression);

                    foreach (var member in path)
                    {
                        entry
                            = Expression.MakeMemberAccess(
                                Expression.Call(
                                    entry,
                                    typeof(EntityEntry).GetMethod(nameof(EntityEntry.Reference)),
                                    Expression.Constant(member.Name)),
                                typeof(ReferenceEntry).GetProperty(nameof(ReferenceEntry.TargetEntry)));
                    }

                    var finalExpression = default(Expression);

                    var property = entityType.FindProperty(propertyName);

                    if (property != null)
                    {
                        finalExpression 
                            = Expression.MakeMemberAccess(
                                Expression.Call(
                                    entry,
                                    typeof(EntityEntry).GetMethod(nameof(EntityEntry.Property)),
                                    arguments[1]),
                                typeof(PropertyEntry).GetProperty(nameof(PropertyEntry.CurrentValue)));
                    }
                    else
                    {
                        var navigation = entityType.FindNavigation(propertyName);

                        if (navigation != null)
                        {
                            if (navigation.IsCollection())
                            {
                                finalExpression =
                                    Expression.MakeMemberAccess(
                                        Expression.Call(
                                            entry,
                                            typeof(EntityEntry).GetMethod(nameof(EntityEntry.Collection)),
                                            arguments[1]),
                                        typeof(CollectionEntry).GetProperty(nameof(CollectionEntry.CurrentValue), typeof(IEnumerable)));
                            }
                            else
                            {
                                finalExpression =
                                    Expression.MakeMemberAccess(
                                        Expression.Call(
                                            entry,
                                            typeof(EntityEntry).GetMethod(nameof(EntityEntry.Reference)),
                                            arguments[1]),
                                        typeof(ReferenceEntry).GetProperty(nameof(ReferenceEntry.CurrentValue)));
                            }
                        }
                    }

                    if (finalExpression != null)
                    {
                        return Expression.Condition(
                            Expression.NotEqual(innerExpression, Expression.Default(innerExpression.Type)),
                            Expression.Convert(finalExpression, node.Type),
                            Expression.Default(node.Type));
                    }
                }
            }

            return node.Update(@object, arguments);
        }

        private static IList<MemberInfo> UnwindMemberExpression(
            MemberExpression memberExpression,
            out Expression innerExpression)
        {
            var path = new List<MemberInfo>();

            do
            {
                path.Insert(0, memberExpression.Member);

                innerExpression = memberExpression.Expression.UnwrapAnnotationsAndConversions();

                memberExpression = innerExpression as MemberExpression;
            }
            while (memberExpression != null);

            return path;
        }
    }
}
