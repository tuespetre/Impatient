using Impatient.Query.Expressions;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Extensions
{
    public static class ExpressionExtensions
    {
        private static Expression ResolveSegment(Expression expression, string segment)
        {
            switch (expression)
            {
                case NewExpression newExpression:
                {
                    var match = newExpression.Members?.FirstOrDefault(m => m.GetPathSegmentName() == segment);

                    if (match != null)
                    {
                        return newExpression.Arguments[newExpression.Members.IndexOf(match)];
                    }

                    return null;
                }

                case MemberInitExpression memberInitExpression:
                {
                    var match = ResolveSegment(memberInitExpression.NewExpression, segment);

                    if (match != null)
                    {
                        return match;
                    }

                    match
                        = memberInitExpression.Bindings
                            .OfType<MemberAssignment>()
                            .Where(a => a.Member.GetPathSegmentName() == segment)
                            .Select(a => a.Expression)
                            .FirstOrDefault();

                    if (match != null)
                    {
                        return match;
                    }

                    return null;
                }

                case ExtraPropertiesExpression extraPropertiesExpression:
                {
                    for (var i = 0; i < extraPropertiesExpression.Names.Count; i++)
                    {
                        var name = extraPropertiesExpression.Names[i];

                        if (name.Equals(segment))
                        {
                            return extraPropertiesExpression.Properties[i];
                        }
                    }

                    return ResolveSegment(extraPropertiesExpression.Expression, segment);
                }

                case AnnotationExpression annotationExpression:
                {
                    return ResolveSegment(annotationExpression.Expression, segment);
                }

                case PolymorphicExpression polymorphicExpression:
                {
                    foreach (var descriptor in polymorphicExpression.Descriptors)
                    {
                        var expanded = descriptor.Materializer.ExpandParameters(polymorphicExpression.Row);

                        var resolved = ResolveSegment(expanded, segment);

                        if (resolved != null)
                        {
                            return resolved;
                        }
                    }

                    return null;
                }

                default:
                {
                    return null;
                }
            }
        }

        public static bool TryResolvePath(this Expression expression, string path, out Expression resolved)
        {
            resolved = expression;

            foreach (var segment in path.Split('.'))
            {
                var next = ResolveSegment(resolved, segment);

                if (next == null)
                {
                    resolved = null;
                    return false;
                }

                resolved = next;
            }

            return true;
        }
    }
}
