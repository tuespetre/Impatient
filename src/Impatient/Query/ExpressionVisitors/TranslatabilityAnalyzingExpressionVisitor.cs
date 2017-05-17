using Impatient.Query.Expressions;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class TranslatabilityAnalyzingExpressionVisitor : ExpressionVisitor
    {
        // TODO: Make this configurable
        private const bool complexNestedQueriesSupported = true;

        public TranslatabilityAnalyzingExpressionVisitor()
        {
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (Visit(node.Left) is TranslatableExpression 
                && Visit(node.Right) is TranslatableExpression)
            {
                switch (node.NodeType)
                {
                    // Logical
                    case ExpressionType.AndAlso:
                    case ExpressionType.OrElse:
                    {
                        return new TranslatableExpression(node);
                    }

                    // Comparison
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    {
                        return new TranslatableExpression(node);
                    }

                    // Math
                    case ExpressionType.Add:
                    case ExpressionType.Subtract:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                    {
                        return new TranslatableExpression(node);
                    }

                    // Other
                    case ExpressionType.Coalesce:
                    {
                        return new TranslatableExpression(node);
                    }
                }
            }

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            if (Visit(node.Test) is TranslatableExpression
                && Visit(node.IfTrue) is TranslatableExpression
                && Visit(node.IfFalse) is TranslatableExpression)
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
                case SqlExpression sql:
                {
                    return new TranslatableExpression(sql);
                }

                case SingleValueRelationalQueryExpression singleValueRelationalQuery:
                {
                    return new TranslatableExpression(singleValueRelationalQuery);
                }
                
                case ComplexNestedQueryExpression complexNestedQuery when complexNestedQueriesSupported:
                {
                    return new TranslatableExpression(node);
                }

                case EnumerableRelationalQueryExpression enumerableRelationalQuery when complexNestedQueriesSupported:
                {
                    return new TranslatableExpression(node);
                }

                case AnnotationExpression annotation:
                {
                    return Visit(annotation.Expression);
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

        protected override Expression VisitMember(MemberExpression node)
        {
            // TODO: Can this be replaced with checking for 'IsParameterizable' like in the translator?

            if (node.Type.IsScalarType())
            {
                var expression = node.Expression;

                while (expression is MemberExpression memberExpression)
                {
                    expression = memberExpression.Expression;
                }

                if (expression is ParameterExpression)
                {
                    return new TranslatableExpression(node);
                }
            }

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            if (Visit(node.NewExpression) is TranslatableExpression translatable
                && node.Bindings.All(b => b.IsTranslatable()))
            {
                return new TranslatableExpression(node);
            }

            return node;
        }
    }
}
