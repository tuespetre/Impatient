using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Projection;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;

public class ShadowPropertyRemovingExpressionVisitor : ExpressionVisitor
{
    private static readonly CoreShadowPropertyRemovingExpressionVisitor coreVisitor = new();

    public override Expression Visit(Expression node)
    {
        if (node is QueryOptionsExpression { QueryTrackingBehavior: QueryTrackingBehavior.NoTracking or QueryTrackingBehavior.NoTrackingWithIdentityResolution })
        {
            // remove shadow properties from materialization expressions
            // so they are not pulled from the server.

            return coreVisitor.Visit(node);
        }

        return node;
    }

    private class CoreShadowPropertyRemovingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression select:
                {
                    return select.UpdateProjection(
                        VisitAndConvert(
                            select.Projection, 
                            nameof(VisitExtension)));
                }

                case EntityMaterializationExpression entity:
                {
                    return new EntityMaterializationExpression(
                        entity.EntityType,
                        entity.QueryTrackingBehavior,
                        Visit(entity.KeyExpression),
                        [],
                        [],
                        Visit(entity.Expression),
                        entity.IncludedNavigations);
                }
                default:
                {
                    return base.VisitExtension(node);
                }
            }
        }
    }
}
