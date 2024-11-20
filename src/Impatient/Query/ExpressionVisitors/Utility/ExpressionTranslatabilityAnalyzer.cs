using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility;

public class ExpressionTranslatabilityAnalyzer
{
    private readonly IQueryFormattingProvider queryFormattingProvider;

    public ExpressionTranslatabilityAnalyzer(IQueryFormattingProvider queryFormattingProvider)
    {
        this.queryFormattingProvider = queryFormattingProvider ?? throw new System.ArgumentNullException(nameof(queryFormattingProvider));
    }

    public virtual bool CanTranslate(Expression node) => node switch
    {
        // standard expressions

        BinaryExpression x => CanTranslate(x),
        ConditionalExpression x => CanTranslate(x),
        ConstantExpression x => CanTranslate(x),
        MemberInitExpression x => CanTranslate(x),
        NewExpression x => CanTranslate(x),
        NewArrayExpression x => CanTranslate(x),
        UnaryExpression x => CanTranslate(x),

        // extension expressions

        // TODO: these four arms should become obsolete with the right infrastructure changes
        // (SqlExpressions should only be constructed with guaranteed-translatable subexpressions)
        SqlCastExpression x => CanTranslate(x.Expression),
        SqlConcatExpression x => x.Segments.All(CanTranslate),
        SqlFunctionExpression x => x.Arguments.All(CanTranslate),
        SqlInExpression x => CanTranslate(x.Value),

        SqlExpression => true,
        PolymorphicExpression => true,
        ExtendedMemberInitExpression => true,
        ExtendedNewExpression => true,
        LateBoundProjectionLeafExpression => true,

        AnnotationExpression x => CanTranslate(x.Expression),
        ExtraPropertiesExpression x => CanTranslate(x.Expression),

        GroupByResultExpression x =>
            queryFormattingProvider.SupportsComplexTypeSubqueries
            && x.SelectExpression.Projection is ServerProjectionExpression,

        EnumerableRelationalQueryExpression x =>
            queryFormattingProvider.SupportsComplexTypeSubqueries
            && x.SelectExpression.Projection is ServerProjectionExpression,

        SingleValueRelationalQueryExpression x =>
            (queryFormattingProvider.SupportsComplexTypeSubqueries || x.Type.IsScalarType())
            && x.SelectExpression.Projection is ServerProjectionExpression,

        _ => false,
    };

    protected virtual bool CanTranslate(MemberBinding binding) => binding switch
    {
        MemberAssignment x => CanTranslate(x.Expression),
        MemberMemberBinding x => x.Bindings.All(CanTranslate),
        _ => false,
    };

    protected virtual bool CanTranslate(BinaryExpression node)
    {
        if (!CanTranslate(node.Left) || !CanTranslate(node.Right))
        {
            return false;
        }

        switch (node.NodeType)
        {
            // Equality
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            {
                return true;
            }

            // Math
            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Modulo:
            case ExpressionType.Power:
            {
                return node.Left.Type.IsScalarType()
                    && node.Right.Type.IsScalarType()
                    && !node.Left.Type.IsTimeType()
                    && !node.Right.Type.IsTimeType();
            }

            // Comparison
            case ExpressionType.GreaterThan:
            case ExpressionType.GreaterThanOrEqual:
            case ExpressionType.LessThan:
            case ExpressionType.LessThanOrEqual:
            // Logical
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
            // Bitwise
            case ExpressionType.And:
            case ExpressionType.Or:
            case ExpressionType.ExclusiveOr:
            // Other
            case ExpressionType.Coalesce:
            {
                return node.Left.Type.IsScalarType()
                    && node.Right.Type.IsScalarType();
            }

            default:
            {
                return false;
            }
        }
    }

    protected virtual bool CanTranslate(ConditionalExpression node)
    {
        return CanTranslate(node.Test)
            && CanTranslate(node.IfTrue)
            && CanTranslate(node.IfFalse)
            && node.IfTrue.Type.IsScalarType()
            && node.IfFalse.Type.IsScalarType();
    }

    protected virtual bool CanTranslate(ConstantExpression node)
    {
        return node.Type.IsScalarType() || node.Value is null;
    }

    protected virtual bool CanTranslate(MemberInitExpression node)
    {
        return CanTranslate(node.NewExpression)
            && node.Bindings.All(CanTranslate);
    }

    protected virtual bool CanTranslate(NewExpression node)
    {
        return node.Members is null
            ? node.Arguments.Count is 0
            : node.Arguments.All(CanTranslate);
    }

    protected virtual bool CanTranslate(NewArrayExpression node)
    {
        return node.Expressions.All(CanTranslate);
    }

    protected virtual bool CanTranslate(UnaryExpression node)
    {
        switch (node.NodeType)
        {
            case ExpressionType.Negate:
            case ExpressionType.NegateChecked:
            case ExpressionType.Not:
            case ExpressionType.Convert:
            {
                return CanTranslate(node.Operand);
            }

            default:
            {
                return false;
            }
        }
    }
}
