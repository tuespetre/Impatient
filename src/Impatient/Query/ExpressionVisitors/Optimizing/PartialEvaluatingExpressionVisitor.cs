using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class PartialEvaluatingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            try
            {
                return base.Visit(node);
            }
            catch
            {
                return node;
            }
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitedLeft = Visit(node.Left);
            var visitedRight = Visit(node.Right);

            if (visitedLeft is ConstantExpression && visitedRight is ConstantExpression)
            {
                return TryEvaluateExpression(node.Update(visitedLeft, node.Conversion, visitedRight));
            }

            return node.Update(visitedLeft, node.Conversion, visitedRight);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            return node;
        }

        protected override Expression VisitConditional(ConditionalExpression node)
        {
            var visitedTest = Visit(node.Test);
            var visitedIfTrue = Visit(node.IfTrue);
            var visitedIfFalse = Visit(node.IfFalse);

            if (visitedTest is ConstantExpression && visitedIfTrue is ConstantExpression && visitedIfFalse is ConstantExpression)
            {
                return TryEvaluateExpression(node.Update(visitedTest, visitedIfTrue, visitedIfFalse));
            }

            return node.Update(visitedTest, visitedIfTrue, visitedIfFalse);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            return Expression.Constant(
                node.Type.GetTypeInfo().IsValueType
                    ? Activator.CreateInstance(node.Type)
                    : null,
                node.Type);
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            var visitedObject = Visit(node.Object);
            var visitedArguments = Visit(node.Arguments);

            if (visitedObject is ConstantExpression objectConstant
                && visitedArguments.All(a => a is ConstantExpression))
            {
                return Expression.Constant(
                    node.Indexer.GetValue(
                        objectConstant.Value,
                        visitedArguments
                            .Cast<ConstantExpression>()
                            .Select(c => c.Value)
                            .ToArray()));
            }

            return node.Update(visitedObject, visitedArguments);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            var visitedNewExpression = VisitNew(node.NewExpression);
            var visitedInitializers = node.Initializers.Select(VisitElementInit).ToArray();

            if (visitedNewExpression is ConstantExpression objectConstant
                && visitedInitializers.All(i => i.Arguments.All(a => a is ConstantExpression)))
            {
                ApplyListInitializers(objectConstant.Value, visitedInitializers);

                return objectConstant;
            }

            return node.Update((NewExpression)visitedNewExpression, visitedInitializers);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var visitedExpression = Visit(node.Expression);

            if (!unevaluableMembers.Contains(node.Member)
                && (visitedExpression is null || visitedExpression is ConstantExpression))
            {
                var @object = (visitedExpression as ConstantExpression)?.Value;

                switch (node.Member)
                {
                    case PropertyInfo propertyInfo:
                    {
                        return Expression.Constant(propertyInfo.GetValue(@object));
                    }

                    case FieldInfo fieldInfo:
                    {
                        return Expression.Constant(fieldInfo.GetValue(@object));
                    }
                }
            }

            return node.Update(visitedExpression);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var visitedNewExpression = VisitNew(node.NewExpression);
            var visitedBindings = node.Bindings.Select(VisitMemberBinding).ToArray();

            if (visitedNewExpression is ConstantExpression constantNewExpression
                && ValidateBindings(visitedBindings))
            {
                ApplyBindings(constantNewExpression.Value, visitedBindings);

                return constantNewExpression;
            }

            return node.Update((NewExpression)visitedNewExpression, visitedBindings);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var visitedObject = Visit(node.Object);
            var visitedArguments = Visit(node.Arguments);

            if (unevaluableMethods.Contains(node.Method))
            {
                // unevaluableMethods is a blacklist of nondeterministic methods
                // or methods that otherwise have side effects. We could open it
                // up for extension later.
                goto Finish;
            }

            if (visitedArguments.Any(a => typeof(IQueryable).IsAssignableFrom(a.Type)))
            {
                // If it's a method call that takes a queryable as an argument, we (for the most part)
                // can't guarantee that evaluating the method call won't trigger a query.
                // Calls like Queryable.Where and Queryable.Select and so forth may be ok, but 
                // we would end up re-parsing the resulting expression trees anyways so it is pointless
                // to evaluate them.
                goto Finish;
            }

            if ((visitedObject is null || visitedObject is ConstantExpression)
                && visitedArguments.All(a => a is ConstantExpression))
            {
                var @object = (visitedObject as ConstantExpression)?.Value;

                var result
                    = node.Method.Invoke(
                        @object,
                        visitedArguments
                            .Cast<ConstantExpression>()
                            .Select(c => c.Value)
                            .ToArray());

                return Expression.Constant(result);
            }

            Finish:
            return node.Update(visitedObject, visitedArguments);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            var visitedArguments = Visit(node.Arguments);

            if (visitedArguments.All(a => a is ConstantExpression))
            {
                return Expression.Constant(
                    node.Constructor.Invoke(
                        visitedArguments
                            .Cast<ConstantExpression>()
                            .Select(c => c.Value)
                            .ToArray()));
            }

            return node.Update(visitedArguments);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var visitedExpressions = Visit(node.Expressions);

            if (visitedExpressions.All(e => e is ConstantExpression))
            {
                switch (node.NodeType)
                {
                    case ExpressionType.NewArrayBounds:
                    {
                        return Expression.Constant(
                            Array.CreateInstance(
                                node.Type.GetElementType(),
                                visitedExpressions
                                    .Cast<ConstantExpression>()
                                    .Select(c => c.Value)
                                    .Cast<int>()
                                    .ToArray()));
                    }

                    case ExpressionType.NewArrayInit:
                    {
                        var array = Array.CreateInstance(
                            node.Type.GetElementType(),
                            visitedExpressions.Count);

                        var values = visitedExpressions
                            .Cast<ConstantExpression>()
                            .Select((c, i) => (c.Value, i));

                        foreach (var (value, index) in values)
                        {
                            array.SetValue(value, index);
                        }

                        return Expression.Constant(array);
                    }
                }
            }

            return node.Update(visitedExpressions);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            var visitedExpression = Visit(node.Expression);

            if (visitedExpression is ConstantExpression constantExpression)
            {
                return Expression.Constant(
                    node.TypeOperand.GetTypeInfo().IsAssignableFrom(
                        constantExpression.Value?.GetType().GetTypeInfo()));
            }

            return node.Update(visitedExpression);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var visitedOperand = Visit(node.Operand);

            if (visitedOperand is ConstantExpression)
            {
                return TryEvaluateExpression(node.Update(visitedOperand));
            }

            return node.Update(visitedOperand);
        }

        private static void ApplyListInitializers(object @object, IEnumerable<ElementInit> initializers)
        {
            foreach (var initializer in initializers)
            {
                initializer.AddMethod.Invoke(
                    @object,
                    initializer.Arguments
                        .Cast<ConstantExpression>()
                        .Select(c => c.Value)
                        .ToArray());
            }
        }

        private static bool ValidateBindings(IEnumerable<MemberBinding> bindings)
        {
            foreach (var binding in bindings)
            {
                switch (binding)
                {
                    case MemberAssignment memberAssignment:
                    {
                        if (memberAssignment.Expression is ConstantExpression)
                        {
                            break;
                        }

                        return false;
                    }

                    case MemberListBinding memberListBinding:
                    {
                        if (memberListBinding.Initializers.All(i => i.Arguments.All(a => a is ConstantExpression)))
                        {
                            break;
                        }

                        return false;
                    }

                    case MemberMemberBinding memberMemberBinding:
                    {
                        if (ValidateBindings(memberMemberBinding.Bindings))
                        {
                            break;
                        }

                        return false;
                    }

                    default:
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static void ApplyBindings(object @object, IEnumerable<MemberBinding> bindings)
        {
            foreach (var binding in bindings)
            {
                switch (binding)
                {
                    case MemberAssignment memberAssignment:
                    {
                        switch (memberAssignment.Member)
                        {
                            case PropertyInfo propertyInfo:
                            {
                                propertyInfo.SetValue(@object, ((ConstantExpression)memberAssignment.Expression).Value);
                                break;
                            }

                            case FieldInfo fieldInfo:
                            {
                                fieldInfo.SetValue(@object, ((ConstantExpression)memberAssignment.Expression).Value);
                                break;
                            }
                        }

                        break;
                    }

                    case MemberListBinding memberListBinding:
                    {
                        switch (memberListBinding.Member)
                        {
                            case PropertyInfo propertyInfo:
                            {
                                ApplyListInitializers(propertyInfo.GetValue(@object), memberListBinding.Initializers);
                                break;
                            }

                            case FieldInfo fieldInfo:
                            {
                                ApplyListInitializers(fieldInfo.GetValue(@object), memberListBinding.Initializers);
                                break;
                            }
                        }

                        break;
                    }

                    case MemberMemberBinding memberMemberBinding:
                    {
                        switch (memberMemberBinding.Member)
                        {
                            case PropertyInfo propertyInfo:
                            {
                                ApplyBindings(propertyInfo.GetValue(@object), memberMemberBinding.Bindings);
                                break;
                            }

                            case FieldInfo fieldInfo:
                            {
                                ApplyBindings(fieldInfo.GetValue(@object), memberMemberBinding.Bindings);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        private static Expression TryEvaluateExpression(Expression expression)
        {
            try
            {
                return Expression.Constant(Expression.Lambda(expression).Compile().DynamicInvoke());
            }
            catch
            {
                return expression;
            }
        }

        private static readonly IReadOnlyCollection<MemberInfo> unevaluableMembers = new List<MemberInfo>
        {
            typeof(DateTime).GetRuntimeProperty(nameof(DateTime.Now)),
            typeof(DateTime).GetRuntimeProperty(nameof(DateTime.UtcNow)),
            typeof(DateTimeOffset).GetRuntimeProperty(nameof(DateTimeOffset.Now)),
            typeof(DateTimeOffset).GetRuntimeProperty(nameof(DateTimeOffset.UtcNow)),
            typeof(Environment).GetRuntimeProperty(nameof(Environment.TickCount)),
        };

        private static readonly IReadOnlyCollection<MethodInfo> unevaluableMethods = new List<MethodInfo>
        {
            typeof(Guid).GetRuntimeMethod(nameof(Guid.NewGuid), new Type[0]),
            typeof(Random).GetRuntimeMethod(nameof(Random.Next), new Type[0]),
            typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[]{ typeof(int) }),
            typeof(Random).GetRuntimeMethod(nameof(Random.Next), new[]{ typeof(int), typeof(int) }),
            typeof(Random).GetRuntimeMethod(nameof(Random.NextBytes), new[] { typeof(byte[]) }),
            typeof(Random).GetRuntimeMethod(nameof(Random.NextDouble), new Type[0]),
        };
    }
}
