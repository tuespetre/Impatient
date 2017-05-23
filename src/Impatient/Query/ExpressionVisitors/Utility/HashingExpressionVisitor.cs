using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class HashingExpressionVisitor : ExpressionVisitor
    {
        private const int InitialHashCode = 17;

        public int HashCode { get; private set; } = InitialHashCode;

        public void Reset()
        {
            HashCode = InitialHashCode;
        }

        private void Hash<T>(T value)
        {
            unchecked
            {
                if (ReferenceEquals(value, null))
                {
                    HashCode = HashCode * 23 + 0;
                }
                else
                {
                    HashCode = HashCode * 23 + value.GetHashCode();
                }
            }
        }

        public override Expression Visit(Expression node)
        {
            if (node is null)
            {
                Hash(node);
                return node;
            }

            Hash(node.NodeType);
            Hash(node.Type);

            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            Hash(node.IsLifted);
            Hash(node.IsLiftedToNull);
            Hash(node.Method);

            return base.VisitBinary(node);
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            Hash(node.Test);

            return base.VisitCatchBlock(node);
        }
        
        // This is different than checking whether the type is scalar or not.
        // These are all of the possible literal expression types (at least in C#).
        private static Type[] constantLiteralTypes = new[]
        {
            typeof(sbyte),
            typeof(byte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(char),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(string),
        };

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (constantLiteralTypes.Contains(node.Type))
            {
                Hash(node.Value);
            }

            return base.VisitConstant(node);
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            Hash(node.Document.DocumentType);
            Hash(node.Document.FileName);
            Hash(node.Document.Language);
            Hash(node.Document.LanguageVendor);
            Hash(node.EndColumn);
            Hash(node.EndLine);
            Hash(node.IsClear);
            Hash(node.StartColumn);
            Hash(node.StartLine);

            return base.VisitDebugInfo(node);
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            Hash(node.AddMethod);

            return base.VisitElementInit(node);
        }

        protected override Expression VisitExtension(Expression node)
        {
            Hash(node);

            return base.VisitExtension(node);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            Hash(node.Indexer);

            return base.VisitIndex(node);
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            Hash(node.Name);
            Hash(node.Type);

            return base.VisitLabelTarget(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            Hash(node.Name);
            Hash(node.ReturnType);
            Hash(node.TailCall);

            return base.VisitLambda(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Hash(node.Member);

            return base.VisitMember(node);
        }

        protected override MemberBinding VisitMemberBinding(MemberBinding node)
        {
            Hash(node.BindingType);
            Hash(node.Member);

            return base.VisitMemberBinding(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Hash(node.Method);

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            Hash(node.Constructor);

            if (node.Members != null)
            {
                foreach (var member in node.Members)
                {
                    Hash(member);
                }
            }

            return base.VisitNew(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Hash(node.IsByRef);
            Hash(node.Name);

            return base.VisitParameter(node);
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            Hash(node.Comparison);

            return base.VisitSwitch(node);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            Hash(node.TypeOperand);

            return base.VisitTypeBinary(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            Hash(node.IsLifted);
            Hash(node.IsLiftedToNull);
            Hash(node.Method);

            return base.VisitUnary(node);
        }
    }
}
