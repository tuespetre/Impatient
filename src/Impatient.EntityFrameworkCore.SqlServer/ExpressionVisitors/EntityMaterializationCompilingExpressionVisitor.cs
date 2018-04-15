using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EntityMaterializationCompilingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo getEntityUsingStateManagerMethodInfo
            = typeof(EntityMaterializationCompilingExpressionVisitor)
                .GetMethod(nameof(GetEntityUsingStateManager), BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo getEntityUsingIdentityMapWithFixupMethodInfo
            = typeof(EntityMaterializationCompilingExpressionVisitor)
                .GetMethod(nameof(GetEntityUsingIdentityMapWithFixup), BindingFlags.NonPublic | BindingFlags.Static);

        private readonly IModel model;
        private readonly ParameterExpression executionContextParameter;

        public EntityMaterializationCompilingExpressionVisitor(
            IModel model,
            ParameterExpression executionContextParameter)
        {
            this.model = model;
            this.executionContextParameter = executionContextParameter;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case EntityMaterializationExpression entityMaterializationExpression:
                {
                    var entityVariable = Expression.Variable(node.Type, "$entity");
                    var shadowPropertiesVariable = Expression.Variable(typeof(object[]), "$shadow");

                    var entityType = entityMaterializationExpression.EntityType;
                    var materializer = Visit(entityMaterializationExpression.Expression);

                    var getEntityMethodInfo = default(MethodInfo);

                    switch (entityMaterializationExpression.IdentityMapMode)
                    {
                        case IdentityMapMode.StateManager
                        when !entityType.HasDefiningNavigation():
                        {
                            getEntityMethodInfo = getEntityUsingStateManagerMethodInfo;
                            break;
                        }

                        case IdentityMapMode.IdentityMapWithFixup:
                        {
                            getEntityMethodInfo = getEntityUsingIdentityMapWithFixupMethodInfo;
                            break;
                        }

                        case IdentityMapMode.IdentityMap:
                        default:
                        {
                            getEntityMethodInfo = getEntityUsingIdentityMapWithFixupMethodInfo;
                            break;
                        }
                    }

                    return Expression.Block(
                        variables: new ParameterExpression[]
                        {
                            entityVariable,
                            shadowPropertiesVariable,
                        },
                        expressions: new Expression[]
                        {
                            Expression.Assign(
                                shadowPropertiesVariable,
                                Expression.NewArrayInit(
                                    typeof(object),
                                    from s in entityMaterializationExpression.Properties
                                    select Expression.Convert(s, typeof(object)))),
                            Expression.Assign(
                                entityVariable,
                                new CollectionNavigationFixupExpressionVisitor(model)
                                    .Visit(materializer)),
                            Expression.Convert(
                                Expression.Call(
                                    getEntityMethodInfo,
                                    Expression.Convert(executionContextParameter, typeof(EFCoreDbCommandExecutor)),
                                    Expression.Constant(entityType.RootType()),
                                    Expression.Constant(entityType.FindPrimaryKey()),
                                    entityMaterializationExpression.KeyExpression
                                        .UnwrapLambda()
                                        .ExpandParameters(entityVariable, shadowPropertiesVariable),
                                    entityVariable,
                                    Expression.Constant(entityMaterializationExpression.ShadowProperties),
                                    shadowPropertiesVariable,
                                    Expression.Constant(entityMaterializationExpression.IncludedNavigations)),
                                node.Type)
                        });
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private static object GetEntityUsingStateManager(
            EFCoreDbCommandExecutor executor,
            IEntityType entityType,
            IKey key,
            object[] keyValues,
            object entity,
            IProperty[] shadowProperties,
            object[] shadowPropertyValues,
            IReadOnlyList<INavigation> loadedNavigations)
        {
            var stateManager = executor.CurrentDbContext.GetDependencies().StateManager;

            var entry = stateManager.TryGetEntry(key, keyValues);

            if (entry == null)
            {
                entry = stateManager.GetOrCreateEntry(entity);

                for (var i = 0; i < shadowProperties.Length; i++)
                {
                    entry.SetProperty(shadowProperties[i], shadowPropertyValues[i], false);
                }

                stateManager.StartTracking(entry);

                entry.MarkUnchangedFromQuery(null);
            }

            foreach (var navigation in loadedNavigations)
            {
                entry.SetIsLoaded(navigation);
            }

            return entry.Entity;
        }

        private static object GetEntityUsingIdentityMapWithFixup(
            EFCoreDbCommandExecutor executor,
            IEntityType entityType,
            IKey key,
            object[] keyValues,
            object entity,
            IProperty[] shadowProperties,
            object[] shadowPropertyValues,
            IReadOnlyList<INavigation> includes)
        {
            if (entity ==  null)
            {
                return null;
            }

            var cached = entity;

            if (!executor.TryGetEntity(entityType.ClrType, keyValues, ref cached))
            {
                executor.CacheEntity(entityType, key, keyValues, entity, shadowProperties, shadowPropertyValues, includes);
            }

            // TODO: Keep information about the includes in a cache so we can return early
            // i.e. modify TryGetEntity to accept the include set and have an out flag
            // telling use whether this include set was processed already

            foreach (var navigation in includes)
            {
                var inverse = navigation.FindInverse();

                if (inverse == null)
                {
                    continue;
                }

                var value = navigation.GetGetter().GetClrValue(entity);

                if (value == null)
                {
                    continue;
                }

                navigation.GetSetter().SetClrValue(cached, value);

                if (inverse.IsCollection())
                {
                    var collection = inverse.GetCollectionAccessor();

                    collection.Add(value, cached);

                    continue;
                }
                else
                {
                    var setter = inverse.GetSetter();

                    if (value is IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            setter.SetClrValue(item, cached);
                        }
                    }
                    else
                    {
                        setter.SetClrValue(value, cached);
                    }
                }
            }

            return cached;
        }

        private class CollectionNavigationFixupExpressionVisitor : ExpressionVisitor
        {
            private readonly IModel model;

            public CollectionNavigationFixupExpressionVisitor(IModel model)
            {
                this.model = model;
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                var newExpression = VisitAndConvert(node.NewExpression, nameof(VisitMemberInit));
                var bindings = node.Bindings.Select(VisitMemberBinding).ToArray();

                var entityType = model.FindEntityType(node.Type);

                if (entityType != null)
                {
                    var additionalBindings = new List<MemberAssignment>();

                    var collectionMembers
                        = from n in entityType.GetNavigations()
                          where n.IsCollection()
                          from m in new[] { n.GetMemberInfo(true, true), n.GetMemberInfo(false, false) }
                          select m;

                    for (var i = 0; i < bindings.Length; i++)
                    {
                        if (collectionMembers.Contains(bindings[i].Member))
                        {
                            var sequenceType = bindings[i].Member.GetMemberType().GetSequenceType();

                            bindings[i]
                                = Expression.Bind(
                                    bindings[i].Member,
                                    Expression.Coalesce(
                                        ((MemberAssignment)bindings[i]).Expression.AsCollectionType(),
                                        Expression.New(typeof(List<>).MakeGenericType(sequenceType))));
                        }
                    }

                    return node.Update(
                        VisitAndConvert(node.NewExpression, nameof(VisitMemberInit)),
                        bindings.Concat(additionalBindings));
                }

                return base.VisitMemberInit(node);
            }
        }
    }
}
