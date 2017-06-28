using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class HashingExpressionVisitor : ExpressionVisitor
    {
        private const int InitialHashCode = unchecked((int)2166136261);

        public int HashCode { get; private set; } = InitialHashCode;

        public void Reset() => HashCode = InitialHashCode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Combine(int hashCode) => HashCode = unchecked((HashCode * 16777619) ^ hashCode);

        public override Expression Visit(Expression node)
        {
            Combine(node == null ? 0 : node.NodeType.GetHashCode());
            Combine(node == null ? 0 : node.Type.GetHashCode());

            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Combine(node.IsLifted.GetHashCode());
            Combine(node.IsLiftedToNull.GetHashCode());
            Combine(node.Method == null ? 0 : node.Method.GetHashCode());

            return base.VisitBinary(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            Combine(node.Test.GetHashCode());

            return base.VisitCatchBlock(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Combine(node.Value == null ? 0 : node.Value.GetHashCode());

            return base.VisitConstant(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            Combine(node.Document.DocumentType.GetHashCode());
            Combine(node.Document.FileName == null ? 0 : node.Document.FileName.GetHashCode());
            Combine(node.Document.Language.GetHashCode());
            Combine(node.Document.LanguageVendor.GetHashCode());
            Combine(node.EndColumn);
            Combine(node.EndLine);
            Combine(node.IsClear.GetHashCode());
            Combine(node.StartColumn);
            Combine(node.StartLine);

            return base.VisitDebugInfo(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            Combine(node.AddMethod.GetHashCode());

            return base.VisitElementInit(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            Combine(node.GetHashCode());

            return base.VisitExtension(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            Combine(node.Indexer == null ? 0 : node.Indexer.GetHashCode());

            return base.VisitIndex(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            Combine(node.Name == null ? 0 : node.Name.GetHashCode());
            Combine(node.Type.GetHashCode());

            return base.VisitLabelTarget(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Combine(node.Name == null ? 0 : node.Name.GetHashCode());
            Combine(node.ReturnType.GetHashCode());
            Combine(node.TailCall.GetHashCode());

            return base.VisitLambda(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Combine(node.Member.GetHashCode());

            return base.VisitMember(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            Combine(node.BindingType.GetHashCode());
            Combine(node.Member.GetHashCode());

            return base.VisitMemberBinding(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Combine(node.Method.GetHashCode());

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Combine(node.Constructor == null ? 0 : node.Constructor.GetHashCode());

            if (node.Members != null)
            {
                for (var i = 0; i < node.Members.Count; i++)
                {
                    Combine(node.Members[i].GetHashCode());
                }
            }
            else
            {
                Combine(0);
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Combine(node.IsByRef.GetHashCode());
            Combine(node.Name == null ? 0 : node.Name.GetHashCode());

            return base.VisitParameter(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            Combine(node.Comparison == null ? 0 : node.Comparison.GetHashCode());

            return base.VisitSwitch(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            Combine(node.TypeOperand.GetHashCode());

            return base.VisitTypeBinary(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            Combine(node.IsLifted.GetHashCode());
            Combine(node.IsLiftedToNull.GetHashCode());
            Combine(node.Method == null ? 0 : node.Method.GetHashCode());

            return base.VisitUnary(node);
        }
    }
}