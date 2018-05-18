using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class EntityTypeInjectionExpression : LateBoundProjectionLeafExpression
    {
        public EntityTypeInjectionExpression(IEntityType entityType)
        {
            EntityType = entityType;
        }

        public override Type Type => typeof(IEntityType);

        public IEntityType EntityType { get; }

        public override Expression Reduce()
        {
            return Constant(EntityType, typeof(IEntityType));
        }
    }
}
