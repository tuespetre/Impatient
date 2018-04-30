using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Impatient.Query.Infrastructure
{
    /* An equality comparer for expressions... instead of the HashingExpressionVisitor, 
     * this turned out to be a better idea for performance, as the EF Core team must have 
     * discovered or known. Reason being that there is enough overhead in all of the virtual calls
     * to the Visit* methods to make a noticeable difference (in benchmarks, anyways.)
     */
    public class ExpressionEqualityComparer : IEqualityComparer<Expression>
    {
        public static readonly ExpressionEqualityComparer Instance = new ExpressionEqualityComparer();

        public bool Equals(Expression x, Expression y)
        {
            // This should be expanded into recursive calls to Equals
            // but right not it is not a priority as this is not used in
            // a hot path.
            return GetHashCode(x).Equals(GetHashCode(y));
        }

        public int GetHashCode(Expression node)
        {
            if (node == null)
            {
                return 0;
            }

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.AddChecked:
                case ExpressionType.SubtractChecked:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Power:
                case ExpressionType.Assign:
                case ExpressionType.AddAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.PowerAssign:
                {
                    return GetHashCode((BinaryExpression)node);
                }

                case ExpressionType.Block:
                {
                    return GetHashCode((BlockExpression)node);
                }

                case ExpressionType.Conditional:
                {
                    return GetHashCode((ConditionalExpression)node);
                }

                case ExpressionType.Constant:
                {
                    return GetHashCode((ConstantExpression)node);
                }

                case ExpressionType.DebugInfo:
                {
                    return GetHashCode((DebugInfoExpression)node);
                }

                case ExpressionType.Default:
                {
                    return GetHashCode((DefaultExpression)node);
                }

                case ExpressionType.Goto:
                {
                    return GetHashCode((GotoExpression)node);
                }

                case ExpressionType.Index:
                {
                    return GetHashCode((IndexExpression)node);
                }

                case ExpressionType.Invoke:
                {
                    return GetHashCode((InvocationExpression)node);
                }

                case ExpressionType.Label:
                {
                    return GetHashCode((LabelExpression)node);
                }

                case ExpressionType.Lambda:
                {
                    return GetHashCode((LambdaExpression)node);
                }

                case ExpressionType.ListInit:
                {
                    return GetHashCode((ListInitExpression)node);
                }

                case ExpressionType.Loop:
                {
                    return GetHashCode((LoopExpression)node);
                }

                case ExpressionType.MemberAccess:
                {
                    return GetHashCode((MemberExpression)node);
                }

                case ExpressionType.MemberInit:
                {
                    return GetHashCode((MemberInitExpression)node);
                }

                case ExpressionType.Call:
                {
                    return GetHashCode((MethodCallExpression)node);
                }

                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                {
                    return GetHashCode((NewArrayExpression)node);
                }

                case ExpressionType.New:
                {
                    return GetHashCode((NewExpression)node);
                }

                case ExpressionType.Parameter:
                {
                    return GetHashCode((ParameterExpression)node);
                }

                case ExpressionType.RuntimeVariables:
                {
                    return GetHashCode((RuntimeVariablesExpression)node);
                }

                case ExpressionType.Switch:
                {
                    return GetHashCode((SwitchExpression)node);
                }

                case ExpressionType.Try:
                {
                    return GetHashCode((TryExpression)node);
                }

                case ExpressionType.TypeEqual:
                case ExpressionType.TypeIs:
                {
                    return GetHashCode((TypeBinaryExpression)node);
                }

                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.Not:
                case ExpressionType.Quote:
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.OnesComplement:
                case ExpressionType.ArrayLength:
                case ExpressionType.Throw:
                case ExpressionType.TypeAs:
                case ExpressionType.UnaryPlus:
                case ExpressionType.Unbox:
                case ExpressionType.Increment:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.Decrement:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostDecrementAssign:
                {
                    return GetHashCode((UnaryExpression)node);
                }

                case ExpressionType.Extension:
                {
                    var hash = StartHashCode(node);

                    if (node is ISemanticHashCodeProvider semanticallyHashable)
                    {
                        hash = Combine(hash, semanticallyHashable.GetSemanticHashCode(this));
                    }

                    return hash;
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(BinaryExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Left));
            hash = Combine(hash, GetHashCode(node.Right));
            hash = Combine(hash, node.IsLifted.GetHashCode());
            hash = Combine(hash, node.IsLiftedToNull.GetHashCode());
            hash = Combine(hash, node.Method?.GetHashCode() ?? 0);
            //hash = Combine(hash, GetHashCode(node.Conversion));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(BlockExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Variables));
            hash = Combine(hash, GetHashCode(node.Expressions));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(CatchBlock node)
        {
            var hash = node.Test.GetHashCode();

            hash = Combine(hash, GetHashCode(node.Filter));
            hash = Combine(hash, GetHashCode(node.Variable));
            hash = Combine(hash, GetHashCode(node.Body));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ConditionalExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Test));
            hash = Combine(hash, GetHashCode(node.IfTrue));
            hash = Combine(hash, GetHashCode(node.IfFalse));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ConstantExpression node)
        {
            var hash = StartHashCode(node);

            if (node.Value is IQueryable queryable && !(queryable is EnumerableQuery))
            {
                hash = Combine(hash, queryable.ElementType.GetHashCode());
                hash = Combine(hash, queryable.Provider.GetType().GetHashCode());
            }
            else
            {
                hash = Combine(hash, node.Value?.GetHashCode() ?? 0);
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(DebugInfoExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, node.Document.DocumentType.GetHashCode());
            hash = Combine(hash, node.Document.FileName?.GetHashCode() ?? 0);
            hash = Combine(hash, node.Document.Language.GetHashCode());
            hash = Combine(hash, node.Document.LanguageVendor.GetHashCode());
            hash = Combine(hash, node.EndColumn);
            hash = Combine(hash, node.EndLine);
            hash = Combine(hash, node.IsClear.GetHashCode());
            hash = Combine(hash, node.StartColumn);
            hash = Combine(hash, node.StartLine);

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(DefaultExpression node)
        {
            return StartHashCode(node);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ElementInit node)
        {
            var hash = node.AddMethod.GetHashCode();

            hash = Combine(hash, GetHashCode(node.Arguments));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(GotoExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, node.Kind.GetHashCode());
            hash = Combine(hash, GetHashCode(node.Target));
            hash = Combine(hash, GetHashCode(node.Value));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(IndexExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, node.Indexer?.GetHashCode() ?? 0);
            hash = Combine(hash, GetHashCode(node.Object));
            hash = Combine(hash, GetHashCode(node.Arguments));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(InvocationExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Expression));
            hash = Combine(hash, GetHashCode(node.Arguments));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(LabelExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Target));
            hash = Combine(hash, GetHashCode(node.DefaultValue));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(LabelTarget node)
        {
            var hash = node.Type.GetHashCode();

            hash = Combine(hash, node.Name.GetHashCode());

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(LambdaExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Body));
            hash = Combine(hash, GetHashCode(node.Parameters));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ListInitExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.NewExpression));
            hash = Combine(hash, GetHashCode(node.Initializers));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(LoopExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Body));
            hash = Combine(hash, GetHashCode(node.BreakLabel));
            hash = Combine(hash, GetHashCode(node.ContinueLabel));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(MemberBinding node)
        {
            var hash = node.BindingType.GetHashCode();

            hash = Combine(hash, node.Member.GetHashCode());

            switch (node)
            {
                case MemberAssignment typed:
                {
                    hash = Combine(hash, GetHashCode(typed.Expression));

                    break;
                }

                case MemberListBinding typed:
                {
                    hash = Combine(hash, GetHashCode(typed.Initializers));

                    break;
                }

                case MemberMemberBinding typed:
                {
                    hash = Combine(hash, GetHashCode(typed.Bindings));

                    break;
                }
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(MemberExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Expression));
            hash = Combine(hash, node.Member.GetHashCode());

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(MemberInitExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.NewExpression));
            hash = Combine(hash, GetHashCode(node.Bindings));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(MethodCallExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Object));
            hash = Combine(hash, GetHashCode(node.Arguments));
            hash = Combine(hash, node.Method.GetHashCode());

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(NewArrayExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Expressions));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(NewExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Arguments));
            hash = Combine(hash, node.Constructor?.GetHashCode() ?? 0);

            if (node.Members != null)
            {
                for (var i = 0; i < node.Members.Count; i++)
                {
                    hash = Combine(hash, node.Members[i].GetHashCode());
                }
            }
            else
            {
                hash = Combine(hash, 0);
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ParameterExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, node.Type.GetHashCode());
            hash = Combine(hash, node.IsByRef.GetHashCode());
            hash = Combine(hash, node.Name?.GetHashCode() ?? 0);

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(RuntimeVariablesExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Variables));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(SwitchCase node)
        {
            var hash = GetHashCode(node.Body);

            hash = Combine(hash, GetHashCode(node.TestValues));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(SwitchExpression node)
        {
            var hash = StartHashCode(node);
            
            hash = Combine(hash, GetHashCode(node.Cases));
            hash = Combine(hash, GetHashCode(node.DefaultBody));
            hash = Combine(hash, GetHashCode(node.SwitchValue));
            hash = Combine(hash, node.Comparison.GetHashCode());

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(TryExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Body));
            hash = Combine(hash, GetHashCode(node.Fault));
            hash = Combine(hash, GetHashCode(node.Handlers));
            hash = Combine(hash, GetHashCode(node.Finally));

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(TypeBinaryExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Expression));
            hash = Combine(hash, node.TypeOperand.GetHashCode());

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(UnaryExpression node)
        {
            var hash = StartHashCode(node);

            hash = Combine(hash, GetHashCode(node.Operand));
            hash = Combine(hash, node.IsLifted.GetHashCode());
            hash = Combine(hash, node.IsLiftedToNull.GetHashCode());
            hash = Combine(hash, node.Method?.GetHashCode() ?? 0);

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ReadOnlyCollection<MemberBinding> expressions)
        {
            if (expressions.Count == 0)
            {
                return 0;
            }

            var hash = GetHashCode(expressions[0]);

            for (var i = 1; i < expressions.Count; i++)
            {
                hash = Combine(hash, GetHashCode(expressions[i]));
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ReadOnlyCollection<CatchBlock> expressions)
        {
            if (expressions.Count == 0)
            {
                return 0;
            }

            var hash = GetHashCode(expressions[0]);

            for (var i = 1; i < expressions.Count; i++)
            {
                hash = Combine(hash, GetHashCode(expressions[i]));
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ReadOnlyCollection<ElementInit> expressions)
        {
            if (expressions.Count == 0)
            {
                return 0;
            }

            var hash = GetHashCode(expressions[0]);

            for (var i = 1; i < expressions.Count; i++)
            {
                hash = Combine(hash, GetHashCode(expressions[i]));
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ReadOnlyCollection<SwitchCase> expressions)
        {
            if (expressions.Count == 0)
            {
                return 0;
            }

            var hash = GetHashCode(expressions[0]);

            for (var i = 1; i < expressions.Count; i++)
            {
                hash = Combine(hash, GetHashCode(expressions[i]));
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ReadOnlyCollection<ParameterExpression> expressions)
        {
            if (expressions.Count == 0)
            {
                return 0;
            }

            var hash = GetHashCode(expressions[0]);

            for (var i = 1; i < expressions.Count; i++)
            {
                hash = Combine(hash, GetHashCode(expressions[i]));
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetHashCode(ReadOnlyCollection<Expression> expressions)
        {
            if (expressions.Count == 0)
            {
                return 0;
            }

            var hash = GetHashCode(expressions[0]);

            for (var i = 1; i < expressions.Count; i++)
            {
                hash = Combine(hash, GetHashCode(expressions[i]));
            }

            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int Combine(int a, int b) => unchecked((a * 16777619) ^ b);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int StartHashCode(Expression node) => Combine(node.NodeType.GetHashCode(), node.Type.GetHashCode());
    }
}
