using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class ServerProjectionExpression : ProjectionExpression
    {
        public ServerProjectionExpression(LambdaExpression resultLambda)
        {
            ResultLambda = resultLambda ?? throw new ArgumentNullException(nameof(resultLambda));
        }

        public ServerProjectionExpression(Expression resultLambdaBody)
        {
            if (resultLambdaBody == null)
            {
                throw new ArgumentNullException(nameof(resultLambdaBody));
            }

            ResultLambda = Lambda(resultLambdaBody);
        }

        public override LambdaExpression ResultLambda { get; }

        public override Type Type => ResultLambda.ReturnType;

        public override LambdaExpression Flatten() => ResultLambda;

        public override ProjectionExpression Merge(LambdaExpression lambda)
            => new ClientProjectionExpression(this, lambda);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var resultLambda = visitor.VisitAndConvert(ResultLambda, nameof(VisitChildren));

            if (resultLambda != ResultLambda)
            {
                return new ServerProjectionExpression(resultLambda);
            }

            return this;
        }
    }
}
