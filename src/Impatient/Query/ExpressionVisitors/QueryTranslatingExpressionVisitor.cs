using Impatient.Query.Expressions;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static System.Linq.Enumerable;

namespace Impatient.Query.ExpressionVisitors
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
            Builder = new DbCommandBuilderExpressionBuilder();
        }

        protected IImpatientExpressionVisitorProvider ExpressionVisitorProvider { get; }

        protected DbCommandBuilderExpressionBuilder Builder { get; }

        // TODO: Separate command builder production from materializer production
        public (Expression materializer, Expression commandBuilder) Translate(SelectExpression selectExpression)
        {
            selectExpression
                = new SqlParameterRewritingExpressionVisitor()
                    .VisitAndConvert(selectExpression, nameof(Translate));

            selectExpression = (SelectExpression)VisitSelectExpression(selectExpression, true);

            return (selectExpression.Projection.Flatten(), Builder.Build());
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

            IEnumerable<Expression> IterateBindings(IEnumerable<MemberBinding> bindings)
            {
                foreach (var binding in bindings)
                {
                    switch (binding)
                    {
                        case MemberAssignment memberAssignment:
                        {
                            yield return memberAssignment.Expression;

                            break;
                        }

                        case MemberMemberBinding memberMemberBinding:
                        {
                            foreach (var yielded in IterateBindings(memberMemberBinding.Bindings))
                            {
                                yield return yielded;
                            }

                            break;
                        }

                        default:
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
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
                    return VisitSimple(" AND ");
                }

                case ExpressionType.OrElse:
                {
                    return VisitSimple(" OR ");
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
                                IterateBindings(leftMemberInitExpression.Bindings));

                        var rightBindings
                            = rightMemberInitExpression.NewExpression.Arguments.Concat(
                                IterateBindings(rightMemberInitExpression.Bindings));

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
                                IterateBindings(leftMemberInitExpression.Bindings));

                        var rightBindings
                            = rightMemberInitExpression.NewExpression.Arguments.Concat(
                                IterateBindings(rightMemberInitExpression.Bindings));

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

            var test = Visit(node.Test);

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
                    Builder.Append($@"N'{value}'");
                    break;
                }

                case char value:
                {
                    Builder.Append($@"N'{value}'");
                    break;
                }

                case bool value:
                {
                    Builder.Append(value ? "1" : "0");
                    break;
                }

                case object value:
                {
                    Builder.Append(value.ToString());
                    break;
                }

                case null:
                {
                    Builder.Append("NULL");
                    break;
                }
            }

            return node;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression selectExpression:
                {
                    return VisitSelectExpression(selectExpression, false);
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

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.TypeEqual:
                case ExpressionType.TypeIs:
                {
                    if (node.Expression is PolymorphicExpression polymorphicExpression)
                    {
                        return Visit(
                            polymorphicExpression
                                .Filter(node.TypeOperand)
                                .Descriptors
                                .Select(d => d.Test.ExpandParameters(polymorphicExpression.Row))
                                .Aggregate(Expression.OrElse));
                    }

                    goto default;
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

        protected virtual Expression VisitSelectExpression(SelectExpression selectExpression, bool isTopLevel)
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

            var readerParameter = Expression.Parameter(typeof(DbDataReader));

            var projectionVisitor
                = new ReaderParameterInjectingExpressionVisitor(
                    ExpressionVisitorProvider,
                    readerParameter);

            var selectorBody = FlattenProjection(selectExpression.Projection, projectionVisitor);

            var projectionExpressions
                = projectionVisitor.GatheredExpressions
                    .Select((p, i) => (i, p.Key, p.Value))
                    .DefaultIfEmpty((0, null, Expression.Constant(1)));

            foreach (var (index, alias, expression) in projectionExpressions)
            {
                if (index > 0)
                {
                    Builder.Append(", ");
                }

                if (isTopLevel
                    && expression.Type.IsBooleanType()
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

                if (!(selectExpression.Predicate is BinaryExpression || selectExpression.Predicate is TypeBinaryExpression))
                {
                    Builder.Append("1 = ");
                }

                Visit(selectExpression.Predicate);
            }

            if (selectExpression.Grouping != null)
            {
                Builder.AppendLine();
                Builder.Append("GROUP BY ");

                var gatherer = new ProjectionLeafGatheringExpressionVisitor();

                gatherer.Visit(selectExpression.Grouping);

                foreach (var (index, alias, expression) in gatherer.GatheredExpressions.Select((p, i) => (i, p.Key, p.Value)))
                {
                    if (index > 0)
                    {
                        Builder.Append(", ");
                    }

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

            return selectExpression.UpdateProjection(
                new ServerProjectionExpression(
                    Expression.Lambda(selectorBody, readerParameter)));
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
                    var i = -1;

                    do
                    {
                        alias = $"{table.Alias}_{++i}";
                    }
                    while (!tableAliases.Add(alias));
                }

                aliasLookup.Add(table, alias);
            }

            return alias;
        }

        protected class SqlParameterRewritingExpressionVisitor : ExpressionVisitor
        {
            private readonly ParameterAndExtensionCountingExpressionVisitor countingVisitor
                = new ParameterAndExtensionCountingExpressionVisitor();

            public override Expression Visit(Expression node)
            {
                if (node == null || node is LambdaExpression)
                {
                    return node;
                }

                if (node.Type.IsScalarType())
                {
                    countingVisitor.Visit(node);

                    if (countingVisitor.ParameterCount > 0 && countingVisitor.ExtensionCount == 0)
                    {
                        return new SqlParameterExpression(node);
                    }

                    countingVisitor.ParameterCount = 0;
                    countingVisitor.ExtensionCount = 0;
                }

                return base.Visit(node);
            }

            private class ParameterAndExtensionCountingExpressionVisitor : ExpressionVisitor
            {
                public int ParameterCount;
                public int ExtensionCount;

                public override Expression Visit(Expression node)
                {
                    if (node == null)
                    {
                        return node;
                    }

                    switch (node.NodeType)
                    {
                        case ExpressionType.Parameter:
                        {
                            ParameterCount++;
                            break;
                        }

                        case ExpressionType.Extension:
                        {
                            ExtensionCount++;
                            break;
                        }
                    }

                    return base.Visit(node);
                }
            }
        }

        protected class SqlParameterExpression : SqlExpression
        {
            public SqlParameterExpression(Expression expression)
            {
                Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            }

            public override Type Type => Expression.Type;

            public Expression Expression { get; }
        }

        protected static Expression FlattenProjection(
            ProjectionExpression projection,
            ReaderParameterInjectingExpressionVisitor visitor)
        {
            switch (projection)
            {
                case ServerProjectionExpression serverProjectionExpression:
                {
                    return visitor.Inject(serverProjectionExpression.ResultLambda.Body);
                }

                case ClientProjectionExpression clientProjectionExpression:
                {
                    var server = clientProjectionExpression.ServerProjection;
                    var result = clientProjectionExpression.ResultLambda;

                    return Expression.Invoke(result, FlattenProjection(server, visitor));
                }

                case CompositeProjectionExpression compositeProjectionExpression:
                {
                    var outer = FlattenProjection(compositeProjectionExpression.OuterProjection, visitor);
                    var inner = FlattenProjection(compositeProjectionExpression.InnerProjection, visitor);
                    var result = compositeProjectionExpression.ResultLambda;

                    return result.ExpandParameters(outer, inner);
                }

                default:
                {
                    throw new InvalidOperationException();
                }
            }
        }

        protected class ReaderParameterInjectingExpressionVisitor : ProjectionExpressionVisitor
        {
            public IDictionary<string, Expression> GatheredExpressions { get; private set; } = new Dictionary<string, Expression>();

            private static readonly TypeInfo dbDataReaderTypeInfo
                = typeof(DbDataReader).GetTypeInfo();

            private static readonly MethodInfo getFieldValueMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

            private static readonly MethodInfo isDBNullMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

            private readonly IImpatientExpressionVisitorProvider expressionVisitorProvider;
            private readonly ParameterExpression readerParameter;
            private int readerIndex;
            private int topLevelIndex;

            private static readonly IReadValueExpressionFactory[] readValueExpressionFactories
                =
                {
                    new DefaultScalarReadValueExpressionFactory(),
                    new SqlServerForJsonReadValueExpressionFactory(),
                };

            public ReaderParameterInjectingExpressionVisitor(
                IImpatientExpressionVisitorProvider expressionVisitorProvider,
                ParameterExpression readerParameter)
            {
                this.expressionVisitorProvider = expressionVisitorProvider;
                this.readerParameter = readerParameter;
            }

            public Expression Inject(Expression node)
            {
                if (topLevelIndex > 0)
                {
                    if (topLevelIndex == 1)
                    {
                        GatheredExpressions
                            = GatheredExpressions
                                .Select(p => ("$0." + p.Key, p.Value))
                                .ToDictionary(p => p.Item1, p => p.Item2);
                    }

                    var previousExpressions = GatheredExpressions;

                    GatheredExpressions = new Dictionary<string, Expression>();

                    var result = Visit(node);

                    GatheredExpressions
                        = GatheredExpressions
                            .Select(p => ($"${topLevelIndex}." + p.Key, p.Value))
                            .ToDictionary(p => p.Item1, p => p.Item2)
                            .Concat(previousExpressions)
                            .ToDictionary(p => p.Key, p => p.Value);

                    topLevelIndex++;

                    return result;
                }

                topLevelIndex++;

                return Visit(node);
            }

            public override Expression Visit(Expression node)
            {
                switch (node)
                {
                    case null:
                    case NewExpression _:
                    case MemberInitExpression _:
                    {
                        return base.Visit(node);
                    }

                    case DefaultIfEmptyExpression defaultIfEmptyExpression:
                    {
                        var name = string.Join(".", GetNameParts().Append("$empty"));

                        GatheredExpressions[name] = defaultIfEmptyExpression.Flag;

                        return Expression.Condition(
                            test: Expression.Call(
                                readerParameter,
                                isDBNullMethodInfo,
                                Expression.Constant(readerIndex++)),
                            ifTrue: Expression.Default(defaultIfEmptyExpression.Type),
                            ifFalse: Visit(defaultIfEmptyExpression.Expression));
                    }

                    case PolymorphicExpression polymorphicExpression:
                    {
                        var row = Visit(polymorphicExpression.Row);
                        var descriptors = polymorphicExpression.Descriptors.ToArray();
                        var result = (Expression)Expression.Default(polymorphicExpression.Type);

                        for (var i = 0; i < descriptors.Length; i++)
                        {
                            result = Expression.Condition(
                                test: descriptors[i].Test.ExpandParameters(row),
                                ifTrue: descriptors[i].Materializer.ExpandParameters(row),
                                ifFalse: result,
                                type: polymorphicExpression.Type);
                        }

                        return result;
                    }

                    case UnaryExpression unaryExpression
                    when unaryExpression.NodeType == ExpressionType.Convert:
                    {
                        return unaryExpression.Update(Visit(unaryExpression.Operand));
                    }

                    default:
                    {
                        if (expressionVisitorProvider.TranslatabilityAnalyzingExpressionVisitor.Visit(node) is TranslatableExpression)
                        {
                            GatheredExpressions[string.Join(".", GetNameParts())] = node;

                            var currentIndex = readerIndex;
                            readerIndex++;

                            return readValueExpressionFactories
                                .First(f => f.CanReadType(node.Type))
                                .CreateExpression(node, readerParameter, currentIndex);
                        }

                        return node;
                    }
                }
            }
        }

        protected class DbCommandBuilderExpressionBuilder
        {
            private int parameterIndex = 0;
            private int indentationLevel = 0;
            private bool containsParameterList = false;
            private StringBuilder archiveStringBuilder = new StringBuilder();
            private StringBuilder workingStringBuilder = new StringBuilder();

            private readonly ParameterExpression dbCommandVariable = Expression.Parameter(typeof(DbCommand), "command");
            private readonly ParameterExpression stringBuilderVariable = Expression.Parameter(typeof(StringBuilder), "builder");
            private readonly ParameterExpression dbParameterVariable = Expression.Parameter(typeof(DbParameter), "parameter");
            private readonly List<Expression> blockExpressions = new List<Expression>();
            private readonly List<Expression> dbParameterExpressions = new List<Expression>();

            private static readonly MethodInfo stringBuilderAppendMethodInfo
                = typeof(StringBuilder).GetRuntimeMethod(nameof(StringBuilder.Append), new[] { typeof(string) });

            private static readonly MethodInfo stringBuilderToStringMethodInfo
                = typeof(StringBuilder).GetRuntimeMethod(nameof(StringBuilder.ToString), new Type[0]);

            private static readonly MethodInfo stringConcatObjectMethodInfo
                = typeof(string).GetRuntimeMethod(nameof(string.Concat), new[] { typeof(object), typeof(object) });

            private static readonly PropertyInfo dbCommandCommandTextPropertyInfo
                = typeof(DbCommand).GetTypeInfo().GetDeclaredProperty(nameof(DbCommand.CommandText));

            private static readonly PropertyInfo dbCommandParametersPropertyInfo
                = typeof(DbCommand).GetTypeInfo().GetDeclaredProperty(nameof(DbCommand.Parameters));

            private static readonly MethodInfo dbCommandCreateParameterMethodInfo
                = typeof(DbCommand).GetTypeInfo().GetDeclaredMethod(nameof(DbCommand.CreateParameter));

            private static readonly PropertyInfo dbParameterParameterNamePropertyInfo
                = typeof(DbParameter).GetTypeInfo().GetDeclaredProperty(nameof(DbParameter.ParameterName));

            private static readonly PropertyInfo dbParameterValuePropertyInfo
                = typeof(DbParameter).GetTypeInfo().GetDeclaredProperty(nameof(DbParameter.Value));

            private static readonly MethodInfo dbParameterCollectionAddMethodInfo
                = typeof(DbParameterCollection).GetTypeInfo().GetDeclaredMethod(nameof(DbParameterCollection.Add));

            private static readonly MethodInfo enumerableGetEnumeratorMethodInfo
                = typeof(IEnumerable).GetTypeInfo().GetDeclaredMethod(nameof(IEnumerable.GetEnumerator));

            private static readonly MethodInfo enumeratorMoveNextMethodInfo
                = typeof(IEnumerator).GetTypeInfo().GetDeclaredMethod(nameof(IEnumerator.MoveNext));

            private static readonly PropertyInfo enumeratorCurrentPropertyInfo
                = typeof(IEnumerator).GetTypeInfo().GetDeclaredProperty(nameof(IEnumerator.Current));

            private static readonly MethodInfo disposableDisposeMethodInfo
                = typeof(IDisposable).GetTypeInfo().GetDeclaredMethod(nameof(IDisposable.Dispose));

            public void Append(string sql)
            {
                workingStringBuilder.Append(sql);
            }

            public void AppendLine()
            {
                workingStringBuilder.AppendLine();
                AppendIndent();
            }

            public void EmitSql()
            {
                if (workingStringBuilder.Length > 0)
                {
                    var currentString = workingStringBuilder.ToString();

                    blockExpressions.Add(
                        Expression.Call(
                            stringBuilderVariable,
                            stringBuilderAppendMethodInfo,
                            Expression.Constant(currentString)));

                    archiveStringBuilder.Append(currentString);

                    workingStringBuilder.Clear();
                }
            }

            public void AddParameter(Expression node, Func<string, string> formatter)
            {
                var parameterName = formatter($"p{parameterIndex}");

                Append(parameterName);

                EmitSql();

                var expressions = new Expression[]
                {
                    Expression.Assign(
                        dbParameterVariable,
                        Expression.Call(
                            dbCommandVariable,
                            dbCommandCreateParameterMethodInfo)),
                    Expression.Assign(
                        Expression.MakeMemberAccess(
                            dbParameterVariable,
                            dbParameterParameterNamePropertyInfo),
                        Expression.Constant(parameterName)),
                    Expression.Assign(
                        Expression.MakeMemberAccess(
                            dbParameterVariable,
                            dbParameterValuePropertyInfo),
                        node.Type.GetTypeInfo().IsValueType
                            ? Expression.Convert(node, typeof(object))
                            : node),
                    Expression.Call(
                        Expression.MakeMemberAccess(
                            dbCommandVariable,
                            dbCommandParametersPropertyInfo),
                        dbParameterCollectionAddMethodInfo,
                        dbParameterVariable)
                };

                blockExpressions.AddRange(expressions);
                dbParameterExpressions.AddRange(expressions);

                parameterIndex++;
            }

            public void AddParameterList(Expression node, Func<string, string> formatter)
            {
                var enumeratorVariable = Expression.Parameter(typeof(IEnumerator), "enumerator");
                var indexVariable = Expression.Parameter(typeof(int), "index");
                var parameterPrefixVariable = Expression.Parameter(typeof(string), "parameterPrefix");
                var parameterNameVariable = Expression.Parameter(typeof(string), "parameterName");
                var breakLabel = Expression.Label();

                var parameterListBlock
                    = Expression.Block(
                        new[]
                        {
                            enumeratorVariable,
                            indexVariable,
                            parameterPrefixVariable,
                            parameterNameVariable
                        },
                        Expression.TryFinally(
                            body: Expression.Block(
                                Expression.Assign(
                                    enumeratorVariable,
                                    Expression.Call(
                                        node,
                                        enumerableGetEnumeratorMethodInfo)),
                                Expression.Assign(
                                    parameterPrefixVariable,
                                    Expression.Constant(formatter($"p{parameterIndex}_"))),
                                Expression.Loop(
                                    @break: breakLabel,
                                    body: Expression.Block(
                                        Expression.Assign(
                                            parameterNameVariable,
                                            Expression.Call(
                                                stringConcatObjectMethodInfo,
                                                parameterPrefixVariable,
                                                Expression.Convert(indexVariable, typeof(object)))),
                                        Expression.IfThenElse(
                                            Expression.Call(enumeratorVariable, enumeratorMoveNextMethodInfo),
                                            Expression.Increment(indexVariable),
                                            Expression.Break(breakLabel)),
                                        Expression.IfThen(
                                            Expression.GreaterThan(indexVariable, Expression.Constant(0)),
                                            Expression.Call(
                                                stringBuilderVariable,
                                                stringBuilderAppendMethodInfo,
                                                Expression.Constant(", "))),
                                        Expression.Call(
                                            stringBuilderVariable,
                                            stringBuilderAppendMethodInfo,
                                            parameterNameVariable),
                                        Expression.Assign(
                                            dbParameterVariable,
                                            Expression.Call(
                                                dbCommandVariable,
                                                dbCommandCreateParameterMethodInfo)),
                                        Expression.Assign(
                                            Expression.MakeMemberAccess(
                                                dbParameterVariable,
                                                dbParameterParameterNamePropertyInfo),
                                            parameterNameVariable),
                                        Expression.Assign(
                                            Expression.MakeMemberAccess(
                                                dbParameterVariable,
                                                dbParameterValuePropertyInfo),
                                            Expression.MakeMemberAccess(
                                                enumeratorVariable,
                                                enumeratorCurrentPropertyInfo)),
                                        Expression.Call(
                                            Expression.MakeMemberAccess(
                                                dbCommandVariable,
                                                dbCommandParametersPropertyInfo),
                                            dbParameterCollectionAddMethodInfo,
                                            dbParameterVariable)))),
                                @finally: Expression.IfThen(
                                    Expression.TypeIs(
                                        enumeratorVariable,
                                        typeof(IDisposable)),
                                    Expression.Call(
                                        Expression.Convert(
                                            enumeratorVariable,
                                            typeof(IDisposable)),
                                        disposableDisposeMethodInfo))));

                EmitSql();
                blockExpressions.Add(parameterListBlock);
                parameterIndex++;
                containsParameterList = true;
            }

            public Expression Build()
            {
                EmitSql();

                var blockVariables = new List<ParameterExpression> { stringBuilderVariable, dbParameterVariable };
                var blockExpressions = this.blockExpressions;

                if (containsParameterList)
                {
                    blockExpressions.Insert(0,
                        Expression.Assign(
                            stringBuilderVariable,
                            Expression.New(typeof(StringBuilder))));

                    blockExpressions.Add(
                        Expression.Assign(
                            Expression.MakeMemberAccess(
                                dbCommandVariable,
                                dbCommandCommandTextPropertyInfo),
                            Expression.Call(
                                stringBuilderVariable,
                                stringBuilderToStringMethodInfo)));
                }
                else
                {
                    blockVariables.Remove(stringBuilderVariable);

                    blockExpressions.Clear();

                    blockExpressions.Add(
                        Expression.Assign(
                            Expression.MakeMemberAccess(
                                dbCommandVariable,
                                dbCommandCommandTextPropertyInfo),
                            Expression.Constant(archiveStringBuilder.ToString())));

                    blockExpressions.AddRange(dbParameterExpressions);

                    if (dbParameterExpressions.Count == 0)
                    {
                        blockVariables.Remove(dbParameterVariable);
                    }
                }

                return Expression.Lambda(
                    typeof(Action<DbCommand>),
                    Expression.Block(blockVariables, blockExpressions),
                    dbCommandVariable);
            }

            public void IncreaseIndent()
            {
                indentationLevel++;
            }

            public void DecreaseIndent()
            {
                indentationLevel--;
            }

            private void AppendIndent()
            {
                workingStringBuilder.Append(string.Empty.PadLeft(indentationLevel * 4));
            }
        }

        #region IReadValueExpressionFactory

        protected interface IReadValueExpressionFactory
        {
            bool CanReadType(Type type);

            Expression CreateExpression(Expression source, Expression reader, int index);
        }

        protected class SqlServerForJsonReadValueExpressionFactory : IReadValueExpressionFactory
        {
            private static readonly TypeInfo dbDataReaderTypeInfo
                = typeof(DbDataReader).GetTypeInfo();

            private static readonly MethodInfo getFieldValueMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

            private static readonly MethodInfo isDBNullMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

            private static readonly MethodInfo enumerableEmptyMethodInfo
                = ImpatientExtensions.GetGenericMethodDefinition((object o) => Empty<object>());

            public bool CanReadType(Type type)
            {
                return !type.IsScalarType();
            }

            public Expression CreateExpression(Expression source, Expression reader, int index)
            {
                var type = source.Type;
                var sequenceType = type.GetSequenceType();
                var defaultValue = Expression.Default(type) as Expression;

                if (type.IsSequenceType())
                {
                    if (type.IsArray)
                    {
                        defaultValue = Expression.NewArrayInit(type.GetElementType());
                    }
                    else if (type.IsGenericType(typeof(List<>)))
                    {
                        defaultValue = Expression.New(type);
                    }
                    else
                    {
                        type = type.FindGenericType(typeof(IEnumerable<>));
                        defaultValue = Expression.Call(enumerableEmptyMethodInfo.MakeGenericMethod(sequenceType));
                    }

                    if (sequenceType.IsScalarType())
                    {
                        // TODO: Handle sequences of scalar types with FOR JSON
                        // - Use a JsonTextReader to stream through the text
                        // - while (reader.Read())
                        // - (StartArray)(1)
                        // - (StartObject -> PropertyName -> String | Number -> EndObject)(*)
                        // - (EndArray)(1)
                    }
                }

                var getFieldValueExpression
                    = Expression.Call(
                        reader,
                        getFieldValueMethodInfo.MakeGenericMethod(typeof(string)),
                        Expression.Constant(index));

                var deserializerExpression
                    = Expression.Call(
                        ImpatientExtensions
                            .GetGenericMethodDefinition((string s) => JsonConvert.DeserializeObject<object>(s))
                            .MakeGenericMethod(type),
                        getFieldValueExpression);

                var result
                    = Expression.Condition(
                        Expression.Call(
                            reader,
                            isDBNullMethodInfo,
                            Expression.Constant(index)),
                        defaultValue,
                        deserializerExpression) as Expression;

                if (source.Type.IsGenericType(typeof(IQueryable<>)))
                {
                    result
                        = Expression.Call(
                            ImpatientExtensions
                                .GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable())
                                .MakeGenericMethod(sequenceType),
                            result);
                }

                return result;
            }
        }

        protected class DefaultScalarReadValueExpressionFactory : IReadValueExpressionFactory
        {
            private static readonly TypeInfo dbDataReaderTypeInfo
                = typeof(DbDataReader).GetTypeInfo();

            private static readonly MethodInfo getFieldValueMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

            private static readonly MethodInfo isDBNullMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

            public bool CanReadType(Type type)
            {
                return type.IsScalarType();
            }

            public Expression CreateExpression(Expression source, Expression reader, int index)
            {
                var readValueExpression
                    = Expression.Call(
                        reader,
                        getFieldValueMethodInfo.MakeGenericMethod(source.Type),
                        Expression.Constant(index));

                if (source is SqlColumnExpression sqlColumnExpression
                    && !sqlColumnExpression.IsNullable)
                {
                    return readValueExpression;
                }

                return Expression.Condition(
                    Expression.Call(
                        reader,
                        isDBNullMethodInfo,
                        Expression.Constant(index)),
                    Expression.Default(source.Type),
                    readValueExpression);
            }
        }

        #endregion

        #region IComplexTypeSubqueryFormatter

        protected interface IComplexTypeSubqueryFormatter
        {
            SelectExpression Format(SelectExpression subquery, DbCommandBuilderExpressionBuilder builder, ExpressionVisitor visitor);
        }

        protected class SqlServerForJsonComplexTypeSubqueryFormatter : IComplexTypeSubqueryFormatter
        {
            public SelectExpression Format(SelectExpression subquery, DbCommandBuilderExpressionBuilder builder, ExpressionVisitor visitor)
            {
                builder.Append("(");

                builder.IncreaseIndent();
                builder.AppendLine();

                subquery = visitor.VisitAndConvert(subquery, nameof(Format));

                builder.AppendLine();
                builder.Append("FOR JSON PATH");

                builder.DecreaseIndent();
                builder.AppendLine();

                builder.Append(")");

                return subquery;
            }
        }

        #endregion
    }
}
