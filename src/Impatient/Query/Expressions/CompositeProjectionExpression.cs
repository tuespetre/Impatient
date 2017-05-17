using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class CompositeProjectionExpression : ProjectionExpression
    {
        public CompositeProjectionExpression(
            ProjectionExpression outerProjection,
            ProjectionExpression innerProjection,
            LambdaExpression resultLambda)
        {
            OuterProjection = outerProjection ?? throw new ArgumentNullException(nameof(outerProjection));
            InnerProjection = innerProjection ?? throw new ArgumentNullException(nameof(innerProjection));
            ResultLambda = resultLambda ?? throw new ArgumentNullException(nameof(resultLambda));
        }

        public ProjectionExpression OuterProjection { get; }

        public ProjectionExpression InnerProjection { get; }

        public override LambdaExpression ResultLambda { get; }

        public override Type Type => ResultLambda.ReturnType;

        public override LambdaExpression Flatten()
            => Lambda(ResultLambda
                .ExpandParameters(
                    OuterProjection.Flatten().Body,
                    InnerProjection.Flatten().Body));

        public override ProjectionExpression Merge(LambdaExpression lambda)
            => new CompositeProjectionExpression(
                OuterProjection,
                InnerProjection,
                Lambda(
                    lambda.ExpandParameters(ResultLambda.Body),
                    ResultLambda.Parameters));

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerProjection = visitor.VisitAndConvert(OuterProjection, nameof(VisitChildren));
            var innerProjection = visitor.VisitAndConvert(InnerProjection, nameof(VisitChildren));
            var resultLambda = visitor.VisitAndConvert(ResultLambda, nameof(VisitChildren));

            if (outerProjection != OuterProjection
                || innerProjection != InnerProjection
                || resultLambda != ResultLambda)
            {
                return new CompositeProjectionExpression(outerProjection, innerProjection, resultLambda);
            }

            return this;
        }
    }
}
