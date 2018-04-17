using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class IncludeCompilingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case IncludeExpression includeExpression:
                {
                    var expression = includeExpression.Expression;

                    var blockVariables = new List<ParameterExpression>();
                    var blockExpressions = new List<Expression>();

                    var variable = Expression.Variable(expression.Type);

                    blockVariables.Add(variable);

                    blockExpressions.Add(Expression.Assign(variable, expression));

                    for (var i = 0; i < includeExpression.Includes.Count; i++)
                    {
                        var include = includeExpression.Includes[i];
                        var path = includeExpression.Paths[i];

                        var target = (Expression)variable;

                        for (var j = 0; j < path.Length; j++)
                        {
                            target = Expression.MakeMemberAccess(target, path[j].PropertyInfo);
                        }

                        if (target.Type.IsCollectionType())
                        {
                            include = include.AsCollectionType();
                        }

                        blockExpressions.Add(Expression.Assign(target, include));
                    }

                    blockExpressions.Add(variable);

                    return Expression.Block(blockVariables, blockExpressions);
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
