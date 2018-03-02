using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.ImpatientExtensions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class EnumerableContainsRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo enumerableContainsMethodInfo
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;

        public EnumerableContainsRewritingExpressionVisitor(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor)
        {
            this.translatabilityAnalyzingExpressionVisitor
                = translatabilityAnalyzingExpressionVisitor
                    ?? throw new ArgumentNullException(nameof(translatabilityAnalyzingExpressionVisitor));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.IsGenericMethod
                && node.Method.GetGenericMethodDefinition() == enumerableContainsMethodInfo
                && arguments[0].Type.GetSequenceType().IsScalarType()
                && translatabilityAnalyzingExpressionVisitor.Visit(arguments[1]) is TranslatableExpression)
            {
                var canUseValues = false;

                switch (arguments[0])
                {
                    case ConstantExpression constantExpression:
                    {
                        canUseValues = constantExpression.Value != null;
                        break;
                    }

                    case NewArrayExpression newArrayExpression:
                    {
                        canUseValues = true;
                        break;
                    }

                    case ListInitExpression listInitExpression:
                    {
                        canUseValues = listInitExpression.Initializers.All(i => i.Arguments.Count == 1);
                        break;
                    }

                    case Expression expression:
                    {
                        canUseValues = true;
                        break;
                    }
                }

                if (canUseValues)
                {
                    return new SqlInExpression(arguments[1], arguments[0]);
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
