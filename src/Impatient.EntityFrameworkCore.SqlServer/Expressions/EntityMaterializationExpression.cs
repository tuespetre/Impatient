using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EntityMaterializationExpression : ExtraPropertiesExpression
    {
        public EntityMaterializationExpression(
            IEntityType entityType, 
            QueryTrackingBehavior queryTrackingBehavior,
            Expression keyExpression,
            IEnumerable<IProperty> shadowProperties,
            IEnumerable<Expression> shadowPropertyExpressions,
            Expression expression,
            IEnumerable<INavigation> includedNavigations = null) 
            : base(expression)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            QueryTrackingBehavior = queryTrackingBehavior;
            KeyExpression = keyExpression ?? throw new ArgumentNullException(nameof(keyExpression));
            ShadowProperties = shadowProperties.ToImmutableArray();
            IncludedNavigations = includedNavigations?.ToImmutableArray() ?? ImmutableArray.Create<INavigation>();
            Names = new ReadOnlyCollection<string>(shadowProperties.Select(p => p.Name).ToArray());
            Properties = new ReadOnlyCollection<Expression>(shadowPropertyExpressions.ToArray());
        }

        public IEntityType EntityType { get; }

        public QueryTrackingBehavior QueryTrackingBehavior { get; }

        public Expression KeyExpression { get; }

        public ImmutableArray<IProperty> ShadowProperties { get; }

        public ImmutableArray<INavigation> IncludedNavigations { get; }

        public override ReadOnlyCollection<string> Names { get; }

        public override ReadOnlyCollection<Expression> Properties { get; }

        public override ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties)
        {
            if (expression != Expression
                || !properties.SequenceEqual(Properties))
            {
                return new EntityMaterializationExpression(
                    EntityType,
                    QueryTrackingBehavior,
                    KeyExpression,
                    ShadowProperties,
                    properties,
                    expression,
                    IncludedNavigations);
            }

            return this;
        }

        public EntityMaterializationExpression UpdateQueryTrackingBehavior(QueryTrackingBehavior queryTrackingBehavior)
        {
            return new EntityMaterializationExpression(
                EntityType, 
                queryTrackingBehavior, 
                KeyExpression, 
                ShadowProperties, 
                Properties,
                Expression,
                IncludedNavigations);
        }

        public EntityMaterializationExpression IncludeNavigation(INavigation navigation)
        {
            return new EntityMaterializationExpression(
                EntityType,
                QueryTrackingBehavior,
                KeyExpression,
                ShadowProperties,
                Properties,
                Expression,
                IncludedNavigations.Append(navigation).Distinct());
        }

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = EntityType.GetHashCode();

                hash = (hash * 16777619) ^ comparer.GetHashCode(Expression);

                return hash;
            }
        }
    }
}
