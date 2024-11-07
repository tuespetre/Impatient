using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions;

public class AsSplitQueryExpression : AnnotationExpression
{
    public AsSplitQueryExpression(Expression expression) : base(expression)
    {
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var expression = visitor.Visit(Expression);

        if (expression != Expression)
        {
            return new AsSplitQueryExpression(expression);
        }

        return this;
    }
}
