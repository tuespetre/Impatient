using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class ChangeTrackerInjectingExpressionVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression executionContextParameter;

        public ChangeTrackerInjectingExpressionVisitor(ParameterExpression executionContextParameter)
        {
            this.executionContextParameter = executionContextParameter;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case EntityMaterializationExpression entityMaterializationExpression:
                {
                    var entityVariable = Expression.Variable(node.Type, "$entity");

                    return Expression.Block(
                        variables: new ParameterExpression[]
                        {
                            entityVariable
                        },
                        expressions: new Expression[]
                        {
                            Expression.Assign(entityVariable, entityMaterializationExpression.Expression),
                            Expression.Convert(
                                Expression.Call(
                                    GetEntityMethodInfo,
                                    Expression.Convert(executionContextParameter, typeof(EFCoreDbCommandExecutor)),
                                    Expression.Constant(entityMaterializationExpression.EntityType.FindPrimaryKey()),
                                    entityMaterializationExpression.KeyExpression
                                        .UnwrapLambda()
                                        .ExpandParameters(entityVariable),
                                    entityVariable,
                                    Expression.Constant(entityMaterializationExpression.EntityState)),
                                node.Type)
                        });
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private static readonly MethodInfo GetEntityMethodInfo
            = typeof(ChangeTrackerInjectingExpressionVisitor)
                .GetMethod(nameof(GetEntity), BindingFlags.NonPublic | BindingFlags.Static);

        private static object GetEntity(EFCoreDbCommandExecutor executor, IKey key, object[] values, object entity, EntityState entityState)
        {
            var stateManager = executor.CurrentDbContext.GetDependencies().StateManager;

            var entry = stateManager.TryGetEntry(key, values);

            if (entry != null)
            {
                return entry.Entity;
            }

            if (entity != null)
            {
                entry = stateManager.GetOrCreateEntry(entity, key.DeclaringEntityType);

                entry.SetEntityState(entityState);

                stateManager.StartTracking(entry);
            }

            return entity;
        }
    }
}
