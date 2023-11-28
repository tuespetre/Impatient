using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class JsonMaterializationExpression(Type type, IEntityType entityType) : Expression, ISemanticHashCodeProvider
    {
        public IEntityType EntityType => entityType;

        public override Type Type => type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            var hash = new HashCode();
            hash.Add(Type);
            hash.Add(EntityType);
            return hash.ToHashCode();
        }
    }
}
