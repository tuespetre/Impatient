﻿using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SelectExpression : Expression, ISemanticHashCodeProvider
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
                  grouping: null,
                  isWindowed: false)
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
            Expression grouping,
            bool isWindowed)
        {
            Projection = projection ?? throw new ArgumentNullException(nameof(projection));
            Table = table;
            Predicate = predicate;
            OrderBy = orderBy;
            Offset = offset;
            Limit = limit;
            IsDistinct = isDistinct;
            Grouping = grouping;
            IsWindowed = isWindowed;
        }

        public ProjectionExpression Projection { get; }

        public TableExpression Table { get; }

        public Expression Predicate { get; }

        public OrderByExpression OrderBy { get; }

        public Expression Offset { get; }

        public Expression Limit { get; }

        public bool IsDistinct { get; }

        public Expression Grouping { get; }

        public bool IsWindowed { get; }

        #region properties for convenience of condition checking

        public bool HasPredicate => Predicate is not null;

        public bool HasOffset => Offset is not null;

        public bool HasLimit => Limit is not null;

        public bool HasOffsetOrLimit => HasOffset || HasLimit;

        public bool HasGrouping => Grouping is not null;

        public bool HasOrdering => OrderBy is not null;

        #endregion

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
                var oldTables = Table.Flatten().ToArray();
                var newTables = table.Flatten().ToArray();

                var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                projection = updater.VisitAndConvert(projection, nameof(VisitChildren));
                predicate = updater.VisitAndConvert(predicate, nameof(VisitChildren));
                orderBy = updater.VisitAndConvert(orderBy, nameof(VisitChildren));
                offset = updater.VisitAndConvert(offset, nameof(VisitChildren));
                limit = updater.VisitAndConvert(limit, nameof(VisitChildren));
                grouping = updater.VisitAndConvert(grouping, nameof(VisitChildren));
            }

            if (projection != Projection
                || table != Table
                || predicate != Predicate
                || orderBy != OrderBy
                || offset != Offset
                || limit != Limit
                || grouping != Grouping)
            {
                return new SelectExpression(projection, table, predicate, orderBy, offset, limit, IsDistinct, grouping, IsWindowed);
            }

            return this;
        }

        #endregion

        public bool RequiresPushdownForLeftSideOfJoin()
        {
            return HasOffsetOrLimit || HasGrouping || IsDistinct || IsWindowed;
        }

        public bool RequiresPushdownForRightSideOfJoin()
        {
            return RequiresPushdownForLeftSideOfJoin() || !(Table is AliasedTableExpression) || HasPredicate || HasOrdering;
        }

        public bool RequiresPushdownForPredicate()
        {
            return HasOffsetOrLimit || HasGrouping || IsDistinct || IsWindowed;
        }

        public bool RequiresPushdownForLimit()
        {
            return HasLimit || IsWindowed || IsDistinct;
        }

        public bool RequiresPushdownForOffset()
        {
            return HasOffsetOrLimit || IsDistinct || IsWindowed;
        }

        public bool RequiresPushdownForGrouping()
        {
            return HasOffsetOrLimit || HasGrouping || IsDistinct || IsWindowed;
        }

        public bool RequiresPushdownForAggregate()
        {
            return HasOffsetOrLimit || HasGrouping || IsDistinct || IsWindowed;
        }

        public SelectExpression UpdateProjection(ProjectionExpression projection)
        {
            return new SelectExpression(projection, Table, Predicate, OrderBy, Offset, Limit, IsDistinct, Grouping, IsWindowed);
        }

        public SelectExpression UpdateTable(TableExpression table)
        {
            return new SelectExpression(Projection, table, Predicate, OrderBy, Offset, Limit, IsDistinct, Grouping, IsWindowed);
        }

        public SelectExpression AddToPredicate(Expression predicate)
        {
            predicate = Predicate is null
                ? predicate
                : AndAlso(Predicate, predicate);

            return new SelectExpression(Projection, Table, predicate, OrderBy, Offset, Limit, IsDistinct, Grouping, IsWindowed);
        }

        public SelectExpression RemovePredicate()
        {
            return new SelectExpression(Projection, Table, null, OrderBy, Offset, Limit, IsDistinct, Grouping, IsWindowed);
        }

        public SelectExpression AddToOrderBy(Expression expression, bool descending)
        {
            var orderBy
                = OrderBy is null
                    ? new OrderByExpression(expression, descending)
                    : new ThenOrderByExpression(OrderBy, expression, descending);

            return UpdateOrderBy(orderBy);
        }

        public SelectExpression UpdateOrderBy(OrderByExpression orderBy)
        {
            return new SelectExpression(Projection, Table, Predicate, orderBy, Offset, Limit, IsDistinct, Grouping, IsWindowed);
        }

        public SelectExpression UpdateLimit(Expression limit)
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, Offset, limit, IsDistinct, Grouping, IsWindowed);
        }

        public SelectExpression UpdateOffset(Expression offset)
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, offset, Limit, IsDistinct, Grouping, IsWindowed);
        }

        public SelectExpression AsDistinct()
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, Offset, Limit, true, Grouping, IsWindowed);
        }

        public SelectExpression UpdateGrouping(Expression grouping)
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, Offset, Limit, IsDistinct, grouping, IsWindowed);
        }

        public SelectExpression AsWindowed()
        {
            return new SelectExpression(Projection, Table, Predicate, OrderBy, Offset, Limit, IsDistinct, Grouping, true);
        }

        public int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = comparer.GetHashCode(Projection);
                
                hash = (hash * 16777619) ^ comparer.GetHashCode(Table);
                hash = (hash * 16777619) ^ comparer.GetHashCode(Predicate);
                hash = (hash * 16777619) ^ comparer.GetHashCode(OrderBy);
                hash = (hash * 16777619) ^ comparer.GetHashCode(Offset);
                hash = (hash * 16777619) ^ comparer.GetHashCode(Limit);
                hash = (hash * 16777619) ^ comparer.GetHashCode(Grouping);
                hash = (hash * 16777619) ^ IsDistinct.GetHashCode();
                hash = (hash * 16777619) ^ IsWindowed.GetHashCode();

                return hash;
            }
        }
    }
}
