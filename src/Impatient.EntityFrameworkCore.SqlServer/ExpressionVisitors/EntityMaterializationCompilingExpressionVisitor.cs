using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static System.Linq.Expressions.Expression;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;

public class EntityMaterializationCompilingExpressionVisitor(IModel model) : ExpressionVisitor
{
    private readonly IModel model = model ?? throw new ArgumentNullException(nameof(model));
    private readonly Dictionary<string, int> identifierCounts = [];

    public override Expression Visit(Expression node)
    {
        switch (node)
        {
            case EntityMaterializationExpression entityMaterializationExpression:
            {
                var entityVariable = Variable(node.Type, "entity");
                var shadowPropertiesVariable = Variable(typeof(object[]), "shadow");

                var entityType = entityMaterializationExpression.EntityType;
                var materializer = Visit(entityMaterializationExpression.Expression);
                var materializerInvocation = new CollectionNavigationFixupExpressionVisitor(model, entityType).Visit(materializer);

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

                if (entityMaterializationExpression.QueryTrackingBehavior == QueryTrackingBehavior.NoTracking)
                {
                    return MaterializationUtilities.Invoke(materializerInvocation, identifier);
                }

                var shadowPropertiesExpression = (Expression)Constant(Array.Empty<object>());
                var shadowProperties = entityMaterializationExpression.ShadowProperties;

                if (!shadowProperties.IsDefaultOrEmpty)
                {
                    var values
                        = Enumerable
                            .Repeat(Constant(null), entityType.GetProperties().Count(p => p.IsShadowProperty()))
                            .Cast<Expression>()
                            .ToArray();

                    for (var i = 0; i < shadowProperties.Length; i++)
                    {
                        values[shadowProperties[i].GetShadowIndex()]
                            = Convert(
                                entityMaterializationExpression.Properties[i],
                                typeof(object));
                    }

                    shadowPropertiesExpression = NewArrayInit(typeof(object), values);
                }

                var result
                    = Block(
                        variables: new ParameterExpression[]
                        {
                            entityVariable,
                            shadowPropertiesVariable,
                        },
                        expressions:
                        [
                            // TODO:
                            // the ordering of expressions here is kind of a leaked concern,
                            // because the query translating visitor spits out the columns
                            // into the select statement for the 'extra properties' first.
                            // it would be nice to make things be independent somehow.
                            Assign(shadowPropertiesVariable, shadowPropertiesExpression),
                            Assign(entityVariable, materializerInvocation),
                            Convert(
                                Call(
                                    EntityTrackingHelper.GetEntityUsingStateManagerMethodInfo,
                                    Convert(
                                        ExecutionContextParameters.DbCommandExecutor,
                                        typeof(EFCoreDbCommandExecutor)),
                                    Constant(entityMaterializationExpression.QueryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                                    Constant(entityType),
                                    entityMaterializationExpression.KeyExpression
                                        .UnwrapLambda()
                                        .ExpandParameters(entityVariable, shadowPropertiesVariable),
                                    entityVariable,
                                    shadowPropertiesVariable,
                                    Constant(entityMaterializationExpression.IncludedNavigations.ToList())),
                                node.Type)
                        ]);

                return MaterializationUtilities.Invoke(result, identifier);
            }

            default:
            {
                return base.Visit(node);
            }
        }
    }

    private class CollectionNavigationFixupExpressionVisitor(IModel model, IEntityType entityType) : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case EntityMaterializationExpression entityMaterializationExpression:
                {
                    throw new NotImplementedException();
                }

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

            var collectionMembers
                = from n in entityType.GetNavigations()
                    where n.IsCollection
                    from m in new[] { n.GetSemanticReadableMemberInfo(), n.GetWritableMemberInfo() }
                    select m;

            for (var i = 0; i < bindings.Length; i++)
            {
                if (collectionMembers.Contains(bindings[i].Member))
                {
                    var collection = ((MemberAssignment)bindings[i]).Expression.AsCollectionType();
                    var elementType = collection.Type.GetSequenceType();
                    //var elementType = bindings[i].Member.GetMemberType().GetSequenceType();
                    var listType = typeof(List<>).MakeGenericType(elementType);

                    bindings[i] = Bind(bindings[i].Member, Coalesce(collection, New(listType)));
                }
            }

            return node.Update(newExpression, bindings);
        }

        protected virtual Expression VisitExtendedMemberInit(ExtendedMemberInitExpression node)
        {
            var newExpression = VisitAndConvert(node.NewExpression, nameof(VisitExtendedMemberInit));
            var arguments = Visit(node.Arguments).ToArray();

            var collectionMembers
                = from n in entityType.GetNavigations()
                    where n.IsCollection
                    from m in new[] { n.GetSemanticReadableMemberInfo(), n.GetWritableMemberInfo() }
                    select m;

            for (var i = 0; i < arguments.Length; i++)
            {
                if (collectionMembers.Contains(node.WritableMembers[i]))
                {
                    var collection = arguments[i].AsCollectionType();
                    var elementType = collection.Type.GetSequenceType();
                    var listType = typeof(List<>).MakeGenericType(elementType);

                    arguments[i] = Coalesce(collection, New(listType));
                }
            }

            return node.Update(newExpression, arguments);
        }
    }
}
