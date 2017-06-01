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

namespace Impatient.Query.ExpressionVisitors
{
    public class QueryTranslatingExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<object> selectExpressionSourceStack = new Stack<object>();
        private int queryDepth = -1;
        private HashSet<string> tableAliases = new HashSet<string>();
        private IDictionary<AliasedTableExpression, string> aliasLookup = new Dictionary<AliasedTableExpression, string>();
        private DbCommandBuilderExpressionBuilder builder = new DbCommandBuilderExpressionBuilder();

        private readonly IImpatientExpressionVisitorProvider expressionVisitorProvider;

        public QueryTranslatingExpressionVisitor(IImpatientExpressionVisitorProvider expressionVisitorProvider)
        {
            this.expressionVisitorProvider = expressionVisitorProvider ?? throw new ArgumentNullException(nameof(expressionVisitorProvider));
        }

        public (Expression materializer, Expression commandBuilder) Translate(SelectExpression selectExpression)
        {
            selectExpression
                = new SqlParameterRewritingExpressionVisitor()
                    .VisitAndConvert(selectExpression, nameof(Translate));

            selectExpressionSourceStack.Push(selectExpression);

            selectExpression = VisitAndConvert(selectExpression, nameof(Translate));

            selectExpressionSourceStack.Pop();

            return (selectExpression.Projection.Flatten(), builder.Build());
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
                        builder.Append("(");

                        operand = VisitBinary(binaryExpression);

                        builder.Append(")");

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

                builder.Append(@operator);

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
                    builder.Append("COALESCE(");

                    var left = Visit(node.Left);

                    builder.Append(", ");

                    var right = Visit(node.Right);

                    builder.Append(")");

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
                                .Aggregate(Expression.AndAlso));
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
                                .Aggregate(Expression.AndAlso));
                    }
                    else if (node.Left is ConstantExpression leftConstantExpression
                        && leftConstantExpression.Value is null)
                    {
                        var visitedRight = Visit(node.Right);

                        builder.Append(" IS NULL");

                        return node.Update(node.Left, node.Conversion, visitedRight);
                    }
                    else if (node.Right is ConstantExpression rightConstantExpression
                        && rightConstantExpression.Value is null)
                    {
                        var visitedLeft = Visit(node.Left);

                        builder.Append(" IS NULL");

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
                        builder.Append("((");
                        Visit(node.Left);
                        builder.Append(" IS NULL AND ");
                        Visit(node.Right);
                        builder.Append(" IS NULL) OR (");
                        var visitedLeft = Visit(node.Left);
                        builder.Append(" = ");
                        var visitedRight = Visit(node.Right);
                        builder.Append("))");

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

                        builder.Append(" IS NOT NULL");

                        return node.Update(node.Left, node.Conversion, visitedRight);
                    }
                    else if (node.Right is ConstantExpression rightConstantExpression
                        && rightConstantExpression.Value is null)
                    {
                        var visitedLeft = Visit(node.Left);

                        builder.Append(" IS NOT NULL");

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
                        builder.Append("((");
                        Visit(node.Left);
                        builder.Append(" IS NULL AND ");
                        Visit(node.Right);
                        builder.Append(" IS NOT NULL) OR (");
                        Visit(node.Left);
                        builder.Append(" IS NOT NULL AND ");
                        Visit(node.Right);
                        builder.Append(" IS NULL) OR (");
                        var visitedLeft = Visit(node.Left);
                        builder.Append(" <> ");
                        var visitedRight = Visit(node.Right);
                        builder.Append("))");

                        return node.Update(visitedLeft, node.Conversion, visitedRight);
                    }
                    else if (leftIsNullable)
                    {
                        builder.Append("(");
                        Visit(node.Left);
                        builder.Append(" IS NULL OR (");
                        var visitedLeft = Visit(node.Left);
                        builder.Append(" <> ");
                        var visitedRight = Visit(node.Right);
                        builder.Append("))");

                        return node.Update(visitedLeft, node.Conversion, visitedRight);
                    }
                    else if (rightIsNullable)
                    {
                        builder.Append("(");
                        Visit(node.Right);
                        builder.Append(" IS NULL OR (");
                        var visitedLeft = Visit(node.Left);
                        builder.Append(" <> ");
                        var visitedRight = Visit(node.Right);
                        builder.Append("))");

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
            builder.Append("(CASE WHEN ");

            var test = Visit(node.Test);

            builder.Append(" THEN ");

            var ifTrue = Visit(node.IfTrue);

            builder.Append(" ELSE ");

            var ifFalse = Visit(node.IfFalse);

            builder.Append(" END)");

            return node.Update(test, ifTrue, ifFalse);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            switch (node.Value)
            {
                case string value:
                {
                    builder.Append($@"N'{value}'");
                    break;
                }

                case bool value:
                {
                    builder.Append(value ? "1" : "0");
                    break;
                }

                case object value:
                {
                    builder.Append(value.ToString());
                    break;
                }

                case null:
                {
                    builder.Append("NULL");
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
                    queryDepth++;

                    builder.Append("SELECT ");

                    if (selectExpression.IsDistinct)
                    {
                        builder.Append("DISTINCT ");
                    }

                    if (selectExpression.Limit != null && selectExpression.Offset == null)
                    {
                        builder.Append("TOP (");

                        Visit(selectExpression.Limit);

                        builder.Append(") ");
                    }

                    var readerParameter = Expression.Parameter(typeof(DbDataReader));

                    var projectionVisitor
                        = new ReaderParameterInjectingExpressionVisitor(
                            expressionVisitorProvider,
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
                            builder.Append(", ");
                        }

                        if (queryDepth == 0
                            && expression.Type.IsBoolean()
                            && !(expression is SqlAliasExpression
                               || expression is SqlColumnExpression
                               || expression is SqlCastExpression))
                        {
                            builder.Append("CAST(");

                            EmitExpressionListExpression(expression);

                            builder.Append(" AS BIT)");
                        }
                        else
                        {
                            EmitExpressionListExpression(expression);
                        }

                        if (!string.IsNullOrEmpty(alias))
                        {
                            builder.Append(" AS ");
                            builder.Append(FormatIdentifier(alias));
                        }
                        else if (selectExpressionSourceStack.Peek() is SubqueryTableExpression
                            && !(expression is SqlColumnExpression
                                || expression is SqlAliasExpression))
                        {
                            // TODO: Something OTHER THAN this
                            builder.Append(" AS ");
                            builder.Append(FormatIdentifier("$c"));
                        }
                    }

                    if (selectExpression.Table != null)
                    {
                        builder.AppendLine();
                        builder.Append($"FROM ");

                        Visit(selectExpression.Table);
                    }

                    if (selectExpression.Predicate != null)
                    {
                        builder.AppendLine();
                        builder.Append("WHERE ");

                        Visit(selectExpression.Predicate);
                    }

                    if (selectExpression.Grouping != null)
                    {
                        builder.AppendLine();
                        builder.Append("GROUP BY ");

                        var gatherer = new ProjectionLeafGatheringExpressionVisitor();
                        gatherer.Visit(selectExpression.Grouping);

                        foreach (var (index, alias, expression) in gatherer.GatheredExpressions.Select((p, i) => (i, p.Key, p.Value)))
                        {
                            if (index > 0)
                            {
                                builder.Append(", ");
                            }

                            EmitExpressionListExpression(expression);
                        }
                    }

                    if (selectExpression.OrderBy != null)
                    {
                        builder.AppendLine();
                        builder.Append("ORDER BY ");

                        Visit(selectExpression.OrderBy);
                    }

                    if (selectExpression.Offset != null)
                    {
                        if (selectExpression.OrderBy == null)
                        {
                            builder.AppendLine();
                            builder.Append("ORDER BY (SELECT 1)");
                        }

                        builder.AppendLine();
                        builder.Append("OFFSET ");

                        Visit(selectExpression.Offset);

                        builder.Append(" ROWS");

                        if (selectExpression.Limit != null)
                        {
                            builder.Append(" FETCH NEXT ");

                            Visit(selectExpression.Limit);

                            builder.Append(" ROWS ONLY");
                        }
                    }

                    queryDepth--;

                    return selectExpression.UpdateProjection(
                        new ServerProjectionExpression(
                            Expression.Lambda(selectorBody, readerParameter)));
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
                {
                    selectExpressionSourceStack.Push(singleValueRelationalQueryExpression);

                    if (!singleValueRelationalQueryExpression.Type.IsScalarType())
                    {
                        VisitComplexNestedQuery(singleValueRelationalQueryExpression.SelectExpression);
                    }
                    else
                    {
                        builder.Append("(");

                        builder.IncreaseIndent();
                        builder.AppendLine();

                        Visit(singleValueRelationalQueryExpression.SelectExpression);

                        builder.DecreaseIndent();
                        builder.AppendLine();

                        builder.Append(")");
                    }

                    selectExpressionSourceStack.Pop();

                    return singleValueRelationalQueryExpression;
                }

                case EnumerableRelationalQueryExpression enumerableRelationalQueryExpression:
                {
                    selectExpressionSourceStack.Push(enumerableRelationalQueryExpression);

                    VisitComplexNestedQuery(enumerableRelationalQueryExpression.SelectExpression);

                    selectExpressionSourceStack.Pop();

                    return enumerableRelationalQueryExpression;
                }

                case TableExpression tableExpression:
                {
                    switch (tableExpression)
                    {
                        case BaseTableExpression baseTableExpression:
                        {
                            builder.Append(FormatIdentifier(baseTableExpression.SchemaName));
                            builder.Append(".");
                            builder.Append(FormatIdentifier(baseTableExpression.TableName));
                            builder.Append(" AS ");
                            builder.Append(FormatIdentifier(GetTableAlias(baseTableExpression)));

                            return baseTableExpression;
                        }

                        case SubqueryTableExpression subqueryTableExpression:
                        {
                            builder.Append("(");

                            builder.IncreaseIndent();
                            builder.AppendLine();

                            selectExpressionSourceStack.Push(subqueryTableExpression);

                            Visit(subqueryTableExpression.Subquery);

                            selectExpressionSourceStack.Pop();

                            builder.DecreaseIndent();
                            builder.AppendLine();

                            builder.Append(") AS ");
                            builder.Append(FormatIdentifier(GetTableAlias(subqueryTableExpression)));

                            return subqueryTableExpression;
                        }

                        case InnerJoinExpression innerJoinExpression:
                        {
                            Visit(innerJoinExpression.OuterTable);

                            builder.AppendLine();
                            builder.Append("INNER JOIN ");

                            Visit(innerJoinExpression.InnerTable);

                            builder.Append(" ON ");

                            Visit(innerJoinExpression.Predicate);

                            return innerJoinExpression;
                        }

                        case LeftJoinExpression leftJoinExpression:
                        {
                            Visit(leftJoinExpression.OuterTable);

                            builder.AppendLine();
                            builder.Append("LEFT JOIN ");

                            Visit(leftJoinExpression.InnerTable);

                            builder.Append(" ON ");

                            Visit(leftJoinExpression.Predicate);

                            return leftJoinExpression;
                        }

                        case CrossJoinExpression crossJoinExpression:
                        {
                            Visit(crossJoinExpression.OuterTable);

                            builder.AppendLine();
                            builder.Append("CROSS JOIN ");

                            Visit(crossJoinExpression.InnerTable);

                            return crossJoinExpression;
                        }

                        case CrossApplyExpression crossApplyExpression:
                        {
                            Visit(crossApplyExpression.OuterTable);

                            builder.AppendLine();
                            builder.Append("CROSS APPLY ");

                            Visit(crossApplyExpression.InnerTable);

                            return crossApplyExpression;
                        }

                        case OuterApplyExpression outerApplyExpression:
                        {
                            Visit(outerApplyExpression.OuterTable);

                            builder.AppendLine();
                            builder.Append("OUTER APPLY ");

                            Visit(outerApplyExpression.InnerTable);

                            return outerApplyExpression;
                        }

                        case SetOperatorExpression setOperatorExpression:
                        {
                            builder.Append("(");

                            builder.IncreaseIndent();
                            builder.AppendLine();

                            Visit(setOperatorExpression.Set1);

                            builder.AppendLine();

                            switch (setOperatorExpression)
                            {
                                case ExceptExpression exceptExpression:
                                {
                                    builder.Append("EXCEPT");
                                    break;
                                }

                                case IntersectExpression intersectExpression:
                                {
                                    builder.Append("INTERSECT");
                                    break;
                                }

                                case UnionAllExpression unionAllExpression:
                                {
                                    builder.Append("UNION ALL");
                                    break;
                                }

                                case UnionExpression unionExpression:
                                {
                                    builder.Append("UNION");
                                    break;
                                }

                                default:
                                {
                                    throw new NotSupportedException();
                                }
                            }

                            builder.AppendLine();

                            Visit(setOperatorExpression.Set2);

                            builder.DecreaseIndent();
                            builder.AppendLine();

                            builder.Append(") AS ");
                            builder.Append(FormatIdentifier(GetTableAlias(setOperatorExpression)));

                            return setOperatorExpression;
                        }

                        default:
                        {
                            throw new NotSupportedException();
                        }
                    }
                }

                case SqlExpression sqlExpression:
                {
                    switch (sqlExpression)
                    {
                        case SqlAggregateExpression sqlAggregateExpression:
                        {
                            builder.Append(sqlAggregateExpression.FunctionName);
                            builder.Append("(");

                            if (sqlAggregateExpression.IsDistinct)
                            {
                                builder.Append("DISTINCT ");
                            }

                            Visit(sqlAggregateExpression.Expression);

                            builder.Append(")");

                            return sqlAggregateExpression;
                        }

                        case SqlAliasExpression sqlAliasExpression:
                        {
                            Visit(sqlAliasExpression.Expression);

                            builder.Append(" AS ");
                            builder.Append(FormatIdentifier(sqlAliasExpression.Alias));

                            return sqlAliasExpression;
                        }

                        case SqlCastExpression sqlCastExpression:
                        {
                            builder.Append("CAST(");

                            Visit(sqlCastExpression.Expression);

                            builder.Append($" AS {sqlCastExpression.SqlType})");

                            return sqlCastExpression;
                        }

                        case SqlColumnExpression sqlColumnExpression:
                        {
                            builder.Append(FormatIdentifier(GetTableAlias(sqlColumnExpression.Table)));
                            builder.Append(".");
                            builder.Append(FormatIdentifier(sqlColumnExpression.ColumnName));

                            return sqlColumnExpression;
                        }

                        case SqlExistsExpression sqlExistsExpression:
                        {
                            builder.Append("EXISTS (");

                            builder.IncreaseIndent();
                            builder.AppendLine();

                            Visit(sqlExistsExpression.SelectExpression);

                            builder.DecreaseIndent();
                            builder.AppendLine();

                            builder.Append(")");

                            return sqlExistsExpression;
                        }

                        case SqlFragmentExpression sqlFragmentExpression:
                        {
                            builder.Append(sqlFragmentExpression.Fragment);

                            return sqlFragmentExpression;
                        }

                        case SqlFunctionExpression sqlFunctionExpression:
                        {
                            builder.Append(sqlFunctionExpression.FunctionName);
                            builder.Append("(");

                            if (sqlFunctionExpression.Arguments.Any())
                            {
                                Visit(sqlFunctionExpression.Arguments.First());

                                foreach (var argument in sqlFunctionExpression.Arguments.Skip(1))
                                {
                                    builder.Append(", ");

                                    Visit(argument);
                                }
                            }

                            builder.Append(")");

                            return sqlFunctionExpression;
                        }

                        case SqlInExpression sqlInExpression:
                        {
                            Visit(sqlInExpression.Value);

                            builder.Append(" IN (");

                            switch (sqlInExpression.Values)
                            {
                                case SelectExpression selectExpression:
                                {
                                    builder.IncreaseIndent();
                                    builder.AppendLine();

                                    Visit(selectExpression);

                                    builder.DecreaseIndent();
                                    builder.AppendLine();

                                    break;
                                }

                                case NewArrayExpression newArrayExpression:
                                {
                                    foreach (var (expression, index) in newArrayExpression.Expressions.Select((e, i) => (e, i)))
                                    {
                                        if (index > 0)
                                        {
                                            builder.Append(", ");
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
                                            builder.Append(", ");
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
                                            builder.Append(", ");
                                        }

                                        Visit(value);
                                    }

                                    break;
                                }

                                case Expression expression:
                                {
                                    builder.AddParameterList(expression, FormatParameterName);

                                    break;
                                }
                            }

                            builder.Append(")");

                            return sqlInExpression;
                        }

                        case SqlParameterExpression sqlParameterExpression:
                        {
                            builder.AddParameter(sqlParameterExpression.Expression, FormatParameterName);

                            return sqlParameterExpression;
                        }

                        default:
                        {
                            throw new NotSupportedException();
                        }
                    }
                }

                case OrderByExpression orderByExpression:
                {
                    if (orderByExpression is ThenOrderByExpression thenOrderBy)
                    {
                        Visit(thenOrderBy.Previous);

                        builder.Append(", ");
                    }

                    EmitExpressionListExpression(orderByExpression.Expression);

                    builder.Append(" ");
                    builder.Append(orderByExpression.Descending ? "DESC" : "ASC");

                    return orderByExpression;
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
                                .Select(d => d.Test.ExpandParameters(
                                    d.Materializer.ExpandParameters(
                                        polymorphicExpression.Row)))
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
                            builder.Append("NOT ");

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
                    builder.Append("~ ");

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

        #region Extensibility points

        protected virtual Expression VisitComplexNestedQuery(SelectExpression subquery)
        {
            builder.Append("(");

            builder.IncreaseIndent();
            builder.AppendLine();

            Visit(subquery);

            builder.AppendLine();
            builder.Append("FOR JSON PATH");

            builder.DecreaseIndent();
            builder.AppendLine();

            builder.Append(")");

            return subquery;
        }

        protected virtual void EmitExpressionListExpression(Expression expression)
        {
            if (expression.Type.IsBoolean()
                && !(expression is ConditionalExpression
                    || expression is ConstantExpression
                    || expression is SqlColumnExpression
                    || expression is SqlCastExpression))
            {
                builder.Append("(CASE WHEN ");

                Visit(expression);

                builder.Append(" THEN 1 ELSE 0 END)");
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

        private string GetTableAlias(AliasedTableExpression table)
        {
            if (!aliasLookup.TryGetValue(table, out var alias))
            {
                alias = table.Alias;

                if (!tableAliases.Add(alias))
                {
                    var i = -1;

                    do
                    {
                        alias = $"{table.Alias}{++i}";
                    }
                    while (!tableAliases.Add(alias));
                }

                aliasLookup.Add(table, alias);
            }

            return alias;
        }

        private class SqlParameterRewritingExpressionVisitor : ExpressionVisitor
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

        private class SqlParameterExpression : SqlExpression
        {
            public SqlParameterExpression(Expression expression)
            {
                Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            }

            public override Type Type => Expression.Type;

            public Expression Expression { get; }
        }

        private static Expression FlattenProjection(
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

        private class ReaderParameterInjectingExpressionVisitor : ProjectionExpressionVisitor
        {
            public IDictionary<string, Expression> GatheredExpressions { get; private set; } = new Dictionary<string, Expression>();

            private static readonly TypeInfo dbDataReaderTypeInfo
                = typeof(DbDataReader).GetTypeInfo();

            private static readonly MethodInfo getFieldValueMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.GetFieldValue));

            private static readonly MethodInfo isDBNullMethodInfo
                = dbDataReaderTypeInfo.GetDeclaredMethod(nameof(DbDataReader.IsDBNull));

            private static readonly MethodInfo enumerableEmptyMethodInfo
                = ImpatientExtensions.GetGenericMethodDefinition((object o) => Enumerable.Empty<object>());

            private readonly IImpatientExpressionVisitorProvider expressionVisitorProvider;
            private readonly ParameterExpression readerParameter;
            private int readerIndex;
            private int subLeafIndex;
            private int topLevelIndex;

            protected bool InSubLeaf { get; private set; }

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

                    CurrentPath.Push($"${topLevelIndex}");

                    var result = Visit(node);

                    CurrentPath.Pop();

                    topLevelIndex++;

                    return result;
                }

                topLevelIndex++;

                return Visit(node);
            }

            private string ComputeCurrentName()
            {
                var parts
                    = CurrentPath
                        .Reverse()
                        .Where(n => !n.StartsWith("<>"));

                return string.Join(".", parts);
            }

            public override Expression Visit(Expression node)
            {
                if (node == null)
                {
                    return node;
                }

                switch (node)
                {
                    case NewExpression newExpression:
                    case MemberInitExpression memberInitExpression:
                    {
                        var subLeafIndex = this.subLeafIndex;
                        this.subLeafIndex = 0;

                        var visited = base.Visit(node);

                        this.subLeafIndex = subLeafIndex;

                        return visited;
                    }

                    case DefaultIfEmptyExpression defaultIfEmptyExpression:
                    {
                        CurrentPath.Push("$empty");
                        var name = ComputeCurrentName();
                        CurrentPath.Pop();

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
                            var descriptor = descriptors[i];
                            var materializer = descriptor.Materializer.ExpandParameters(row);
                            var test = descriptor.Test.ExpandParameters(materializer);

                            result = Expression.Condition(
                                test: test,
                                ifTrue: materializer,
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

                    case Expression expression
                    when expressionVisitorProvider
                        .TranslatabilityAnalyzingExpressionVisitor
                        .Visit(expression) is TranslatableExpression:
                    {
                        // TODO: SubLeaf may not apply anymore.
                        if (InSubLeaf)
                        {
                            CurrentPath.Push($"${++subLeafIndex}");
                            var name = ComputeCurrentName();
                            CurrentPath.Pop();
                            GatheredExpressions[name] = expression;
                        }
                        else
                        {
                            var name = ComputeCurrentName();
                            GatheredExpressions[name] = expression;
                        }

                        if (!expression.Type.IsScalarType())
                        {
                            var type = node.Type;
                            var defaultValue = Expression.Default(type) as Expression;

                            if (node.Type.FindGenericType(typeof(IEnumerable<>)) != null)
                            {
                                if (node.Type.IsArray)
                                {
                                    defaultValue = Expression.NewArrayInit(node.Type.GetElementType());
                                }
                                else if (node.Type.FindGenericType(typeof(List<>)) != null)
                                {
                                    defaultValue = Expression.New(type);
                                }
                                else
                                {
                                    type = node.Type.FindGenericType(typeof(IEnumerable<>));
                                    defaultValue = Expression.Call(enumerableEmptyMethodInfo.MakeGenericMethod(type.GetSequenceType()));
                                }
                            }

                            var currentIndex = readerIndex;
                            readerIndex++;

                            var result
                                = Expression.Condition(
                                    Expression.Call(
                                        readerParameter,
                                        isDBNullMethodInfo,
                                        Expression.Constant(currentIndex)),
                                    defaultValue,
                                    Expression.Call(
                                        ImpatientExtensions
                                            .GetGenericMethodDefinition((string s) => JsonConvert.DeserializeObject<object>(s))
                                            .MakeGenericMethod(type),
                                        Expression.Call(
                                            readerParameter,
                                            getFieldValueMethodInfo.MakeGenericMethod(typeof(string)),
                                            Expression.Constant(currentIndex)))) as Expression;

                            if (node.Type.FindGenericType(typeof(IQueryable<>)) != null)
                            {
                                result
                                    = Expression.Call(
                                        ImpatientExtensions
                                            .GetGenericMethodDefinition((IEnumerable<object> e) => e.AsQueryable())
                                            .MakeGenericMethod(type.GetSequenceType()),
                                        result);
                            }

                            return result;
                        }
                        else
                        {
                            var currentIndex = readerIndex;
                            readerIndex++;

                            return Expression.Condition(
                                Expression.Call(
                                    readerParameter,
                                    isDBNullMethodInfo,
                                    Expression.Constant(currentIndex)),
                                Expression.Default(node.Type),
                                Expression.Call(
                                    readerParameter,
                                    getFieldValueMethodInfo.MakeGenericMethod(node.Type),
                                    Expression.Constant(currentIndex)));
                        }
                    }

                    case Expression expression when InLeaf && !InSubLeaf:
                    {
                        InSubLeaf = true;
                        subLeafIndex = 0;

                        var visited = base.Visit(expression);

                        InSubLeaf = false;
                        subLeafIndex = 0;

                        return visited;
                    }

                    default:
                    {
                        return node;
                    }
                }
            }
        }

        private class DbCommandBuilderExpressionBuilder
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
                                    dbParameterVariable))));

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
    }
}
