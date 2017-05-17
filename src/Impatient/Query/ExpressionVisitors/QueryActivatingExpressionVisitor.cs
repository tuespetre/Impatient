using Impatient.Query.Expressions;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class QueryActivatingExpressionVisitor : ExpressionVisitor
    {
        private readonly ImpatientQueryProvider provider;

        private string alias;

        public QueryActivatingExpressionVisitor(ImpatientQueryProvider provider)
        {
            this.provider = provider;
        }

        protected override Expression VisitExtension(Expression node)
        {
            if (alias != null
                && !alias.StartsWith("<>")
                && node is EnumerableRelationalQueryExpression enumerableRelationalQueryExpression
                && enumerableRelationalQueryExpression.SelectExpression.Table is BaseTableExpression oldTableExpression)
            {
                var newTableExpression
                    = new BaseTableExpression(
                        oldTableExpression.SchemaName,
                        oldTableExpression.TableName,
                        alias,
                        oldTableExpression.Type);

                return enumerableRelationalQueryExpression.Replace(oldTableExpression, newTableExpression);
            }

            return base.VisitExtension(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable && queryable.Provider == provider)
            {
                return Visit(queryable.Expression);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                || node.Method.DeclaringType == typeof(Enumerable))
            {
                switch (node.Method.Name)
                {
                    case nameof(Queryable.All):
                    case nameof(Queryable.Any) when node.Arguments.Count == 2:
                    case nameof(Queryable.Average) when node.Arguments.Count == 2:
                    case nameof(Queryable.Count) when node.Arguments.Count == 2:
                    case nameof(Queryable.First) when node.Arguments.Count == 2:
                    case nameof(Queryable.FirstOrDefault) when node.Arguments.Count == 2:
                    case nameof(Queryable.Last) when node.Arguments.Count == 2:
                    case nameof(Queryable.LastOrDefault) when node.Arguments.Count == 2:
                    case nameof(Queryable.LongCount) when node.Arguments.Count == 2:
                    case nameof(Queryable.Max) when node.Arguments.Count == 2:
                    case nameof(Queryable.Min) when node.Arguments.Count == 2:
                    case nameof(Queryable.OrderBy):
                    case nameof(Queryable.OrderByDescending):
                    case nameof(Queryable.Select):
                    case nameof(Queryable.Single) when node.Arguments.Count == 2:
                    case nameof(Queryable.SingleOrDefault) when node.Arguments.Count == 2:
                    case nameof(Queryable.SkipWhile):
                    case nameof(Queryable.Sum) when node.Arguments.Count == 2:
                    case nameof(Queryable.TakeWhile):
                    case nameof(Queryable.ThenBy):
                    case nameof(Queryable.ThenByDescending):
                    case nameof(Queryable.Where):
                    {
                        var arguments = new Expression[node.Arguments.Count];
                        var alias = this.alias;
                        this.alias = node.Arguments[1].UnwrapLambda().Parameters[0].Name;
                        arguments[0] = Visit(node.Arguments[0]);
                        this.alias = null;
                        arguments[1] = Visit(node.Arguments[1]);
                        var result = node.Update(Visit(node.Object), arguments);
                        this.alias = alias;
                        return result;
                    }

                    case nameof(Queryable.Concat):
                    case nameof(Queryable.Except):
                    case nameof(Queryable.Intersect):
                    case nameof(Queryable.Union):
                    {
                        return base.VisitMethodCall(node);
                    }

                    case nameof(Queryable.GroupJoin):
                    case nameof(Queryable.Join):
                    {
                        var arguments = new Expression[node.Arguments.Count];
                        var alias = this.alias;
                        this.alias = node.Arguments[2].UnwrapLambda().Parameters[0].Name;
                        arguments[0] = Visit(node.Arguments[0]);
                        this.alias = node.Arguments[3].UnwrapLambda().Parameters[0].Name;
                        arguments[1] = Visit(node.Arguments[1]);
                        this.alias = null;
                        for (var i = 2; i < node.Arguments.Count; i++)
                        {
                            arguments[i] = Visit(node.Arguments[i]);
                        }
                        var result = node.Update(Visit(node.Object), arguments);
                        this.alias = alias;
                        return result;
                    }

                    case nameof(Queryable.SelectMany):
                    {
                        var arguments = new Expression[node.Arguments.Count];
                        var alias = this.alias;
                        this.alias = node.Arguments[1].UnwrapLambda().Parameters[0].Name;
                        arguments[0] = Visit(node.Arguments[0]);
                        this.alias = null;
                        for (var i = 1; i < node.Arguments.Count; i++)
                        {
                            arguments[i] = Visit(node.Arguments[i]);
                        }
                        var result = node.Update(Visit(node.Object), arguments);
                        this.alias = alias;
                        return result;
                    }

                    case nameof(Queryable.Zip):
                    {
                        var arguments = new Expression[node.Arguments.Count];
                        var alias = this.alias;
                        this.alias = node.Arguments[2].UnwrapLambda().Parameters[0].Name;
                        arguments[0] = Visit(node.Arguments[0]);
                        this.alias = node.Arguments[2].UnwrapLambda().Parameters[1].Name;
                        arguments[1] = Visit(node.Arguments[1]);
                        this.alias = null;
                        arguments[2] = Visit(node.Arguments[2]);
                        var result = node.Update(Visit(node.Object), arguments);
                        this.alias = alias;
                        return result;
                    }

                    case nameof(Queryable.Cast):
                    case nameof(Queryable.Contains):
                    case nameof(Queryable.DefaultIfEmpty):
                    case nameof(Queryable.Distinct):
                    case nameof(Queryable.ElementAt):
                    case nameof(Queryable.ElementAtOrDefault):
                    case nameof(Queryable.GroupBy):
                    case nameof(Queryable.OfType):
                    case nameof(Queryable.Reverse):
                    case nameof(Queryable.SequenceEqual):
                    case nameof(Queryable.Skip):
                    case nameof(Queryable.Take):
                    default:
                    {
                        var arguments = new Expression[node.Arguments.Count];
                        arguments[0] = Visit(node.Arguments[0]);
                        var alias = this.alias;
                        this.alias = null;
                        for (var i = 1; i < node.Arguments.Count; i++)
                        {
                            arguments[i] = Visit(node.Arguments[i]);
                        }
                        var result = node.Update(Visit(node.Object), arguments);
                        this.alias = alias;
                        return result;
                    }
                }
            }
            else
            {
                var alias = this.alias;
                this.alias = null;
                var result = base.VisitMethodCall(node);
                this.alias = alias;
                return result;
            }
        }
    }
}
