using Impatient.Extensions;
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
            if (node == null || node.NodeType == ExpressionType.Constant)
            {
                return node;
            }

            return base.Visit(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var visitedLeft = Visit(node.Left);
            var visitedRight = Visit(node.Right);

            if (visitedLeft.NodeType == ExpressionType.Constant
                && visitedRight.NodeType == ExpressionType.Constant)
            {
                return TryEvaluateExpression(node.Update(visitedLeft, node.Conversion, visitedRight));
            }

            if (visitedLeft.Type != visitedRight.Type)
            {
                if (visitedLeft.IsNullConstant())
                {
                    visitedLeft = Expression.Convert(visitedLeft, visitedRight.Type);
                }
                else if (visitedRight.IsNullConstant())
                {
                    visitedRight = Expression.Convert(visitedRight, visitedLeft.Type);
                }
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

            if (visitedTest.NodeType == ExpressionType.Constant
                && visitedIfTrue.NodeType == ExpressionType.Constant
                && visitedIfFalse.NodeType == ExpressionType.Constant)
            {
                return TryEvaluateExpression(node.Update(visitedTest, visitedIfTrue, visitedIfFalse));
            }

            return node.Update(visitedTest, visitedIfTrue, visitedIfFalse);
        }

        protected override Expression VisitDefault(DefaultExpression node)
        {
            try
            {
                return Expression.Constant(
                    node.Type.GetTypeInfo().IsValueType
                        ? Activator.CreateInstance(node.Type)
                        : null,
                    node.Type);
            }
            catch
            {
                return node;
            }
        }

        protected override Expression VisitIndex(IndexExpression node)
        {
            var @object = Visit(node.Object);

            if (@object == null && @object.NodeType != ExpressionType.Constant)
            {
                return node.Update(@object, Visit(node.Arguments));
            }

            var arguments = new Expression[node.Arguments.Count];
            var shouldApply = true;

            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = Visit(node.Arguments[i]);

                shouldApply &= argument.NodeType == ExpressionType.Constant;

                arguments[i] = argument;
            }

            if (shouldApply)
            {
                try
                {
                    var index = new object[arguments.Length];

                    for (var i = 0; i < index.Length; i++)
                    {
                        index[i] = ((ConstantExpression)arguments[i]).Value;
                    }

                    return Expression.Constant(
                        node.Indexer.GetValue(
                            ((ConstantExpression)@object).Value,
                            index));
                }
                catch
                {
                    // no-op, proceed to update arguments
                }
            }

            return node.Update(@object, arguments);
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            var initializers = new ElementInit[node.Initializers.Count];
            var shouldApply = true;

            for (var i = 0; i < node.Initializers.Count; i++)
            {
                var initializer = node.Initializers[i];
                var initializerArguments = new Expression[initializer.Arguments.Count];

                for (var j = 0; j < initializer.Arguments.Count; j++)
                {
                    var argument = Visit(initializer.Arguments[j]);

                    shouldApply &= argument.NodeType == ExpressionType.Constant;

                    initializerArguments[j] = argument;
                }

                initializers[i] = initializer.Update(initializerArguments);
            }

            if (shouldApply)
            {
                try
                {
                    if (Visit(node.NewExpression) is ConstantExpression @object)
                    {
                        for (var i = 0; i < initializers.Length; i++)
                        {
                            var initializer = initializers[i];
                            var addArguments = new object[initializer.Arguments.Count];

                            for (var j = 0; j < initializer.Arguments.Count; j++)
                            {
                                addArguments[j] = ((ConstantExpression)initializer.Arguments[j]).Value;
                            }

                            initializer.AddMethod.Invoke(@object.Value, addArguments);
                        }

                        return @object;
                    }
                }
                catch
                {
                    // no-op, proceed to update initializers
                }
            }

            var newExpression = node.NewExpression;
            var newArguments = new Expression[newExpression.Arguments.Count];

            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                newArguments[i] = Visit(newExpression.Arguments[i]);
            }

            return node.Update(newExpression.Update(newArguments), initializers);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = node.Expression;

            if (expression == null)
            {
                if (node.Member is FieldInfo field && (field.IsInitOnly || !field.IsLiteral))
                {
                    goto Finish;
                }

                if (node.Member is PropertyInfo)
                {
                    goto Finish;
                }
            }
            else
            {
                expression = Visit(node.Expression);
            }

            if (!unevaluableMembers.Contains(node.Member)
                && (expression == null || expression.NodeType == ExpressionType.Constant))
            {
                object @object = null;

                if (expression != null)
                {
                    @object = ((ConstantExpression)expression).Value;
                }

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

            Finish:
            return node.Update(expression);
        }

        protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
        {
            return base.VisitMemberListBinding(node);
        }

        protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
        {
            return base.VisitMemberMemberBinding(node);
        }

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            var bindings = new MemberBinding[node.Bindings.Count];
            var shouldApply = true;

            for (var i = 0; i < bindings.Length; i++)
            {
                var binding = VisitMemberBinding(node.Bindings[i]);

                shouldApply &= binding.BindingType == MemberBindingType.Assignment;
                shouldApply &= ((MemberAssignment)binding).Expression.NodeType == ExpressionType.Constant;

                bindings[i] = binding;
            }

            if (shouldApply)
            {
                try
                {
                    if (Visit(node.NewExpression) is ConstantExpression @object)
                    {
                        for (var i = 0; i < bindings.Length; i++)
                        {
                            var binding = (MemberAssignment)bindings[i];
                            var value = ((ConstantExpression)binding.Expression).Value;

                            switch (binding.Member)
                            {
                                case FieldInfo fieldInfo:
                                {
                                    fieldInfo.SetValue(@object.Value, value);
                                    break;
                                }

                                case PropertyInfo propertyInfo:
                                {
                                    propertyInfo.SetValue(@object.Value, value);
                                    break;
                                }
                            }
                        }

                        return @object;
                    }
                }
                catch
                {
                    // no-op, proceed to update arguments and bindings
                }
            }

            var newExpression = node.NewExpression;
            var newArguments = new Expression[node.NewExpression.Arguments.Count];

            for (var i = 0; i < newExpression.Arguments.Count; i++)
            {
                newArguments[i] = Visit(newExpression.Arguments[i]);
            }

            return node.Update(newExpression.Update(newArguments), bindings);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);

            if (@object == null && node.Arguments.Count == 0)
            {
                return node;
            }

            var shouldApply = @object == null || @object.NodeType == ExpressionType.Constant;

            if (!shouldApply)
            {
                return node.Update(@object, Visit(node.Arguments));
            }

            var arguments = new Expression[node.Arguments.Count];

            for (var i = 0; i < arguments.Length; i++)
            {
                var argument = Visit(node.Arguments[i]);

                shouldApply &= argument.NodeType == ExpressionType.Constant;
                shouldApply &= !typeof(IQueryable).IsAssignableFrom(argument.Type);

                arguments[i] = argument;
            }

            // If it's a method call that takes a queryable as an argument, we (for the most part)
            // can't guarantee that evaluating the method call won't trigger a query. One would
            // at first think that if the return type is also IQueryable it might be OK to evaluate,
            // but the truth is that methods like Single() could still return an IQueryable.
            // Calls like Queryable.Where and Queryable.Select and so forth may be ok, but 
            // we would end up re-parsing the resulting expression trees anyways so it is pointless
            // to evaluate them.

            if (unevaluableMethods.Contains(node.Method) || node.Method.IsQueryableOrEnumerableMethod())
            {
                // unevaluableMethods is a blacklist of nondeterministic methods
                // or methods that otherwise have side effects. We could open it
                // up for extension later.
                goto Finish;
            }

            if (shouldApply)
            {
                var target = (@object as ConstantExpression)?.Value;
                var parameters = new object[arguments.Length];

                for (var i = 0; i < arguments.Length; i++)
                {
                    parameters[i] = ((ConstantExpression)arguments[i]).Value;
                }

                try
                {
                    var result = node.Method.Invoke(target, parameters);

                    return Expression.Constant(result);
                }
                catch
                {
                    // no-op, proceed to update arguments
                }
            }

            Finish:
            return node.Update(@object, arguments);
        }

        protected override Expression VisitNew(NewExpression node)
        {
            if (node.Constructor == null)
            {
                return Expression.Constant(Activator.CreateInstance(node.Type));
            }

            var visitedArguments = new Expression[node.Arguments.Count];
            var shouldApply = true;

            for (var i = 0; i < node.Arguments.Count; i++)
            {
                var argument = Visit(node.Arguments[i]);

                shouldApply &= argument.NodeType == ExpressionType.Constant;

                visitedArguments[i] = argument;
            }

            if (shouldApply)
            {
                var arguments = new object[visitedArguments.Length];

                for (var i = 0; i < arguments.Length; i++)
                {
                    arguments[i] = ((ConstantExpression)visitedArguments[i]).Value;
                }

                try
                {
                    return Expression.Constant(node.Constructor.Invoke(arguments));
                }
                catch
                {
                    // no-op, proceed to update node with arguments
                }
            }

            return node.Update(visitedArguments);
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            var expressions = new Expression[node.Expressions.Count];
            var shouldApply = true;

            for (var i = 0; i < expressions.Length; i++)
            {
                var expression = Visit(node.Expressions[i]);

                shouldApply &= expression.NodeType == ExpressionType.Constant;

                expressions[i] = expression;
            }

            if (shouldApply)
            {
                try
                {
                    switch (node.NodeType)
                    {
                        case ExpressionType.NewArrayBounds:
                        {
                            var lengths = new int[expressions.Length];

                            for (var i = 0; i < expressions.Length; i++)
                            {
                                lengths[i] = (int)((ConstantExpression)expressions[i]).Value;
                            }

                            return Expression.Constant(
                                Array.CreateInstance(
                                    node.Type.GetElementType(),
                                    lengths));
                        }

                        case ExpressionType.NewArrayInit:
                        {
                            var array
                                = Array.CreateInstance(
                                    node.Type.GetElementType(),
                                    expressions.Length);

                            for (var i = 0; i < expressions.Length; i++)
                            {
                                array.SetValue(((ConstantExpression)expressions[i]).Value, i);
                            }

                            return Expression.Constant(array);
                        }

                        default:
                        {
                            throw new NotSupportedException();
                        }
                    }
                }
                catch
                {
                    // no-op, proceed to update expressions
                }
            }

            return node.Update(expressions);
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            var expression = Visit(node.Expression);

            if (expression is ConstantExpression || expression is ParameterExpression)
            {
                if (node.TypeOperand.IsAssignableFrom(expression.Type))
                {
                    return Expression.Constant(true);
                }
                else if (!expression.Type.IsAssignableFrom(node.TypeOperand))
                {
                    return Expression.Constant(false);
                }
            }

            return node.Update(expression);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            var operand = Visit(node.Operand);

            node = node.Update(operand);

            if (operand.NodeType == ExpressionType.Constant && node.NodeType != ExpressionType.Convert)
            {
                return TryEvaluateExpression(node);
            }

            return node.Update(operand);
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

        private static readonly List<MemberInfo> unevaluableMembers = new List<MemberInfo>
        {
            typeof(DateTime).GetRuntimeProperty(nameof(DateTime.Now)),
            typeof(DateTime).GetRuntimeProperty(nameof(DateTime.UtcNow)),
            typeof(DateTimeOffset).GetRuntimeProperty(nameof(DateTimeOffset.Now)),
            typeof(DateTimeOffset).GetRuntimeProperty(nameof(DateTimeOffset.UtcNow)),
            typeof(Environment).GetRuntimeProperty(nameof(Environment.TickCount)),
        };

        private static readonly List<MethodInfo> unevaluableMethods = new List<MethodInfo>
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
