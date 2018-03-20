using Impatient.Query.Expressions;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class TranslatabilityAnalyzingExpressionVisitor : ExpressionVisitor
    {
        public TranslatabilityAnalyzingExpressionVisitor()
        {
        }

        public virtual bool ComplexNestedQueriesSupported => true;

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (Visit(node.Left) is TranslatableExpression t1
                && Visit(node.Right) is TranslatableExpression t2)
            {
                switch (node.NodeType)
                {
                    // Equality
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    {
                        return new TranslatableExpression(node);
                    }

                    // Comparison
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    // Logical
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                    // Math
                    case ExpressionType.Add:
                    case ExpressionType.Subtract:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    // Bitwise
                    case ExpressionType.And:
                    case ExpressionType.Or:
                    case ExpressionType.ExclusiveOr:
                    // Other
                    case ExpressionType.Coalesce:
                    {
                        if (t1.Type.IsScalarType() && t2.Type.IsScalarType())
                        {
                            return new TranslatableExpression(node);
                        }

                        break;
                    }
                }
            }

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if (Visit(node.Test) is TranslatableExpression
                && Visit(node.IfTrue) is TranslatableExpression t1
                && Visit(node.IfFalse) is TranslatableExpression t2
                && t1.Type.IsScalarType()
                && t2.Type.IsScalarType())
            {
                return new TranslatableExpression(node);
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type.IsScalarType())
            {
                return new TranslatableExpression(node);
            }

            return node;
        }

        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SqlExpression _:
                {
                    return new TranslatableExpression(node);
                }

                case PolymorphicExpression _:
                {
                    return new TranslatableExpression(node);
                }

                case GroupByResultExpression _
                when ComplexNestedQueriesSupported:
                {
                    return new TranslatableExpression(node);
                }

                case GroupedRelationalQueryExpression _
                when ComplexNestedQueriesSupported:
                {
                    return new TranslatableExpression(node);
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression
                when singleValueRelationalQueryExpression.Type.IsScalarType() || ComplexNestedQueriesSupported:
                {
                    return new TranslatableExpression(node);
                }

                case EnumerableRelationalQueryExpression _
                when ComplexNestedQueriesSupported:
                {
                    return new TranslatableExpression(node);
                }

                case AnnotationExpression annotationExpression:
                {
                    return Visit(annotationExpression.Expression);
                }
            }

            return node;
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Members == null)
            {
                if (node.Arguments.Count == 0)
                {
                    return new TranslatableExpression(node);
                }
            }
            else if (node.Arguments.All(a => Visit(a) is TranslatableExpression))
            {
                return new TranslatableExpression(node);
            }

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (Visit(node.NewExpression) is TranslatableExpression
                && node.Bindings.All(IsTranslatable))
            {
                return new TranslatableExpression(node);
            }

            return node;
        }

        private bool IsTranslatable(MemberBinding memberBinding)
        {
            switch (memberBinding)
            {
                case MemberAssignment memberAssignment:
                {
                    return Visit(memberAssignment.Expression) is TranslatableExpression;
                }

                case MemberMemberBinding memberMemberBinding:
                {
                    return memberMemberBinding.Bindings.All(IsTranslatable);
                }

                default:
                {
                    return false;
                }
            }
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                case ExpressionType.Convert:
                {
                    if (Visit(node.Operand) is TranslatableExpression)
                    {
                        return new TranslatableExpression(node);
                    }

                    goto default;
                }

                default:
                {
                    return node;
                }
            }
        }
    }
}
