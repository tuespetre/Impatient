using System.Linq.Expressions;

namespace Impatient.Query.Expressions;

public class ContainsRelationalQueryExpression : SingleValueRelationalQueryExpression
{
    public ContainsRelationalQueryExpression(SqlInExpression sqlInExpression)
        : this(new SelectExpression(new ServerProjectionExpression(sqlInExpression)))
    {
    }

    private ContainsRelationalQueryExpression(SelectExpression selectExpression) : base(selectExpression)
    {
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));

        if (selectExpression != SelectExpression)
        {
            if (selectExpression.Projection.ResultLambda.Body is SqlInExpression sqlInExpression)
            {
                return new ContainsRelationalQueryExpression(selectExpression);
            }
            else
            {
                return new SingleValueRelationalQueryExpression(selectExpression);
            }
        }

        return this;
    }
}
