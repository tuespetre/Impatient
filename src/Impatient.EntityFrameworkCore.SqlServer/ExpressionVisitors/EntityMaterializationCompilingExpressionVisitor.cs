using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
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
