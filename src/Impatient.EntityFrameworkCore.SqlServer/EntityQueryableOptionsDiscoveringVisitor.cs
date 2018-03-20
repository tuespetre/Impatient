using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    // TODO: Verify how this works with result selectors like SelectMany
    internal class EntityQueryableOptionsDiscoveringVisitor : ExpressionVisitor
    {
        public bool FoundAsNoTracking { get; private set; }

        public bool FoundAsTracking { get; private set; }

        public bool FoundIgnoreQueryFilters { get; private set; }

        public override Expression Visit(Expression node)
        {
            if (node is MethodCallExpression call && call.Method.DeclaringType == typeof(EntityFrameworkQueryableExtensions))
            {
                switch (call.Method.Name)
                {
                    case nameof(EntityFrameworkQueryableExtensions.AsNoTracking):
                    {
                        FoundAsNoTracking = true;
                        break;
                    }

                    case nameof(EntityFrameworkQueryableExtensions.AsTracking):
                    {
                        FoundAsTracking = true;
                        break;
                    }

                    case nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters):
                    {
                        FoundIgnoreQueryFilters = true;
                        break;
                    }
                }
            }

            return base.Visit(node);
        }
    }
}
