using System;
using System.Linq.Expressions;
using System.Text;
using Impatient.Extensions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class ExpressionTreePrintingExpressionVisitor : ExpressionVisitor
    {
        private int indentationLevel = 0;
        private bool justAppendedLine;

        protected void Append(string text)
        {
            StringBuilder.Append(text);

            justAppendedLine = false;
        }

        protected void AppendLine(bool conditional = false)
        {
            if (conditional && justAppendedLine)
            {
                return;
            }

            StringBuilder.AppendLine();
            StringBuilder.Append(string.Empty.PadLeft(indentationLevel * 4));

            justAppendedLine = true;
        }

        protected void IncreaseIndent()
        {
            indentationLevel++;
        }

        protected void DecreaseIndent()
        {
            indentationLevel--;
        }

        public StringBuilder StringBuilder { get; } = new StringBuilder();

        public void Reset()
        {
            StringBuilder.Clear();
            indentationLevel = 0;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.ArrayIndex)
            {
                Visit(node.Left);

                Append("[");
                Visit(node.Right);
                Append("]");

                return node;
            }

            var @checked = node.IsChecked();

            var parentheses
                = node.NodeType == ExpressionType.AndAlso
                || node.NodeType == ExpressionType.OrElse;

            var appendLine = node.IsAssignment();

            if (@checked)
            {
                Append("checked(");
            }
            else if (parentheses)
            {
                Append(")");
            }

            Visit(node.Left);

            switch (node.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                {
                    Append(" + ");
                    break;
                }

                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                {
                    Append(" - ");
                    break;
                }

                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                {
                    Append(" * ");
                    break;
                }

                case ExpressionType.Divide:
                {
                    Append(" / ");
                    break;
                }

                case ExpressionType.Modulo:
                {
                    Append(" % ");
                    break;
                }

                case ExpressionType.LessThan:
                {
                    Append(" < ");
                    break;
                }

                case ExpressionType.LessThanOrEqual:
                {
                    Append(" <= ");
                    break;
                }

                case ExpressionType.GreaterThan:
                {
                    Append(" > ");
                    break;
                }

                case ExpressionType.GreaterThanOrEqual:
                {
                    Append(" >= ");
                    break;
                }

                case ExpressionType.Equal:
                {
                    Append(" == ");
                    break;
                }

                case ExpressionType.NotEqual:
                {
                    Append(" != ");
                    break;
                }

                case ExpressionType.And:
                {
                    Append(" & ");
                    break;
                }

                case ExpressionType.Or:
                {
                    Append(" | ");
                    break;
                }

                case ExpressionType.ExclusiveOr:
                case ExpressionType.Power:
                {
                    Append(" ^ ");
                    break;
                }

                case ExpressionType.RightShift:
                {
                    Append(" >> ");
                    break;
                }

                case ExpressionType.LeftShift:
                {
                    Append(" << ");
                    break;
                }

                case ExpressionType.AndAlso:
                {
                    Append(" && ");
                    break;
                }

                case ExpressionType.OrElse:
                {
                    Append(" || ");
                    break;
                }

                case ExpressionType.Coalesce:
                {
                    Append(" ?? ");
                    break;
                }

                case ExpressionType.ArrayIndex:
                {
                    throw new NotSupportedException();
                }

                case ExpressionType.Assign:
                {
                    Append(" = ");
                    break;
                }

                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                {
                    Append(" += ");
                    break;
                }

                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                {
                    Append(" -= ");
                    break;
                }

                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                {
                    Append(" *= ");
                    break;
                }

                case ExpressionType.DivideAssign:
                {
                    Append(" /= ");
                    break;
                }

                case ExpressionType.ModuloAssign:
                {
                    Append(" %= ");
                    break;
                }

                case ExpressionType.AndAssign:
                {
                    Append(" &= ");
                    break;
                }

                case ExpressionType.OrAssign:
                {
                    Append(" |= ");
                    break;
                }

                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.PowerAssign:
                {
                    Append(" ^= ");
                    break;
                }

                case ExpressionType.RightShiftAssign:
                {
                    Append(" >>= ");
                    break;
                }

                case ExpressionType.LeftShiftAssign:
                {
                    Append(" <<= ");
                    break;
                }
            }

            Visit(node.Right);

            if (@checked || parentheses)
            {
                Append(")");
            }

            if (appendLine)
            {
                AppendLine();
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            AppendLine(conditional: true);
            Append("{");
            IncreaseIndent();
            AppendLine();

            for (var i = 0; i < node.Variables.Count; i++)
            {
                Append(node.Variables[i].Type.Name);
                Append(" ");
                Append(node.Variables[i].Name);
                Append(";");
                AppendLine();
            }

            for (var i = 0; i < node.Expressions.Count; i++)
            {
                AppendLine(conditional: true);
                Visit(node.Expressions[i]);
            }

            DecreaseIndent();
            AppendLine();

            return node;
        }

        protected override CatchBlock VisitCatchBlock(CatchBlock node)
        {
            Append($"catch ({node.Test.Name} {node.Variable.Name})");

            if (node.Filter != null)
            {
                IncreaseIndent();
                AppendLine();
                Append("when (");
                Visit(node.Filter);
                Append(")");
                DecreaseIndent();
            }

            AppendLine();

            if (node.Body is BlockExpression block)
            {
                Visit(block);
            }
            else
            {
                Append("{");
                IncreaseIndent();
                AppendLine();
                Visit(node.Body);
                DecreaseIndent();
                AppendLine();
                Append("}");
                AppendLine();
            }

            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            Visit(node.Test);
            Append(" ? ");
            Visit(node.IfTrue);
            Append(" : ");
            Visit(node.IfFalse);

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            Append(node.ToString());

            return node;
        }

        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            Append("default(");
            Append(node.Type.Name);
            Append(")");

            return node;
        }

        protected override ElementInit VisitElementInit(ElementInit node)
        {
            if (node.Arguments.Count == 1)
            {
                Visit(node.Arguments[0]);
            }
            else
            {
                Append("{ ");

                Visit(node.Arguments[0]);

                for (var i = 1; i < node.Arguments.Count; i++)
                {
                    Append(", ");
                    Visit(node.Arguments[i]);
                }

                Append(" }");
            }

            return node;
        }

        protected override Expression VisitExtension(Expression node)
        {
            Append("<");
            Append(node.GetType().Name);
            Append(">(");
            base.VisitExtension(node);
            Append(")");

            return node;
        }

        protected override Expression VisitGoto(GotoExpression node)
        {
            switch (node.Kind)
            {
                case GotoExpressionKind.Break:
                {
                    Append("break;");

                    return node;
                }

                case GotoExpressionKind.Continue:
                {
                    Append("continue;");

                    return node;
                }

                case GotoExpressionKind.Goto:
                {
                    if (node.Target.Type == typeof(void))
                    {
                        Append($"goto {node.Target.Name};");
                    }
                    else
                    {
                        Append($"goto {node.Target.Name} (");
                        Visit(node.Value);
                        Append(");");
                    }

                    return node;
                }

                case GotoExpressionKind.Return:
                {
                    if (node.Target.Type == typeof(void))
                    {
                        Append("return;");
                    }
                    else
                    {
                        Append("return ");
                        Visit(node.Value);
                    }

                    return node;
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            if (node.Object is null)
            {
                // Not sure if this is even possible in IL
                Append(node.Indexer.DeclaringType.Name);
            }
            else
            {
                Visit(node.Object);
            }

            if (node.Indexer.Name != "Item")
            {
                Append(".");
                Append(node.Indexer.Name);
            }

            Append("[");

            Visit(node.Arguments[0]);

            for (var i = 1; i < node.Arguments.Count; i++)
            {
                Append(", ");
                Visit(node.Arguments[i]);
            }

            Append("]");

            return base.VisitIndex(node);
        }

        protected override Expression VisitInvocation(InvocationExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitLabel(LabelExpression node)
        {
            throw new NotImplementedException();
        }

        protected override LabelTarget VisitLabelTarget(LabelTarget node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitLoop(LoopExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is null)
            {
                Append(node.Member.DeclaringType.Name);
            }
            else
            {
                Visit(node.Expression);
            }

            Append(".");
            Append(node.Member.Name);

            return node;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            Append(node.Member.Name);
            Append(" = ");
            Visit(node.Expression);

            return node;
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            Visit(node.NewExpression);

            if (node.Bindings.Count != 0)
            {
                AppendLine();
                Append("{");
                IncreaseIndent();
                AppendLine();
                VisitMemberBinding(node.Bindings[0]);
                Append(",");

                for (var i = 0; i < node.Bindings.Count; i++)
                {
                    AppendLine();
                    VisitMemberBinding(node.Bindings[i]);
                    Append(",");
                }

                DecreaseIndent();
                AppendLine();
                Append("}");
            }
            else
            {
                Append("{ }");
            }

            return node;
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            Append(node.Member.Name);
            Append(" = ");

            if (node.Initializers.Count != 0)
            {
                AppendLine();
                Append("{");
                IncreaseIndent();
                AppendLine();
                VisitElementInit(node.Initializers[0]);
                Append(",");

                for (var i = 0; i < node.Initializers.Count; i++)
                {
                    AppendLine();
                    VisitElementInit(node.Initializers[i]);
                    Append(",");
                }

                DecreaseIndent();
                AppendLine();
                Append("}");
            }
            else
            {
                Append("{ }");
            }

            return node;
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            Append(node.Member.Name);
            Append(" = ");

            if (node.Bindings.Count != 0)
            {
                AppendLine();
                Append("{");
                IncreaseIndent();
                AppendLine();
                VisitMemberBinding(node.Bindings[0]);
                Append(",");

                for (var i = 0; i < node.Bindings.Count; i++)
                {
                    AppendLine();
                    VisitMemberBinding(node.Bindings[i]);
                    Append(",");
                }

                DecreaseIndent();
                AppendLine();
                Append("}");
            }
            else
            {
                Append("{ }");
            }

            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitNew(NewExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            Append(node.Name);

            throw new NotImplementedException();
        }

        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitSwitch(SwitchExpression node)
        {
            throw new NotImplementedException();
        }

        protected override SwitchCase VisitSwitchCase(SwitchCase node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitTry(TryExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
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
                    break;
                }
            }

            throw new NotImplementedException();
        }
    }
}
