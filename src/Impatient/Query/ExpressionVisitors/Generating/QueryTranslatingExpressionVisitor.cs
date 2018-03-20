using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Generating
{
    public class QueryTranslatingExpressionVisitor : ExpressionVisitor
    {
        private static readonly IComplexTypeSubqueryFormatter complexTypeSubqueryFormatter
            = new SqlServerForJsonComplexTypeSubqueryFormatter();

        private readonly HashSet<string> tableAliases = new HashSet<string>();
        private readonly IDictionary<AliasedTableExpression, string> aliasLookup = new Dictionary<AliasedTableExpression, string>();

        public QueryTranslatingExpressionVisitor(IImpatientExpressionVisitorProvider expressionVisitorProvider)
        {
            ExpressionVisitorProvider = expressionVisitorProvider ?? throw new ArgumentNullException(nameof(expressionVisitorProvider));
            Builder = new DefaultDbCommandExpressionBuilder();
        }

        protected IImpatientExpressionVisitorProvider ExpressionVisitorProvider { get; }

        protected IDbCommandExpressionBuilder Builder { get; }

        public LambdaExpression Translate(SelectExpression selectExpression)
        {
            Visit(selectExpression);

            return Builder.Build();
        }

        #region ExpressionVisitor overrides

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Expression VisitBinaryOperand(Expression operand)
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

            Expression VisitSimple(string @operator)
            {
                var left = VisitBinaryOperand(node.Left);

                Builder.Append(@operator);

                var right = VisitBinaryOperand(node.Right);

                return node.Update(left, node.Conversion, right);
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

                    return node.Update(left, node.Conversion, right);
                }

                case ExpressionType.AndAlso:
                {
                    var left = VisitBinaryOperand(node.Left.AsSqlBooleanExpression());

                    Builder.Append(" AND ");

                    var right = VisitBinaryOperand(node.Right.AsSqlBooleanExpression());

                    return node.Update(left, node.Conversion, right);
                }

                case ExpressionType.OrElse:
                {
                    var left = VisitBinaryOperand(node.Left.AsSqlBooleanExpression());

                    Builder.Append(" OR ");

                    var right = VisitBinaryOperand(node.Right.AsSqlBooleanExpression());

                    return node.Update(left, node.Conversion, right);
                }

                case ExpressionType.Equal:
                {
                    if (node.Left is NewExpression leftNewExpression
                        && node.Right is NewExpression rightNewExpression)
                    {
                        return Visit(
                            leftNewExpression.Arguments
                                .Zip(rightNewExpression.Arguments, Expression.Equal)
                                .Aggregate(Expression.AndAlso)
                                .Balance());
                    }
                    else if (node.Left is MemberInitExpression leftMemberInitExpression
                        && node.Right is MemberInitExpression rightMemberInitExpression)
                    {
                        var leftBindings
                            = leftMemberInitExpression.NewExpression.Arguments.Concat(
                                leftMemberInitExpression.Bindings.Iterate()
                                    .Cast<MemberAssignment>().Select(m => m.Expression));

                        var rightBindings
                            = rightMemberInitExpression.NewExpression.Arguments.Concat(
                                rightMemberInitExpression.Bindings.Iterate()
                                    .Cast<MemberAssignment>().Select(m => m.Expression));

                        return Visit(
                            leftBindings
                                .Zip(rightBindings, Expression.Equal)
                                .Aggregate(Expression.AndAlso)
                                .Balance());
                    }
                    else if (node.Left is ConstantExpression leftConstantExpression
                        && leftConstantExpression.Value is null)
                    {
                        var visitedRight = Visit(node.Right);

                        Builder.Append(" IS NULL");

                        return node.Update(node.Left, node.Conversion, visitedRight);
                    }
                    else if (node.Right is ConstantExpression rightConstantExpression
                        && rightConstantExpression.Value is null)
                    {
                        var visitedLeft = Visit(node.Left);

                        Builder.Append(" IS NULL");

                        return node.Update(visitedLeft, node.Conversion, node.Right);
                    }

                    var leftIsNullable
                        = node.Left is SqlExpression leftSqlExpression
                            && leftSqlExpression.IsNullable;

                    var rightIsNullable
                        = node.Right is SqlExpression rightSqlExpression
                            && rightSqlExpression.IsNullable;

                    if (leftIsNullable && rightIsNullable)
                    {
                        Builder.Append("((");
                        Visit(node.Left);
                        Builder.Append(" IS NULL AND ");
                        Visit(node.Right);
                        Builder.Append(" IS NULL) OR (");
                        var visitedLeft = Visit(node.Left);
                        Builder.Append(" = ");
                        var visitedRight = Visit(node.Right);
                        Builder.Append("))");

                        return node.Update(visitedLeft, node.Conversion, visitedRight);
                    }

                    return VisitSimple(" = ");
                }

                case ExpressionType.NotEqual:
                {
                    if (node.Left is NewExpression leftNewExpression
                        && node.Right is NewExpression rightNewExpression)
                    {
                        return Visit(
                            leftNewExpression.Arguments
                                .Zip(rightNewExpression.Arguments, Expression.NotEqual)
                                .Aggregate(Expression.OrElse));
                    }
                    else if (node.Left is MemberInitExpression leftMemberInitExpression
                        && node.Right is MemberInitExpression rightMemberInitExpression)
                    {
                        var leftBindings
                            = leftMemberInitExpression.NewExpression.Arguments.Concat(
                                leftMemberInitExpression.Bindings.Iterate()
                                    .Cast<MemberAssignment>().Select(m => m.Expression));

                        var rightBindings
                            = rightMemberInitExpression.NewExpression.Arguments.Concat(
                                rightMemberInitExpression.Bindings.Iterate()
                                    .Cast<MemberAssignment>().Select(m => m.Expression));

                        return Visit(
                            leftBindings
                                .Zip(rightBindings, Expression.NotEqual)
                                .Aggregate(Expression.OrElse));
                    }
                    else if (node.Left is ConstantExpression leftConstantExpression
                        && leftConstantExpression.Value is null)
                    {
                        var visitedRight = Visit(node.Right);

                        Builder.Append(" IS NOT NULL");

                        return node.Update(node.Left, node.Conversion, visitedRight);
                    }
                    else if (node.Right is ConstantExpression rightConstantExpression
                        && rightConstantExpression.Value is null)
                    {
                        var visitedLeft = Visit(node.Left);

                        Builder.Append(" IS NOT NULL");

                        return node.Update(visitedLeft, node.Conversion, node.Right);
                    }

                    var leftIsNullable
                        = node.Left is SqlExpression leftSqlExpression
                            && leftSqlExpression.IsNullable;

                    var rightIsNullable
                        = node.Right is SqlExpression rightSqlExpression
                            && rightSqlExpression.IsNullable;

                    if (leftIsNullable && rightIsNullable)
                    {
                        Builder.Append("((");
                        Visit(node.Left);
                        Builder.Append(" IS NULL AND ");
                        Visit(node.Right);
                        Builder.Append(" IS NOT NULL) OR (");
                        Visit(node.Left);
                        Builder.Append(" IS NOT NULL AND ");
                        Visit(node.Right);
                        Builder.Append(" IS NULL) OR (");
                        var visitedLeft = Visit(node.Left);
                        Builder.Append(" <> ");
                        var visitedRight = Visit(node.Right);
                        Builder.Append("))");

                        return node.Update(visitedLeft, node.Conversion, visitedRight);
                    }
                    else if (leftIsNullable)
                    {
                        Builder.Append("(");
                        Visit(node.Left);
                        Builder.Append(" IS NULL OR (");
                        var visitedLeft = Visit(node.Left);
                        Builder.Append(" <> ");
                        var visitedRight = Visit(node.Right);
                        Builder.Append("))");

                        return node.Update(visitedLeft, node.Conversion, visitedRight);
                    }
                    else if (rightIsNullable)
                    {
                        Builder.Append("(");
                        Visit(node.Right);
                        Builder.Append(" IS NULL OR (");
                        var visitedLeft = Visit(node.Left);
                        Builder.Append(" <> ");
                        var visitedRight = Visit(node.Right);
                        Builder.Append("))");

                        return node.Update(visitedLeft, node.Conversion, visitedRight);
                    }

                    return VisitSimple(" <> ");
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

            var test = Visit(node.Test.AsSqlBooleanExpression());

            Builder.Append(" THEN ");

            var ifTrue = Visit(node.IfTrue);

            Builder.Append(" ELSE ");

            var ifFalse = Visit(node.IfFalse);

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
                    throw new NotSupportedException();
                }
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not
                when node.Type == typeof(bool):
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
                            return base.Visit(Expression.Equal(Expression.Constant(false), node.Operand));
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

            foreach (var (index, alias, expression) in projectionExpressions)
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

                    Builder.Append(" AS BIT)");
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

                Visit(selectExpression.Predicate.AsSqlBooleanExpression());
            }

            if (selectExpression.Grouping != null)
            {
                Builder.AppendLine();
                Builder.Append("GROUP BY ");

                var groupings = FlattenProjection(selectExpression.Grouping);

                EmitExpressionListExpression(groupings.First().expression);

                foreach (var grouping in groupings.Skip(1))
                {
                    Builder.Append(", ");

                    EmitExpressionListExpression(grouping.expression);
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
            Builder.Append(FormatIdentifier(baseTableExpression.SchemaName));
            Builder.Append(".");
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

            Visit(innerJoinExpression.Predicate);

            return innerJoinExpression;
        }

        protected virtual Expression VisitLeftJoinExpression(LeftJoinExpression leftJoinExpression)
        {
            Visit(leftJoinExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("LEFT JOIN ");

            Visit(leftJoinExpression.InnerTable);

            Builder.Append(" ON ");

            Visit(leftJoinExpression.Predicate);

            return leftJoinExpression;
        }

        protected virtual Expression VisitFullJoinExpression(FullJoinExpression fullJoinExpression)
        {
            Visit(fullJoinExpression.OuterTable);

            Builder.AppendLine();
            Builder.Append("FULL JOIN ");

            Visit(fullJoinExpression.InnerTable);

            Builder.Append(" ON ");

            Visit(fullJoinExpression.Predicate);

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

        protected virtual Expression VisitSqlAggregateExpression(SqlAggregateExpression sqlAggregateExpression)
        {
            Builder.Append(sqlAggregateExpression.FunctionName);
            Builder.Append("(");

            if (sqlAggregateExpression.IsDistinct)
            {
                Builder.Append("DISTINCT ");
            }

            Visit(sqlAggregateExpression.Expression);

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
            Builder.Append("CAST(");

            Visit(sqlCastExpression.Expression);

            Builder.Append($" AS {sqlCastExpression.SqlType})");

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
            Visit(sqlInExpression.Value);

            Builder.Append(" IN (");

            switch (sqlInExpression.Values)
            {
                case SelectExpression selectExpression:
                {
                    Builder.IncreaseIndent();
                    Builder.AppendLine();

                    Visit(selectExpression);

                    Builder.DecreaseIndent();
                    Builder.AppendLine();

                    break;
                }

                case NewArrayExpression newArrayExpression:
                {
                    foreach (var (expression, index) in newArrayExpression.Expressions.Select((e, i) => (e, i)))
                    {
                        if (index > 0)
                        {
                            Builder.Append(", ");
                        }

                        Visit(expression);
                    }

                    break;
                }

                case ListInitExpression listInitExpression:
                {
                    foreach (var (elementInit, index) in listInitExpression.Initializers.Select((e, i) => (e, i)))
                    {
                        if (index > 0)
                        {
                            Builder.Append(", ");
                        }

                        Visit(elementInit.Arguments[0]);
                    }

                    break;
                }

                case ConstantExpression constantExpression:
                {
                    var values = from object value in ((IEnumerable)constantExpression.Value)
                                 select Expression.Constant(value);

                    foreach (var (value, index) in values.Select((v, i) => (v, i)))
                    {
                        if (index > 0)
                        {
                            Builder.Append(", ");
                        }

                        Visit(value);
                    }

                    break;
                }

                case Expression expression:
                {
                    Builder.AddParameterList(expression, FormatParameterName);

                    break;
                }
            }

            Builder.Append(")");

            return sqlInExpression;
        }

        protected virtual Expression VisitSqlParameterExpression(SqlParameterExpression sqlParameterExpression)
        {
            Builder.AddParameter(sqlParameterExpression.Expression, FormatParameterName);

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
            if (orderByExpression is ThenOrderByExpression thenOrderBy)
            {
                Visit(thenOrderBy.Previous);

                Builder.Append(", ");
            }

            EmitExpressionListExpression(orderByExpression.Expression);

            Builder.Append(" ");
            Builder.Append(orderByExpression.Descending ? "DESC" : "ASC");

            return orderByExpression;
        }

        #endregion

        #region Extensibility points

        protected virtual void EmitExpressionListExpression(Expression expression)
        {
            if (expression.Type.IsBooleanType()
                && !(expression is ConditionalExpression
                    || expression is ConstantExpression
                    || expression is SqlColumnExpression
                    || expression is SqlCastExpression))
            {
                Builder.Append("(CASE WHEN ");

                Visit(expression);

                Builder.Append(" THEN 1 ELSE 0 END)");
            }
            else
            {
                Visit(expression);
            }
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
    }
}
