﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that, given a mapping of target and replacement
    /// <see cref="Expression"/> instances, replaces each target with its replacement.
    /// </summary>
    public class ExpressionReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly IDictionary<Expression, Expression> mapping;

        public ExpressionReplacingExpressionVisitor(Expression target, Expression replacement)
        {
            mapping = new Dictionary<Expression, Expression>
            {
                {
                    target ?? throw new ArgumentNullException(nameof(target)),
                    replacement ?? throw new ArgumentNullException(nameof(replacement))
                }
            };
        }

        public ExpressionReplacingExpressionVisitor(IDictionary<Expression, Expression> mapping)
        {
            this.mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        public override Expression Visit(Expression node)
        {
            if (node is null)
            {
                return node;
            }
            else if (mapping.TryGetValue(node, out var replacement))
            {
                return replacement;
            }
            else
            {
                return base.Visit(node);
            }
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (expression is null)
            {
                return node;
            }

            if (node.Member.DeclaringType.IsAssignableFrom(expression.Type))
            {
                return node.Update(expression);
            }

            if (expression.Type.IsAssignableFrom(node.Member.DeclaringType))
            {
                return node.Update(Expression.Convert(expression, node.Member.DeclaringType));
            }

            return node.Update(expression);
        }
    }
}
