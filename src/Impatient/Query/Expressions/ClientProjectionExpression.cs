using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class ClientProjectionExpression : ProjectionExpression
    {
        public ClientProjectionExpression(LambdaExpression serverLambda, LambdaExpression resultLambda)
        {
            ServerLambda = serverLambda ?? throw new ArgumentNullException(nameof(serverLambda));
            ResultLambda = resultLambda ?? throw new ArgumentNullException(nameof(resultLambda));
        }

        public LambdaExpression ServerLambda { get; }

        public override LambdaExpression ResultLambda { get; }

        public override Type Type => ResultLambda.ReturnType;

        public override LambdaExpression Flatten()
            => Lambda(ResultLambda
                .ExpandParameters(ServerLambda.Body));

        public override ProjectionExpression Merge(LambdaExpression lambda)
            => new ClientProjectionExpression(
                ServerLambda,
                Lambda(
                    lambda.ExpandParameters(ResultLambda.Body),
                    ResultLambda.Parameters));

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var serverLambda = visitor.VisitAndConvert(ServerLambda, nameof(VisitChildren));
            var resultLambda = visitor.VisitAndConvert(ResultLambda, nameof(VisitChildren));

            if (serverLambda != ServerLambda || resultLambda != ResultLambda)
            {
                return new ClientProjectionExpression(serverLambda, resultLambda);
            }

            return this;
        }
    }
}
