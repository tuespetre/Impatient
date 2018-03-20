using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EntityMaterializationExpression : AnnotationExpression
    {
        public EntityMaterializationExpression(
            IEntityType entityType, 
            EntityState entityState,
            Expression keyExpression,
            Expression expression) 
            : base(expression)
        {
            EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType));
            EntityState = entityState;
            KeyExpression = keyExpression ?? throw new ArgumentNullException(nameof(keyExpression));
        }

        public IEntityType EntityType { get; }

        public EntityState EntityState { get; }

        public Expression KeyExpression { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            if (expression != Expression)
            {
                return new EntityMaterializationExpression(EntityType, EntityState, KeyExpression, expression);
            }

            return this;
        }
    }
}
