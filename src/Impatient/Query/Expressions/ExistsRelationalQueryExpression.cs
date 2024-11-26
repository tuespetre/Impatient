using System.Linq.Expressions;

namespace Impatient.Query.Expressions;

public class ExistsRelationalQueryExpression : SingleValueRelationalQueryExpression
{
    public ExistsRelationalQueryExpression(SqlExistsExpression sqlInExpression)
        : this(new SelectExpression(new ServerProjectionExpression(sqlInExpression)))
    {
    }

    private ExistsRelationalQueryExpression(SelectExpression selectExpression) : base(selectExpression)
    {
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));

        if (selectExpression != SelectExpression)
        {
            if (selectExpression.Projection.ResultLambda.Body is SqlExistsExpression sqlInExpression)
            {
                return new ExistsRelationalQueryExpression(selectExpression);
            }
            else
            {
                return new SingleValueRelationalQueryExpression(selectExpression);
            }
        }

        return this;
    }
}