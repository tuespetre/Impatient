using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class CollectionContainsRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            var method = node.Method;

            var collectionType = method.DeclaringType.FindGenericType(typeof(ICollection<>));

            if (collectionType != null && @object.Type.GetSequenceType().IsScalarType())
            {
                var canRewriteMethod = false;

                if (method.DeclaringType == collectionType)
                {
                    canRewriteMethod = true;
                }
                else
                {
                    var map = method.DeclaringType.GetTypeInfo().GetRuntimeInterfaceMap(collectionType);

                    var index = map.InterfaceMethods.ToList().FindIndex(m => m.Name == nameof(ICollection<int>.Contains));

                    canRewriteMethod = method == map.TargetMethods[index];
                }

                if (canRewriteMethod)
                {
                    var canUseValues = false;

                    switch (@object)
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
                        return new SqlInExpression(arguments[0], @object);
                    }
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
