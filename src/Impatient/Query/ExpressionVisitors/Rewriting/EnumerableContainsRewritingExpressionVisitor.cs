﻿using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class EnumerableContainsRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo enumerableContainsMethodInfo
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

        private static readonly MethodInfo queryableContainsMethodInfo
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Contains(null));

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.IsGenericMethod
                && (node.Method.GetGenericMethodDefinition() == enumerableContainsMethodInfo
                    || node.Method.GetGenericMethodDefinition() == queryableContainsMethodInfo))
            {
                // The separate ifs are here for breakpoint purposes.
                if (arguments[0].Type.GetSequenceType().IsScalarType())
                {
                    var canUseValues = false;

                    switch (arguments[0])
                    {
                        case ConstantExpression constantExpression:
                        {
                            canUseValues = constantExpression.Value is not null;
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
            }

            return node.Update(@object, arguments);
        }
    }
}
