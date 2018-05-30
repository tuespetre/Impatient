using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EntityMaterializationCompilingExpressionVisitor : ExpressionVisitor
    {
        private readonly IModel model;
        private readonly ParameterExpression executionContextParameter;
        private readonly Dictionary<string, int> identifierCounts = new Dictionary<string, int>();

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
                    var entityVariable = Expression.Variable(node.Type, "entity");
                    var shadowPropertiesVariable = Expression.Variable(typeof(object[]), "shadow");

                    var entityType = entityMaterializationExpression.EntityType;
                    var materializer = Visit(entityMaterializationExpression.Expression);

                    var getEntityMethodInfo = default(MethodInfo);

                    switch (entityMaterializationExpression.IdentityMapMode)
                    {
                        case IdentityMapMode.StateManager:
                        {
                            getEntityMethodInfo = EntityTrackingHelper.GetEntityUsingStateManagerMethodInfo;
                            break;
                        }
                        
                        case IdentityMapMode.IdentityMap:
                        default:
                        {
                            getEntityMethodInfo = EntityTrackingHelper.GetEntityUsingIdentityMapMethodInfo;
                            break;
                        }
                    }

                    var shadowPropertiesExpression = (Expression)Expression.Constant(new object[0]);
                    var shadowProperties = entityMaterializationExpression.ShadowProperties;

                    if (!shadowProperties.IsDefaultOrEmpty)
                    {
                        var values 
                            = Enumerable
                                .Repeat(Expression.Constant(null), entityType.PropertyCount())
                                .Cast<Expression>()
                                .ToArray();

                        for (var i = 0; i < shadowProperties.Length; i++)
                        {
                            values[shadowProperties[i].GetIndex()]
                                = Expression.Convert(
                                    entityMaterializationExpression.Properties[i], 
                                    typeof(object));
                        }

                        shadowPropertiesExpression = Expression.NewArrayInit(typeof(object), values);
                    }

                    var result 
                        = Expression.Block(
                            variables: new ParameterExpression[]
                            {
                                entityVariable,
                                shadowPropertiesVariable,
                            },
                            expressions: new Expression[]
                            {
                                Expression.Assign(
                                    shadowPropertiesVariable,
                                    shadowPropertiesExpression),
                                Expression.Assign(
                                    entityVariable,
                                    new CollectionNavigationFixupExpressionVisitor(model)
                                        .Visit(materializer)),
                                Expression.Convert(
                                    Expression.Call(
                                        getEntityMethodInfo,
                                        Expression.Convert(executionContextParameter, typeof(EFCoreDbCommandExecutor)),
                                        Expression.Constant(entityType),
                                        entityMaterializationExpression.KeyExpression
                                            .UnwrapLambda()
                                            .ExpandParameters(entityVariable, shadowPropertiesVariable),
                                        entityVariable,
                                        shadowPropertiesVariable,
                                        Expression.Constant(entityMaterializationExpression.IncludedNavigations.ToList())),
                                    node.Type)
                            });

                    var identifier = $"MaterializeEntity_{entityType.DisplayName()}";

                    if (identifierCounts.TryGetValue(identifier, out var count))
                    {
                        identifierCounts[identifier] = count + 1;

                        identifier += $"_{count}";
                    }
                    else
                    {
                        identifierCounts[identifier] = 1;

                        identifier += "_0";
                    }

                    return MaterializationUtilities.Invoke(result, identifier);
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private class CollectionNavigationFixupExpressionVisitor : ExpressionVisitor
        {
            private readonly IModel model;

            public CollectionNavigationFixupExpressionVisitor(IModel model)
            {
                this.model = model;
            }

            protected override Expression VisitExtension(Expression node)
            {
                switch (node)
                {
                    case ExtendedMemberInitExpression extendedMemberInitExpression:
                    {
                        return VisitExtendedMemberInit(extendedMemberInitExpression);
                    }

                    default:
                    {
                        return base.VisitExtension(node);
                    }
                }
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                var newExpression = VisitAndConvert(node.NewExpression, nameof(VisitMemberInit));
                var bindings = node.Bindings.Select(VisitMemberBinding).ToArray();

                var entityType = model.FindEntityType(node.Type);

                if (entityType != null)
                {
                    var collectionMembers
                        = from n in entityType.GetNavigations()
                          where n.IsCollection()
                          from m in new[] { n.GetReadableMemberInfo(), n.GetWritableMemberInfo() }
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
                }
                
                return node.Update(newExpression, bindings);
            }

            protected virtual Expression VisitExtendedMemberInit(ExtendedMemberInitExpression node)
            {
                var newExpression = VisitAndConvert(node.NewExpression, nameof(VisitExtendedMemberInit));
                var arguments = Visit(node.Arguments).ToArray();

                var entityType = model.FindEntityType(node.Type);

                if (entityType != null)
                {
                    var collectionMembers
                        = from n in entityType.GetNavigations()
                          where n.IsCollection()
                          from m in new[] { n.GetReadableMemberInfo(), n.GetWritableMemberInfo() }
                          select m;

                    for (var i = 0; i < arguments.Length; i++)
                    {
                        if (collectionMembers.Contains(node.WritableMembers[i]))
                        {
                            var sequenceType = node.WritableMembers[i].GetMemberType().GetSequenceType();

                            arguments[i]
                                = Expression.Coalesce(
                                    arguments[i].AsCollectionType(),
                                    Expression.New(typeof(List<>).MakeGenericType(sequenceType)));
                        }
                    }
                }

                return node.Update(newExpression, arguments);
            }
        }
    }
}
