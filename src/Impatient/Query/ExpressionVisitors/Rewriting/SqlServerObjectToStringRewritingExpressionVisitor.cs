﻿using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlServerObjectToStringRewritingExpressionVisitor : ExpressionVisitor
    {        
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(ToString) 
                && node.Arguments.Count == 0 
                && node.Object is not null
                && node.Object.Type.IsScalarType()
                && !node.Object.Type.IsEnum())
            {
                return new SqlFunctionExpression(
                    "CONVERT", 
                    node.Type, 
                    new SqlFragmentExpression("VARCHAR(100)"), 
                    node.Object);
            }

            return node;
        }
    }
}
