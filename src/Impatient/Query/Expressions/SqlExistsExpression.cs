using System;

namespace Impatient.Query.Expressions
{
    public class SqlExistsExpression : SqlExpression
    {
        public SqlExistsExpression(SelectExpression selectExpression)
        {
            SelectExpression = selectExpression ?? throw new ArgumentNullException(nameof(selectExpression));
            Type = typeof(bool);
        }

        public SelectExpression SelectExpression { get; }

        public override Type Type { get; }
    }
}
