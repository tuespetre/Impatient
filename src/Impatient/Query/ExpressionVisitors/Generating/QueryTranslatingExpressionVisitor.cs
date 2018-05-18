using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Generating
{
    public class QueryTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly ITypeMappingProvider typeMappingProvider;
        private readonly IComplexTypeSubqueryFormatter complexTypeSubqueryFormatter;
        private readonly HashSet<string> tableAliases = new HashSet<string>();
        private readonly IDictionary<AliasedTableExpression, string> aliasLookup = new Dictionary<AliasedTableExpression, string>();

        public QueryTranslatingExpressionVisitor(
            IDbCommandExpressionBuilder dbCommandExpressionBuilder,
            ITypeMappingProvider typeMappingProvider,
            IComplexTypeSubqueryFormatter complexTypeSubqueryFormatter)
        {
            Builder = dbCommandExpressionBuilder;
            this.typeMappingProvider = typeMappingProvider;
            this.complexTypeSubqueryFormatter = complexTypeSubqueryFormatter;
        }

        protected IDbCommandExpressionBuilder Builder { get; }

        public LambdaExpression Translate(SelectExpression selectExpression)
        {
            Visit(selectExpression);

            return Builder.Build();
        }

        #region Logical overrides

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case BinaryExpression binaryExpression:
                {
                    return VisitBinary(binaryExpression);
                }

                case ConditionalExpression conditionalExpression:
                {
                    return VisitConditional(conditionalExpression);
                }

                case ConstantExpression constantExpression:
                {
                    return VisitConstant(constantExpression);
                }

                case UnaryExpression unaryExpression:
                {
                    return VisitUnary(unaryExpression);
                }

                default:
                {
                    if (node.NodeType == ExpressionType.Extension)
                    {
                        return VisitExtension(node);
                    }

                    throw new NotSupportedException();
                }
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression VisitSimple(string @operator)
            {
                var left = VisitBinaryOperand(node.Left);

                Builder.Append(@operator);

                var right = VisitBinaryOperand(node.Right);

                return node.UpdateWithConversion(left, right);
            }

            switch (node.NodeType)
            {
                case ExpressionType.Coalesce:
                {
                    Builder.Append("COALESCE(");

                    var left = Visit(node.Left);

                    Builder.Append(", ");

                    var right = Visit(node.Right);

                    Builder.Append(")");

                    return node.UpdateWithConversion(left, right);
                }

                case ExpressionType.AndAlso:
                case ExpressionType.And when node.Type.IsBooleanType():
                {
                    var left = VisitBinaryOperand(node.Left.AsLogicalBooleanSqlExpression());

                    Builder.Append(" AND ");

                    var right = VisitBinaryOperand(node.Right.AsLogicalBooleanSqlExpression());

                    return node.UpdateWithConversion(left, right);
                }

                case ExpressionType.OrElse:
                case ExpressionType.Or when node.Type.IsBooleanType():
                {
                    var left = VisitBinaryOperand(node.Left.AsLogicalBooleanSqlExpression());

                    Builder.Append(" OR ");

                    var right = VisitBinaryOperand(node.Right.AsLogicalBooleanSqlExpression());

                    return node.UpdateWithConversion(left, right);
                }

                case ExpressionType.Equal:
                {
                    var left = node.Left;
                    var right = node.Right;

                    if (FlattenExpressionLists(left, right, out var leftExpressions, out var rightExpressions))
                    {
                        return Visit(Enumerable
                            .Zip(leftExpressions, rightExpressions, Expression.Equal)
                            .Aggregate(Expression.AndAlso)
                            .Balance());
                    }
                    else if (left.IsNullConstant())
                    {
                        right = Visit(right);

                        Builder.Append(" IS NULL");

                        return node.UpdateWithConversion(left, right);
                    }
                    else if (right.IsNullConstant())
                    {
                        left = Visit(left);

                        Builder.Append(" IS NULL");

                        return node.UpdateWithConversion(left, right);
                    }

                    left = node.Left.Type.IsBooleanType() ? node.Left.AsBooleanValuedSqlExpression() : left;
                    right = node.Right.Type.IsBooleanType() ? node.Right.AsBooleanValuedSqlExpression() : right;

                    var leftIsNullable = IsNullableOperand(left);
                    var rightIsNullable = IsNullableOperand(right);

                    if (leftIsNullable && rightIsNullable)
                    {
                        Builder.Append("((");
                        Visit(left);
                        Builder.Append(" IS NULL AND ");
                        Visit(right);
                        Builder.Append(" IS NULL) OR (");
                        left = Visit(left);
                        Builder.Append(" = ");
                        right = Visit(right);
                        Builder.Append("))");

                        return node.UpdateWithConversion(left, right);
                    }

                    left = VisitBinaryOperand(left);

                    Builder.Append(" = ");

                    right = VisitBinaryOperand(right);

                    return node.UpdateWithConversion(left, right);
                }

                case ExpressionType.NotEqual:
                {
                    var left = node.Left;
                    var right = node.Right;

                    if (FlattenExpressionLists(left, right, out var leftExpressions, out var rightExpressions))
                    {
                        return Visit(Enumerable
                            .Zip(leftExpressions, rightExpressions, Expression.NotEqual)
                            .Aggregate(Expression.OrElse)
                            .Balance());
                    }
                    else if (left.IsNullConstant())
                    {
                        right = Visit(right);

                        Builder.Append(" IS NOT NULL");

                        return node.UpdateWithConversion(left, right);
                    }
                    else if (right.IsNullConstant())
                    {
                        left = Visit(left);

                        Builder.Append(" IS NOT NULL");

                        return node.UpdateWithConversion(left, right);
                    }

                    left = node.Left.Type.IsBooleanType() ? node.Left.AsBooleanValuedSqlExpression() : left;
                    right = node.Right.Type.IsBooleanType() ? node.Right.AsBooleanValuedSqlExpression() : right;

                    var leftIsNullable = IsNullableOperand(left);
                    var rightIsNullable = IsNullableOperand(right);

                    if (leftIsNullable && rightIsNullable)
                    {
                        Builder.Append("((");
                        Visit(left);
                        Builder.Append(" IS NULL AND ");
                        Visit(right);
                        Builder.Append(" IS NOT NULL) OR (");
                        Visit(left);
                        Builder.Append(" IS NOT NULL AND ");
                        Visit(right);
                        Builder.Append(" IS NULL) OR (");
                        left = Visit(left);
                        Builder.Append(" <> ");
                        right = Visit(right);
                        Builder.Append("))");

                        return node.UpdateWithConversion(left, right);
                    }
                    else if (leftIsNullable)
                    {
                        Builder.Append("(");
                        Visit(left);
                        Builder.Append(" IS NULL OR (");
                        left = Visit(left);
                        Builder.Append(" <> ");
                        right = Visit(right);
                        Builder.Append("))");

                        return node.UpdateWithConversion(left, right);
                    }
                    else if (rightIsNullable)
                    {
                        Builder.Append("(");
                        Visit(right);
                        Builder.Append(" IS NULL OR (");
                        left = Visit(left);
                        Builder.Append(" <> ");
                        right = Visit(right);
                        Builder.Append("))");

                        return node.UpdateWithConversion(left, right);
                    }

                    left = VisitBinaryOperand(left);

                    Builder.Append(" <> ");

                    right = VisitBinaryOperand(right);

                    return node.UpdateWithConversion(left, right);
                }

                case ExpressionType.GreaterThan:
                {
                    return VisitSimple(" > ");
                }

                case ExpressionType.GreaterThanOrEqual:
                {
                    return VisitSimple(" >= ");
                }

                case ExpressionType.LessThan:
                {
                    return VisitSimple(" < ");
                }

                case ExpressionType.LessThanOrEqual:
                {
                    return VisitSimple(" <= ");
                }

                case ExpressionType.Add:
                {
                    return VisitSimple(" + ");
                }

                case ExpressionType.Subtract:
                {
                    return VisitSimple(" - ");
                }

                case ExpressionType.Multiply:
                {
                    return VisitSimple(" * ");
                }

                case ExpressionType.Divide:
                {
                    return VisitSimple(" / ");
                }

                case ExpressionType.Modulo:
                {
                    return VisitSimple(" % ");
                }

                case ExpressionType.And:
                {
                    return VisitSimple(" & ");
                }

                case ExpressionType.Or:
                {
                    return VisitSimple(" | ");
                }

                case ExpressionType.ExclusiveOr:
                {
                    return VisitSimple(" ^ ");
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Builder.Append("(CASE WHEN ");

            var test = Visit(node.Test.AsLogicalBooleanSqlExpression());

            Builder.Append(" THEN ");

            var ifTrue
                = Visit(node.IfTrue.Type.IsBooleanType()
                    ? node.IfTrue.AsBooleanValuedSqlExpression()
                    : node.IfTrue);

            if (ifTrue.Type != node.IfTrue.Type)
            {
                ifTrue = Expression.Convert(ifTrue, node.IfTrue.Type);
            }

            Builder.Append(" ELSE ");

            var ifFalse
                = Visit(node.IfFalse.Type.IsBooleanType()
                    ? node.IfFalse.AsBooleanValuedSqlExpression()
                    : node.IfFalse);

            if (ifFalse.Type != node.IfTrue.Type)
            {
                ifFalse = Expression.Convert(ifFalse, node.IfTrue.Type);
            }

            Builder.Append(" END)");

            return node.Update(test, ifTrue, ifFalse);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            switch (node.Value)
            {
                case string value:
                {
                    Builder.Append($@"N'{value.Replace("'", "''")}'");

                    return node;
                }

                case char value:
                {
                    Builder.Append($@"N'{value}'");

                    return node;
                }

                case bool value:
                {
                    Builder.Append(value ? "1" : "0");

                    return node;
                }

                case double value:
                {
                    Builder.Append(value.ToString("G17"));

                    return node;
                }

                case decimal value:
                {
                    Builder.Append(value.ToString("0.0###########################"));

                    return node;
                }

                case DateTime value:
                {
                    Builder.Append($"'{value.ToString("yyyy-MM-ddTHH:mm:ss.fffK")}'");

                    return node;
                }

                case DateTimeOffset value:
                {
                    Builder.Append($"'{value}'");

                    return node;
                }

                case TimeSpan value:
                {
                    Builder.Append($"'{value}'");

                    return node;
                }

                case Enum value:
                {
                    // TODO: This is probably not correct

                    Builder.Append(Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType())).ToString());

                    return node;
                }

                case byte[] value:
                {
                    Builder.Append("0x");

                    foreach (var @byte in value)
                    {
                        Builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
                    }

                    return node;
                }

                case object value:
                {
                    Builder.Append(value.ToString());

                    return node;
                }

                case null:
                {
                    Builder.Append("NULL");

                    return node;
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression selectExpression:
                {
                    return VisitSelectExpression(selectExpression);
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
                {
                    return VisitSingleValueRelationalQueryExpression(singleValueRelationalQueryExpression);
                }

                case EnumerableRelationalQueryExpression enumerableRelationalQueryExpression:
                {
                    return VisitEnumerableRelationalQueryExpression(enumerableRelationalQueryExpression);
                }

                case BaseTableExpression baseTableExpression:
                {
                    return VisitBaseTableExpression(baseTableExpression);
                }

                case SubqueryTableExpression subqueryTableExpression:
                {
                    return VisitSubqueryTableExpression(subqueryTableExpression);
                }

                case InnerJoinExpression innerJoinExpression:
                {
                    return VisitInnerJoinExpression(innerJoinExpression);
                }

                case LeftJoinExpression leftJoinExpression:
                {
                    return VisitLeftJoinExpression(leftJoinExpression);
                }

                case FullJoinExpression fullJoinExpression:
                {
                    return VisitFullJoinExpression(fullJoinExpression);
                }

                case CrossJoinExpression crossJoinExpression:
                {
                    return VisitCrossJoinExpression(crossJoinExpression);
                }

                case CrossApplyExpression crossApplyExpression:
                {
                    return VisitCrossApplyExpression(crossApplyExpression);
                }

                case OuterApplyExpression outerApplyExpression:
                {
                    return VisitOuterApplyExpression(outerApplyExpression);
                }

                case SetOperatorExpression setOperatorExpression:
                {
                    return VisitSetOperatorExpression(setOperatorExpression);
                }

                case TableValuedExpressionTableExpression tableValuedExpressionTableExpression:
                {
                    return VisitTableValuedExpressionTableExpression(tableValuedExpressionTableExpression);
                }

                case SqlAggregateExpression sqlAggregateExpression:
                {
                    return VisitSqlAggregateExpression(sqlAggregateExpression);
                }

                case SqlAliasExpression sqlAliasExpression:
                {
                    return VisitSqlAliasExpression(sqlAliasExpression);
                }

                case SqlCastExpression sqlCastExpression:
                {
                    return VisitSqlCastExpression(sqlCastExpression);
                }

                case SqlColumnExpression sqlColumnExpression:
                {
                    return VisitSqlColumnExpression(sqlColumnExpression);
                }

                case SqlConcatExpression sqlConcatExpression:
                {
                    return VisitSqlConcatExpression(sqlConcatExpression);
                }

                case SqlExistsExpression sqlExistsExpression:
                {
                    return VisitSqlExistsExpression(sqlExistsExpression);
                }

                case SqlFragmentExpression sqlFragmentExpression:
                {
                    return VisitSqlFragmentExpression(sqlFragmentExpression);
                }

                case SqlFunctionExpression sqlFunctionExpression:
                {
                    return VisitSqlFunctionExpression(sqlFunctionExpression);
                }

                case SqlInExpression sqlInExpression:
                {
                    return VisitSqlInExpression(sqlInExpression);
                }

                case SqlParameterExpression sqlParameterExpression:
                {
                    return VisitSqlParameterExpression(sqlParameterExpression);
                }

                case SqlWindowFunctionExpression sqlWindowFunctionExpression:
                {
                    return VisitSqlWindowFunctionExpression(sqlWindowFunctionExpression);
                }

                case OrderByExpression orderByExpression:
                {
                    return VisitOrderByExpression(orderByExpression);
                }

                default:
                {
                    return base.VisitExtension(node);
                }
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not
                when node.Type.IsBooleanType():
                {
                    switch (node.Operand)
                    {
                        case SqlExistsExpression sqlExistsExpression:
                        case SqlInExpression sqlInExpression:
                        {
                            Builder.Append("NOT ");

                            return base.VisitUnary(node);
                        }

                        default:
                        {
                            if (node.Operand.Type.IsNullableType())
                            {
                                return Visit(
                                    Expression.Condition(
                                        Expression.Equal(
                                            Expression.Constant(null),
                                            node.Operand),
                                        Expression.Constant(null),
                                        node.Operand));
                            }

                            return Visit(Expression.Equal(Expression.Constant(false), node.Operand));
                        }
                    }
                }

                case ExpressionType.Not:
                {
                    Builder.Append("~ ");

                    return base.VisitUnary(node);
                }

                case ExpressionType.Convert:
                {
                    if (node.Operand.Type.UnwrapNullableType() != node.Type.UnwrapNullableType())
                    {
                        var mapping = typeMappingProvider.FindMapping(node.Type);

                        if (mapping != null)
                        {
                            Builder.Append("CAST(");

                            var visited = Visit(node.Operand);

                            Builder.Append($" AS {mapping.DbTypeName})");

                            return node.Update(visited);
                        }
                    }

                    return base.VisitUnary(node);
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        #endregion

        #region Extension Expression visiting methods

        protected virtual Expression VisitSelectExpression(SelectExpression selectExpression)
        {
            Builder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                Builder.Append("DISTINCT ");
            }

            if (selectExpression.Limit != null && selectExpression.Offset == null)
            {
                Builder.Append("TOP (");

                Visit(selectExpression.Limit);

                Builder.Append(") ");
            }

            var projectionExpressions
                = FlattenProjection(selectExpression.Projection)
                    .Select((e, i) => (i, e.alias, e.expression));

            foreach (var (index, alias, expression) in projectionExpressions.DefaultIfEmpty((0, null, Expression.Constant(0))))
            {
                if (index > 0)
                {
                    Builder.Append(", ");
                }

                if (expression.Type.IsBooleanType()
                    && !(expression is SqlAliasExpression
                       || expression is SqlColumnExpression
                       || expression is SqlCastExpression))
                {
                    Builder.Append("CAST(");

                    EmitExpressionListExpression(expression);

                    var booleanMapping = typeMappingProvider.FindMapping(typeof(bool));

                    Builder.Append($" AS {booleanMapping.DbTypeName})");
                }
                else
                {
                    EmitExpressionListExpression(expression);
                }

                if (!string.IsNullOrEmpty(alias))
                {
                    Builder.Append(" AS ");
                    Builder.Append(FormatIdentifier(alias));
                }
            }

            if (selectExpression.Table != null)
            {
                Builder.AppendLine();
                Builder.Append("FROM ");

                Visit(selectExpression.Table);
            }

            if (selectExpression.Predicate != null)
            {
                Builder.AppendLine();
                Builder.Append("WHERE ");

                Visit(selectExpression.Predicate.AsLogicalBooleanSqlExpression());
            }

            if (selectExpression.Grouping != null)
            {
                Builder.AppendLine();
                Builder.Append("GROUP BY ");

                var groupings = FlattenProjection(selectExpression.Grouping);

                EmitExpressionListExpression(groupings.First().expression);

                foreach (var (alias, expression) in groupings.Skip(1))
                {
                    Builder.Append(", ");

                    EmitExpressionListExpression(expression);
                }
            }

            if (selectExpression.OrderBy != null)
            {
                Builder.AppendLine();
                Builder.Append("ORDER BY ");

                Visit(selectExpression.OrderBy);
            }

            if (selectExpression.Offset != null)
            {
                Builder.AppendLine();
                Builder.Append("OFFSET ");

                Visit(selectExpression.Offset);

                Builder.Append(" ROWS");

                if (selectExpression.Limit != null)
                {
                    Builder.Append(" FETCH NEXT ");

                    Visit(selectExpression.Limit);

                    Builder.Append(" ROWS ONLY");
                }
            }

            return selectExpression;
        }

        protected virtual Expression VisitSingleValueRelationalQueryExpression(SingleValueRelationalQueryExpression singleValueRelationalQueryExpression)
        {
            if (!singleValueRelationalQueryExpression.Type.IsScalarType())
            {
                complexTypeSubqueryFormatter.Format(
                    singleValueRelationalQueryExpression.SelectExpression,
                    Builder,
                    this);
            }
            else if (singleValueRelationalQueryExpression == SingleValueRelationalQueryExpression.SelectOne)
            {
                Builder.Append("(SELECT 1)");
            }
            else
            {
                Builder.Append("(");

                Builder.IncreaseIndent();
                Builder.AppendLine();

                Visit(singleValueRelationalQueryExpression.SelectExpression);

                Builder.DecreaseIndent();
                Builder.AppendLine();

                Builder.Append(")");
            }

            return singleValueRelationalQueryExpression;
        }

        protected virtual Expression VisitEnumerableRelationalQueryExpression(EnumerableRelationalQueryExpression enumerableRelationalQueryExpression)
        {
            complexTypeSubqueryFormatter.Format(
                enumerableRelationalQueryExpression.SelectExpression,
                Builder,
                this);

            return enumerableRelationalQueryExpression;
        }

        protected virtual Expression VisitBaseTableExpression(BaseTableExpression baseTableExpression)
        {
            if (!string.IsNullOrEmpty(baseTableExpression.SchemaName))
            {
                Builder.Append(FormatIdentifier(baseTableExpression.SchemaName));
                Builder.Append(".");
            }

            Builder.Append(FormatIdentifier(baseTableExpression.TableName));
            Builder.Append(" AS ");
            Builder.Append(FormatIdentifier(GetTableAlias(baseTableExpression)));

            return baseTableExpression;
        }

        protected virtual Expression VisitSubqueryTableExpression(SubqueryTableExpression subqueryTableExpression)
        {
            Builder.Append("(");

            Builder.IncreaseIndent();
            Builder.AppendLine();

            Visit(subqueryTableExpression.Subquery);

            Builder.DecreaseIndent();
            Builder.AppendLine();

            Builder.Append(") AS ");
            Builder.Append(FormatIdentifier(GetTableAlias(subqueryTableExpression)));

            return subqueryTableExpression;
        }

        protected virtual Expression VisitInnerJoinExpression(InnerJoinExpression innerJoinExpression)
        {
            Visit(innerJoinExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("INNER JOIN ");

            Visit(innerJoinExpression.InnerTable);

            Builder.Append(" ON ");

            Visit(innerJoinExpression.Predicate.AsLogicalBooleanSqlExpression());

            return innerJoinExpression;
        }

        protected virtual Expression VisitLeftJoinExpression(LeftJoinExpression leftJoinExpression)
        {
            Visit(leftJoinExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("LEFT JOIN ");

            Visit(leftJoinExpression.InnerTable);

            Builder.Append(" ON ");

            Visit(leftJoinExpression.Predicate.AsLogicalBooleanSqlExpression());

            return leftJoinExpression;
        }

        protected virtual Expression VisitFullJoinExpression(FullJoinExpression fullJoinExpression)
        {
            Visit(fullJoinExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("FULL JOIN ");

            Visit(fullJoinExpression.InnerTable);

            Builder.Append(" ON ");

            Visit(fullJoinExpression.Predicate.AsLogicalBooleanSqlExpression());

            return fullJoinExpression;
        }

        protected virtual Expression VisitCrossJoinExpression(CrossJoinExpression crossJoinExpression)
        {
            Visit(crossJoinExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("CROSS JOIN ");

            Visit(crossJoinExpression.InnerTable);

            return crossJoinExpression;
        }

        protected virtual Expression VisitCrossApplyExpression(CrossApplyExpression crossApplyExpression)
        {
            Visit(crossApplyExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("CROSS APPLY ");

            Visit(crossApplyExpression.InnerTable);

            return crossApplyExpression;
        }

        protected virtual Expression VisitOuterApplyExpression(OuterApplyExpression outerApplyExpression)
        {
            Visit(outerApplyExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("OUTER APPLY ");

            Visit(outerApplyExpression.InnerTable);

            return outerApplyExpression;
        }

        protected virtual Expression VisitSetOperatorExpression(SetOperatorExpression setOperatorExpression)
        {
            Builder.Append("(");

            Builder.IncreaseIndent();
            Builder.AppendLine();

            Visit(setOperatorExpression.Set1);

            Builder.AppendLine();

            switch (setOperatorExpression)
            {
                case ExceptExpression exceptExpression:
                {
                    Builder.Append("EXCEPT");
                    break;
                }

                case IntersectExpression intersectExpression:
                {
                    Builder.Append("INTERSECT");
                    break;
                }

                case UnionAllExpression unionAllExpression:
                {
                    Builder.Append("UNION ALL");
                    break;
                }

                case UnionExpression unionExpression:
                {
                    Builder.Append("UNION");
                    break;
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }

            Builder.AppendLine();

            Visit(setOperatorExpression.Set2);

            Builder.DecreaseIndent();
            Builder.AppendLine();

            Builder.Append(") AS ");
            Builder.Append(FormatIdentifier(GetTableAlias(setOperatorExpression)));

            return setOperatorExpression;
        }

        protected virtual Expression VisitTableValuedExpressionTableExpression(TableValuedExpressionTableExpression tableValuedExpressionTableExpression)
        {
            Visit(tableValuedExpressionTableExpression.Expression);

            Builder.Append(" AS ");
            Builder.Append(FormatIdentifier(GetTableAlias(tableValuedExpressionTableExpression)));

            return tableValuedExpressionTableExpression;
        }

        protected virtual Expression VisitSqlAggregateExpression(SqlAggregateExpression sqlAggregateExpression)
        {
            var aggregatedExpression = sqlAggregateExpression.Expression;

            switch (aggregatedExpression)
            {
                case ConstantExpression constantExpression:
                {
                    aggregatedExpression = new SqlCastExpression(constantExpression, constantExpression.Type);
                    break;
                }
            }

            Builder.Append(sqlAggregateExpression.FunctionName);
            Builder.Append("(");

            if (sqlAggregateExpression.IsDistinct)
            {
                Builder.Append("DISTINCT ");
            }

            Visit(aggregatedExpression);

            Builder.Append(")");

            return sqlAggregateExpression;
        }

        protected virtual Expression VisitSqlAliasExpression(SqlAliasExpression sqlAliasExpression)
        {
            Visit(sqlAliasExpression.Expression);

            Builder.Append(" AS ");
            Builder.Append(FormatIdentifier(sqlAliasExpression.Alias));

            return sqlAliasExpression;
        }

        protected virtual Expression VisitSqlCastExpression(SqlCastExpression sqlCastExpression)
        {
            if (!string.IsNullOrWhiteSpace(sqlCastExpression.SqlType))
            {
                Builder.Append("CAST(");

                Visit(sqlCastExpression.Expression);

                Builder.Append($" AS {sqlCastExpression.SqlType})");
            }
            else
            {
                var mapping = typeMappingProvider.FindMapping(sqlCastExpression.Type);

                if (mapping != null)
                {
                    Builder.Append("CAST(");

                    Visit(sqlCastExpression.Expression);

                    Builder.Append($" AS {mapping.DbTypeName})");
                }
                else
                {
                    Visit(sqlCastExpression.Expression);
                }
            }

            return sqlCastExpression;
        }

        protected virtual Expression VisitSqlColumnExpression(SqlColumnExpression sqlColumnExpression)
        {
            Builder.Append(FormatIdentifier(GetTableAlias(sqlColumnExpression.Table)));
            Builder.Append(".");
            Builder.Append(FormatIdentifier(sqlColumnExpression.ColumnName));

            return sqlColumnExpression;
        }

        protected virtual Expression VisitSqlConcatExpression(SqlConcatExpression sqlConcatExpression)
        {
            Visit(sqlConcatExpression.Segments.First());

            foreach (var segment in sqlConcatExpression.Segments.Skip(1))
            {
                Builder.Append(" + ");

                Visit(segment);
            }

            return sqlConcatExpression;
        }

        protected virtual Expression VisitSqlExistsExpression(SqlExistsExpression sqlExistsExpression)
        {
            Builder.Append("EXISTS (");

            Builder.IncreaseIndent();
            Builder.AppendLine();

            Visit(sqlExistsExpression.SelectExpression);

            Builder.DecreaseIndent();
            Builder.AppendLine();

            Builder.Append(")");

            return sqlExistsExpression;
        }

        protected virtual Expression VisitSqlFragmentExpression(SqlFragmentExpression sqlFragmentExpression)
        {
            Builder.Append(sqlFragmentExpression.Fragment);

            return sqlFragmentExpression;
        }

        protected virtual Expression VisitSqlFunctionExpression(SqlFunctionExpression sqlFunctionExpression)
        {
            Builder.Append(sqlFunctionExpression.FunctionName);
            Builder.Append("(");

            if (sqlFunctionExpression.Arguments.Any())
            {
                Visit(sqlFunctionExpression.Arguments.First());

                foreach (var argument in sqlFunctionExpression.Arguments.Skip(1))
                {
                    Builder.Append(", ");

                    Visit(argument);
                }
            }

            Builder.Append(")");

            return sqlFunctionExpression;
        }

        protected virtual Expression VisitSqlInExpression(SqlInExpression sqlInExpression)
        {
            switch (sqlInExpression.Values)
            {
                case RelationalQueryExpression relationalQueryExpression:
                {
                    Visit(sqlInExpression.Value);

                    Builder.Append(" IN (");

                    Builder.IncreaseIndent();
                    Builder.AppendLine();

                    Visit(relationalQueryExpression.SelectExpression);

                    Builder.DecreaseIndent();
                    Builder.AppendLine();

                    Builder.Append(")");

                    break;
                }

                case SelectExpression selectExpression:
                {
                    Visit(sqlInExpression.Value);

                    Builder.Append(" IN (");

                    Builder.IncreaseIndent();
                    Builder.AppendLine();

                    Visit(selectExpression);

                    Builder.DecreaseIndent();
                    Builder.AppendLine();

                    Builder.Append(")");

                    break;
                }

                // TODO: Check the following three cases for if they contain parameters

                case NewArrayExpression newArrayExpression:
                {
                    HandleSqlInExpression(
                        sqlInExpression.Value,
                        newArrayExpression.Expressions);

                    break;
                }

                case ListInitExpression listInitExpression:
                {
                    HandleSqlInExpression(
                        sqlInExpression.Value,
                        listInitExpression.Initializers.Select(i => i.Arguments[0]));

                    break;
                }

                case ConstantExpression constantExpression:
                {
                    HandleSqlInExpression(
                        sqlInExpression.Value,
                        from object value in ((IEnumerable)constantExpression.Value)
                        select Expression.Constant(value));

                    break;
                }

                case Expression expression:
                {
                    Builder.StartCapture();

                    Visit(sqlInExpression.Value);

                    var value = Builder.StopCapture();

                    Builder.AddDynamicParameters(value, expression, FormatParameterName);

                    break;
                }

                default:
                {
                    Builder.Append(" IN (SELECT 1 WHERE 1 = 0)");

                    break;
                }
            }

            return sqlInExpression;
        }

        protected virtual Expression VisitSqlParameterExpression(SqlParameterExpression sqlParameterExpression)
        {
            Builder.AddParameter(sqlParameterExpression, FormatParameterName);

            return sqlParameterExpression;
        }

        protected virtual Expression VisitSqlWindowFunctionExpression(SqlWindowFunctionExpression sqlWindowFunctionExpression)
        {
            Visit(sqlWindowFunctionExpression.Function);

            if (sqlWindowFunctionExpression.Ordering != null)
            {
                Builder.Append(" OVER(ORDER BY ");

                Visit(sqlWindowFunctionExpression.Ordering);

                Builder.Append(")");
            }

            return sqlWindowFunctionExpression;
        }

        protected virtual Expression VisitOrderByExpression(OrderByExpression orderByExpression)
        {
            var orderings = orderByExpression.Iterate().Reverse().ToArray();
            var hashes = new HashSet<int>();

            for (var i = 0; i < orderings.Length; i++)
            {
                if (i > 0)
                {
                    Builder.Append(", ");
                }

                var ordering = orderings[i];

                var detector = new SqlParameterDetectingExpressionVisitor();

                detector.Visit(ordering.Expression);

                var needsWrapping = detector.ParameterDetected;

                if (!needsWrapping)
                {
                    var hash = ExpressionEqualityComparer.Instance.GetHashCode(ordering.Expression);

                    needsWrapping = !hashes.Add(hash) && !(ordering.Expression is RelationalQueryExpression);
                }

                if (needsWrapping)
                {
                    var wrapped
                        = new SingleValueRelationalQueryExpression(
                            new SelectExpression(
                                new ServerProjectionExpression(
                                    ordering.Expression)));

                    EmitExpressionListExpression(wrapped);
                }
                else
                {
                    EmitExpressionListExpression(ordering.Expression);
                }

                Builder.Append(" ");
                Builder.Append(ordering.Descending ? "DESC" : "ASC");
            }

            return orderByExpression;
        }

        #endregion

        #region Extensibility points

        protected virtual void EmitExpressionListExpression(Expression expression)
        {
            if (expression.Type.IsBooleanType())
            {
                Visit(expression.AsBooleanValuedSqlExpression());

                return;
            }

            if (RequiresNumericConversion(expression))
            {
                var mapping = typeMappingProvider.FindMapping(expression.Type);

                if (mapping != null)
                {
                    Builder.Append("CAST(");

                    Visit(expression);

                    Builder.Append($" AS {mapping.DbTypeName})");

                    return;
                }
            }

            Visit(expression);
        }

        protected virtual string FormatIdentifier(string identifier)
        {
            return $"[{identifier}]";
        }

        protected virtual string FormatParameterName(string name)
        {
            return $"@{name}";
        }

        #endregion

        private void HandleSqlInExpression(Expression valueExpression, IEnumerable<Expression> values)
        {
            var valueList = new List<Expression>();
            var foundNull = false;

            foreach (var value in values)
            {
                if (value.IsNullConstant())
                {
                    foundNull = true;
                    continue;
                }

                valueList.Add(value);
            }

            if (valueList.Count == 0)
            {
                if (!foundNull)
                {
                    Visit(valueExpression);
                    Builder.Append(" IN (SELECT 1 WHERE 1 = 0)");
                    return;
                }
                else
                {
                    Visit(valueExpression);
                    Builder.Append(" IS NULL");
                    return;
                }
            }
            else
            {
                if (foundNull)
                {
                    Builder.Append("(");
                    Visit(valueExpression);
                    Builder.Append(" IS NULL OR ");
                }

                Visit(valueExpression);
                Builder.Append(" IN (");
                Visit(valueList[0]);

                for (var i = 1; i < valueList.Count; i++)
                {
                    Builder.Append(", ");
                    Visit(valueList[i]);
                }

                Builder.Append(")");

                if (foundNull)
                {
                    Builder.Append(")");
                }
            }
        }

        private Expression VisitBinaryOperand(Expression operand)
        {
            switch (operand)
            {
                case BinaryExpression binaryExpression:
                {
                    Builder.Append("(");

                    operand = VisitBinary(binaryExpression);

                    Builder.Append(")");

                    break;
                }

                default:
                {
                    operand = Visit(operand);

                    break;
                }
            }

            return operand;
        }

        private static bool FlattenExpressionLists(
            Expression left,
            Expression right,
            out IEnumerable<Expression> leftExpressions,
            out IEnumerable<Expression> rightExpressions)
        {
            leftExpressions = default;
            rightExpressions = default;

            switch (left)
            {
                case NewExpression leftNewExpression
                when right is NewExpression rightNewExpression:
                {
                    leftExpressions = leftNewExpression.Arguments;
                    rightExpressions = rightNewExpression.Arguments;

                    return true;
                }

                case MemberInitExpression leftMemberInitExpression
                when right is MemberInitExpression rightMemberInitExpression:
                {
                    leftExpressions
                        = leftMemberInitExpression.NewExpression.Arguments.Concat(
                            leftMemberInitExpression.Bindings.Iterate()
                                .Cast<MemberAssignment>().Select(m => m.Expression));

                    rightExpressions
                        = rightMemberInitExpression.NewExpression.Arguments.Concat(
                            rightMemberInitExpression.Bindings.Iterate()
                                .Cast<MemberAssignment>().Select(m => m.Expression));

                    return true;
                }

                case ExtendedNewExpression leftNewExpression
                when right is ExtendedNewExpression rightNewExpression:
                {
                    leftExpressions = leftNewExpression.Arguments;
                    rightExpressions = rightNewExpression.Arguments;

                    return true;
                }

                case ExtendedMemberInitExpression leftMemberInitExpression
                when right is ExtendedMemberInitExpression rightMemberInitExpression:
                {
                    leftExpressions
                        = leftMemberInitExpression.NewExpression.Arguments.Concat(
                            leftMemberInitExpression.Arguments);

                    rightExpressions
                        = rightMemberInitExpression.NewExpression.Arguments.Concat(
                            rightMemberInitExpression.Arguments);

                    return true;
                }

                case NewArrayExpression leftNewArrayExpression
                when right is NewArrayExpression rightNewArrayExpression:
                {
                    leftExpressions = leftNewArrayExpression.Expressions;
                    rightExpressions = rightNewArrayExpression.Expressions;

                    return true;
                }
            }

            return false;
        }

        protected string GetTableAlias(AliasedTableExpression table)
        {
            if (!aliasLookup.TryGetValue(table, out var alias))
            {
                alias = table.Alias;

                if (!tableAliases.Add(alias))
                {
                    var i = 0;

                    do
                    {
                        alias = $"{table.Alias}_{i++}";
                    }
                    while (!tableAliases.Add(alias));
                }

                aliasLookup.Add(table, alias);
            }

            return alias;
        }

        protected static IEnumerable<(string alias, Expression expression)> FlattenProjection(Expression expression)
        {
            var visitor = new ProjectionLeafGatheringExpressionVisitor();

            visitor.Visit(expression);

            return visitor.GatheredExpressions.Select(p => (p.Key, p.Value)).ToArray();
        }

        protected static IEnumerable<(string alias, Expression expression)> FlattenProjection(ProjectionExpression projection)
        {
            IEnumerable<Expression> IterateServerProjectionExpressions(ProjectionExpression p)
            {
                switch (p)
                {
                    case ServerProjectionExpression server:
                    {
                        yield return server.ResultLambda.Body;
                        yield break;
                    }

                    case ClientProjectionExpression client:
                    {
                        yield return client.ServerProjection.ResultLambda.Body;
                        yield break;
                    }

                    case CompositeProjectionExpression composite:
                    {
                        foreach (var expression in IterateServerProjectionExpressions(composite.OuterProjection))
                        {
                            yield return expression;
                        }

                        foreach (var expression in IterateServerProjectionExpressions(composite.InnerProjection))
                        {
                            yield return expression;
                        }

                        yield break;
                    }
                }
            }

            var expressions = IterateServerProjectionExpressions(projection).ToArray();

            if (expressions.Length == 1)
            {
                var visitor = new ProjectionLeafGatheringExpressionVisitor();

                visitor.Visit(expressions[0]);

                foreach (var p in visitor.GatheredExpressions)
                {
                    yield return (p.Key, p.Value);
                }
            }
            else
            {
                for (var i = 0; i < expressions.Length; i++)
                {
                    var visitor = new ProjectionLeafGatheringExpressionVisitor();

                    visitor.Visit(expressions[i]);

                    foreach (var p in visitor.GatheredExpressions)
                    {
                        yield return ($"${i}." + p.Key, p.Value);
                    }
                }
            }
        }

        private class SqlParameterDetectingExpressionVisitor : ExpressionVisitor
        {
            public bool ParameterDetected { get; private set; }

            protected override Expression VisitExtension(Expression node)
            {
                switch (node)
                {
                    case SqlParameterExpression _ when !ParameterDetected:
                    case SqlInExpression _ when !ParameterDetected:
                    {
                        ParameterDetected = true;

                        return node;
                    }

                    case RelationalQueryExpression _:
                    {
                        return node;
                    }

                    default:
                    {
                        return base.VisitExtension(node);
                    }
                }
            }
        }

        private static bool IsNullableOperand(Expression node)
        {
            switch (node)
            {
                case AnnotationExpression annotationExpression:
                {
                    return IsNullableOperand(annotationExpression.Expression);
                }

                case UnaryExpression unaryExpression
                when node.NodeType == ExpressionType.Convert
                    || node.NodeType == ExpressionType.Not:
                {
                    return IsNullableOperand(unaryExpression.Operand);
                }

                case BinaryExpression binaryExpression
                when node.NodeType == ExpressionType.Coalesce:
                {
                    return IsNullableOperand(binaryExpression.Left)
                        && IsNullableOperand(binaryExpression.Right);
                }

                case ConditionalExpression conditionalExpression:
                {
                    return (node.Type.IsNullableType() || !node.Type.GetTypeInfo().IsValueType)
                        || IsNullableOperand(conditionalExpression.IfTrue)
                        || IsNullableOperand(conditionalExpression.IfFalse);
                }

                case SqlExpression sqlExpression
                when sqlExpression.IsNullable:
                {
                    return true;
                }

                default:
                {
                    return false;
                }
            }
        }

        private bool RequiresNumericConversion(Expression node)
        {
            var type = node.Type.UnwrapNullableType();

            if (type.IsEnum() && !(node.UnwrapInnerExpression() is SqlColumnExpression))
            {
                var mapping = typeMappingProvider.FindMapping(type);

                if (mapping != null)
                {
                    return mapping.SourceType.IsNumericType();
                }
            }

            if (!type.IsNumericType())
            {
                return false;
            }

            switch (node)
            {
                case SqlAggregateExpression sqlAggregateExpression:
                {
                    return type == typeof(float);
                }

                case BinaryExpression binaryExpression:
                {
                    if (type != typeof(int) && type != typeof(decimal))
                    {
                        return node.NodeType != ExpressionType.Coalesce
                            || RequiresNumericConversion(binaryExpression.Left)
                            || RequiresNumericConversion(binaryExpression.Right);
                    }

                    return false;
                }

                case ConstantExpression constantExpression:
                {
                    return type != typeof(int) && type != typeof(decimal);
                }

                case ConditionalExpression conditionalExpression:
                {
                    return RequiresNumericConversion(conditionalExpression.IfTrue)
                        || RequiresNumericConversion(conditionalExpression.IfFalse);
                }

                case UnaryExpression unaryExpression
                when node.NodeType == ExpressionType.Convert:
                {
                    return RequiresNumericConversion(unaryExpression.Operand);
                }

                default:
                {
                    return false;
                }
            }
        }

        private static ITypeMapping FindTypeMapping(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression
                when sqlColumnExpression.TypeMapping != null:
                {
                    return sqlColumnExpression.TypeMapping;
                }

                case SqlParameterExpression sqlParameterExpression
                when sqlParameterExpression.TypeMapping != null:
                {
                    return sqlParameterExpression.TypeMapping;
                }

                case SqlAggregateExpression sqlAggregateExpression:
                {
                    return FindTypeMapping(sqlAggregateExpression.Expression);
                }

                default:
                {
                    return null;
                }
            }
        }
    }
}
