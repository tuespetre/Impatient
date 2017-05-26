using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class ClientProjectionExpression : ProjectionExpression
    {
        public ClientProjectionExpression(ServerProjectionExpression serverProjection, LambdaExpression resultLambda)
        {
            ServerProjection = serverProjection ?? throw new ArgumentNullException(nameof(serverProjection));
            ResultLambda = resultLambda ?? throw new ArgumentNullException(nameof(resultLambda));
        }

        public ServerProjectionExpression ServerProjection { get; }

        public override LambdaExpression ResultLambda { get; }

        public override Type Type => ResultLambda.ReturnType;

        public override LambdaExpression Flatten()
            => Lambda(ResultLambda
                .ExpandParameters(ServerProjection.Flatten().Body));

        public override ProjectionExpression Merge(LambdaExpression lambda)
            => new ClientProjectionExpression(
                ServerProjection,
                Lambda(
                    lambda.ExpandParameters(ResultLambda.Body),
                    ResultLambda.Parameters));

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var serverProjection = visitor.VisitAndConvert(ServerProjection, nameof(VisitChildren));
            var resultLambda = visitor.VisitAndConvert(ResultLambda, nameof(VisitChildren));

            if (serverProjection != ServerProjection || resultLambda != ResultLambda)
            {
                return new ClientProjectionExpression(serverProjection, resultLambda);
            }

            return this;
        }
    }
}
