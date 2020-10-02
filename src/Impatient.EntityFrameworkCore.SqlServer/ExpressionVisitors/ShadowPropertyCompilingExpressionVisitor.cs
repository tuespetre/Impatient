using Impatient.Extensions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

        public ShadowPropertyCompilingExpressionVisitor(IModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.IsEFPropertyMethod())
            {
                var expression = arguments[0].UnwrapInnerExpression();
                var propertyNameArgument = arguments[1];

                var entityType = model.GetEntityTypes(expression.Type).FirstOrDefault();

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

                        if (definingNavigation?.GetSemanticReadableMemberInfo() != memberExpression.Member)
                        {
                            return node.Update(@object, arguments);
                        }

                        currentType = definingNavigation.DeclaringEntityType;

                        path.Insert(0, memberExpression.Member);

                        innerExpression = memberExpression.Expression.UnwrapInnerExpression();
                    }

                    var entry
                        = (Expression)Expression.Call(
                            Expression.MakeMemberAccess(
                                Expression.MakeMemberAccess(
                                    Expression.Convert(
                                        ExecutionContextParameters.DbCommandExecutor,
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

                    if (model.GetEntityTypes().Any(t => t.ClrType == node.Type))
                    {
                        finalExpression =
                            Expression.MakeMemberAccess(
                                Expression.Call(
                                    entry,
                                    typeof(EntityEntry).GetMethod(nameof(EntityEntry.Reference)),
                                    arguments[1]),
                                typeof(ReferenceEntry).GetProperty(nameof(ReferenceEntry.CurrentValue)));
                    }
                    else if (node.Type.IsCollectionType())
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
                        finalExpression 
                            = Expression.MakeMemberAccess(
                                Expression.Call(
                                    entry,
                                    typeof(EntityEntry).GetMethod(nameof(EntityEntry.Property)),
                                    arguments[1]),
                                typeof(PropertyEntry).GetProperty(nameof(PropertyEntry.CurrentValue)));
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
    }
}
