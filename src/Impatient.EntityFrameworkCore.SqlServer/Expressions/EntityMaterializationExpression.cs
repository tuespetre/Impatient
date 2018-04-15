using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EntityMaterializationExpression : ExtraPropertiesExpression
    {
        public EntityMaterializationExpression(
            IEntityType entityType, 
            IdentityMapMode identityMapMode,
            Expression keyExpression,
            IEnumerable<IProperty> shadowProperties,
            IEnumerable<Expression> shadowPropertyExpressions,
            Expression expression,
            IEnumerable<INavigation> includedNavigations = null) 
            : base(expression)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            IdentityMapMode = identityMapMode;
            KeyExpression = keyExpression ?? throw new ArgumentNullException(nameof(keyExpression));
            ShadowProperties = shadowProperties.ToArray();
            IncludedNavigations = includedNavigations?.ToArray() ?? new INavigation[0];
            Names = shadowProperties.Select(p => p.Name).ToArray();
            Properties = new ReadOnlyCollection<Expression>(shadowPropertyExpressions.ToArray());
        }

        public IEntityType EntityType { get; }

        public IdentityMapMode IdentityMapMode { get; }

        public Expression KeyExpression { get; }

        public IReadOnlyList<IProperty> ShadowProperties { get; }

        public IReadOnlyList<INavigation> IncludedNavigations { get; }

        public override IReadOnlyList<string> Names { get; }

        public override ReadOnlyCollection<Expression> Properties { get; }

        public override ExtraPropertiesExpression Update(Expression expression, IEnumerable<Expression> properties)
        {
            if (expression != Expression
                || !properties.SequenceEqual(Properties))
            {
                return new EntityMaterializationExpression(
                    EntityType,
                    IdentityMapMode,
                    KeyExpression,
                    ShadowProperties,
                    properties,
                    expression,
                    IncludedNavigations);
            }

            return this;
        }

        public EntityMaterializationExpression UpdateIdentityMapMode(IdentityMapMode identityMapMode)
        {
            return new EntityMaterializationExpression(
                EntityType, 
                identityMapMode, 
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
                IdentityMapMode,
                KeyExpression,
                ShadowProperties,
                Properties,
                Expression,
                IncludedNavigations.Append(navigation).Distinct());
        }

        public override int GetAnnotationHashCode() => EntityType.GetHashCode();
    }
}
