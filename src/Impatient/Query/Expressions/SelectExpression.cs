using Impatient.Query.ExpressionVisitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SelectExpression : Expression
    {
        public SelectExpression(ProjectionExpression projection) : this(projection, null)
        {
        }

        public SelectExpression(
            ProjectionExpression projection,
            TableExpression table)
            : this(
                  projection ?? throw new ArgumentNullException(nameof(projection)),
                  table,
                  predicate: null,
                  orderBy: null,
                  offset: null,
                  limit: null,
                  isDistinct: false,
                  grouping: null)
        {
        }

        private SelectExpression(
            ProjectionExpression projection,
            TableExpression table,
            Expression predicate,
            OrderByExpression orderBy,
            Expression offset,
            Expression limit,
            bool isDistinct,
            Expression grouping)
        {
            Projection = projection ?? throw new ArgumentNullException(nameof(projection));
            Table = table;
            Predicate = predicate;
            OrderBy = orderBy;
            Offset = offset;
            Limit = limit;
            IsDistinct = isDistinct;
            Grouping = grouping;
        }
        
        public ProjectionExpression Projection { get; }

        public TableExpression Table { get; }

        public Expression Predicate { get; }

        public OrderByExpression OrderBy { get; }

        public Expression Offset { get; }

        public Expression Limit { get; }

        public bool IsDistinct { get; }

        public Expression Grouping { get; }

        #region Expression overrides

        public override Type Type => Projection.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var projection = visitor.VisitAndConvert(Projection, nameof(VisitChildren));
            var table = visitor.VisitAndConvert(Table, nameof(VisitChildren));
            var predicate = visitor.VisitAndConvert(Predicate, nameof(VisitChildren));
            var orderBy = visitor.VisitAndConvert(OrderBy, nameof(VisitChildren));
            var offset = visitor.VisitAndConvert(Offset, nameof(VisitChildren));
            var limit = visitor.VisitAndConvert(Limit, nameof(VisitChildren));
            var grouping = visitor.VisitAndConvert(Grouping, nameof(VisitChildren));

            if (table != Table)
            {
                var oldTables = Table.Flatten().Cast<Expression>();
                var newTables = table.Flatten().Cast<Expression>();

                var replacingVisitor = new ExpressionReplacingExpressionVisitor(oldTables.Zip(newTables, ValueTuple.Create));

                projection = replacingVisitor.VisitAndConvert(projection, nameof(VisitChildren));
                predicate = replacingVisitor.VisitAndConvert(predicate, nameof(VisitChildren));
                orderBy = replacingVisitor.VisitAndConvert(orderBy, nameof(VisitChildren));
                offset = replacingVisitor.VisitAndConvert(offset, nameof(VisitChildren));
                limit = replacingVisitor.VisitAndConvert(limit, nameof(VisitChildren));
                grouping = replacingVisitor.VisitAndConvert(grouping, nameof(VisitChildren));
            }

            if (projection != Projection
                || table != Table
                || predicate != Predicate
                || orderBy != OrderBy
                || offset != Offset
                || limit != Limit
                || grouping != Grouping)
            {
                return new SelectExpression(projection, table, predicate, orderBy, offset, limit, IsDistinct, grouping);
            }

            return this;
        }

        #endregion
        public bool RequiresPushdownForLeftSideOfJoin()
        {
            return Offset != null
                || Limit != null
                || IsDistinct
                || Grouping != null;
        }

        public bool RequiresPushdownForRightSideOfJoin()
        {
            return RequiresPushdownForLeftSideOfJoin()
                || !(Table is AliasedTableExpression)
                || Predicate != null;
        }

        public SelectExpression UpdateProjection(ProjectionExpression projection)
        {
            return new SelectExpression(projection, Table, Predicate, OrderBy, Offset, Limit, IsDistinct, Grouping);
        }

        public SelectExpression UpdateTable(TableExpression table)
        {
            return new SelectExpression(Projection, table, Predicate, OrderBy, Offset, Limit, IsDistinct, Grouping);
        }

        public SelectExpression AddToPredicate(Expression predicate)
        {
            predicate = Predicate == null
                ? predicate
                : AndAlso(Predicate, predicate);

            return new SelectExpression(Projection, Table, predicate, OrderBy, Offset, Limit, IsDistinct, Grouping);
        }

        public SelectExpression RemovePredicate()
        {
            return new SelectExpression(Projection, Table, null, OrderBy, Offset, Limit, IsDistinct, Grouping);
        }

        public SelectExpression AddToOrderBy(Expression expression, bool descending)
        {
            var orderBy
                = OrderBy == null
                    ? new OrderByExpression(expression, descending)
                    : new ThenOrderByExpression(OrderBy, expression, descending);

            return UpdateOrderBy(orderBy);
        }

        public SelectExpression UpdateOrderBy(OrderByExpression orderBy)
        {
            return new SelectExpression(Projection, Table, Predicate, orderBy, Offset, Limit, IsDistinct, Grouping);
        }

        public SelectExpression UpdateLimit(Expression limit)
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, Offset, limit, IsDistinct, Grouping);
        }

        public SelectExpression UpdateOffset(Expression offset)
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, offset, Limit, IsDistinct, Grouping);
        }

        public SelectExpression AsDistinct()
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, Offset, Limit, true, Grouping);
        }

        public SelectExpression UpdateGrouping(Expression grouping)
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, Offset, Limit, IsDistinct, grouping);
        }
    }
}
