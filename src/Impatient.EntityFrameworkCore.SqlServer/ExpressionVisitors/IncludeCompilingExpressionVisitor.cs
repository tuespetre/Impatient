using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class IncludeCompilingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            var visited = base.VisitExtension(node);

            switch (visited)
            {
                case IncludeExpression includeExpression:
                {
                    var expression = includeExpression.Expression;

                    var variables = new List<ParameterExpression>();
                    var expressions = new List<Expression>();

                    var entityVariable = Expression.Variable(expression.Type, "entity");
                    var returnLabelTarget = Expression.Label(expression.Type, "Return");

                    expressions.Add(
                        Expression.Assign(entityVariable, expression));

                    expressions.Add(
                        Expression.IfThen(
                            Expression.Equal(entityVariable, Expression.Constant(null)), 
                            Expression.Return(returnLabelTarget, entityVariable)));

                    for (var i = 0; i < includeExpression.Includes.Count; i++)
                    {
                        var include = includeExpression.Includes[i];
                        var includeVariable = Expression.Variable(include.Type, includeExpression.Names[i]);

                        variables.Insert(i, includeVariable);
                        expressions.Insert(i, Expression.Assign(includeVariable, include));

                        include = includeVariable;

                        var path = includeExpression.Paths[i];
                        var target = (Expression)entityVariable;
                        var innerVariables = new List<ParameterExpression>();
                        var innerExpressions = new List<Expression>();
                        var innerBlockEndLabelTarget = Expression.Label();

                        for (var j = 0; j < path.Length - 1; j++)
                        {
                            // Prefer field over property access b/c proxies, etc.
                            var member = path[j].FieldInfo ?? path[j].GetSemanticReadableMemberInfo();
                            var access = Expression.MakeMemberAccess(target, member);
                            var innerVariable = Expression.Variable(access.Type, member.Name);

                            innerVariables.Add(innerVariable);

                            innerExpressions.Add(
                                Expression.Assign(innerVariable, access));

                            innerExpressions.Add(
                                Expression.IfThen(
                                    Expression.Equal(innerVariable, Expression.Constant(null)),
                                    Expression.Goto(innerBlockEndLabelTarget)));

                            target = innerVariable;
                        }

                        target = Expression.MakeMemberAccess(target, path[path.Length - 1].GetWritableMemberInfo());

                        if (target.Type.IsCollectionType())
                        {
                            include = include.AsCollectionType();
                        }

                        innerExpressions.Add(Expression.Assign(target, include));
                        innerExpressions.Add(Expression.Label(innerBlockEndLabelTarget));

                        expressions.Add(Expression.Block(innerVariables, innerExpressions));
                    }

                    variables.Add(entityVariable);

                    expressions.Add(Expression.Label(returnLabelTarget, entityVariable));

                    return Expression.Block(variables, expressions);
                }

                default:
                {
                    return visited;
                }
            }
        }
    }
}
