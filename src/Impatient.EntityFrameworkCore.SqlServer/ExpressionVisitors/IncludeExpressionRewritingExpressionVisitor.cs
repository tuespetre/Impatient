using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class IncludeExpressionRewritingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case IncludeExpression includeExpression:
                {
                    var expression = includeExpression.Expression;

                    var remainingIncludes = new List<Expression>();
                    var remainingPaths = new List<IReadOnlyList<INavigation>>();

                    for (var i = 0; i < includeExpression.Includes.Count; i++)
                    {
                        var include = includeExpression.Includes[i];
                        var path = includeExpression.Paths[i];

                        var rewriter = new CoreProjectionIncludeRewritingExpressionVisitor(include, path);

                        expression = rewriter.Visit(expression);

                        if (!rewriter.Finished)
                        {
                            remainingIncludes.Add(include);
                            remainingPaths.Add(path);
                        }
                    }

                    if (remainingIncludes.Count > 0)
                    {
                        return new IncludeExpression(expression, remainingIncludes, remainingPaths);
                    }

                    return expression;
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private class CoreProjectionIncludeRewritingExpressionVisitor : ExpressionVisitor
        {
            private Expression includedExpression;
            private readonly Stack<INavigation> path;

            public bool Finished { get; private set; }

            public CoreProjectionIncludeRewritingExpressionVisitor(
                Expression includedExpression, 
                IReadOnlyList<INavigation> path)
            {
                this.includedExpression 
                    = path.Last().PropertyInfo.GetMemberType().IsCollectionType() 
                        ? includedExpression.AsCollectionType() 
                        : includedExpression;

                this.path = new Stack<INavigation>(path.Reverse());
            }

            public override Expression Visit(Expression node)
            {
                if (Finished)
                {
                    return node;
                }
                else
                {
                    return base.Visit(node);
                }
            }

            protected override Expression VisitExtension(Expression node)
            {
                switch (node)
                {
                    case EntityMaterializationExpression entityMaterializationExpression:
                    {
                        var currentNavigation = path.Peek();

                        var visited = (EntityMaterializationExpression)base.VisitExtension(node);

                        if (Finished)
                        {
                            visited = visited.IncludeNavigation(currentNavigation);
                        }

                        return visited;
                    }

                    case ExtraPropertiesExpression extraPropertiesExpression:
                    {
                        var currentNavigation = path.Peek();

                        var visited = (ExtraPropertiesExpression)base.VisitExtension(node);

                        return visited;
                    }

                    case PolymorphicExpression polymorphicExpression:
                    {
                        var extraProperties = polymorphicExpression.Row as SimpleExtraPropertiesExpression;

                        if (extraProperties != null)
                        {
                            extraProperties = (SimpleExtraPropertiesExpression)base.Visit(extraProperties);

                            if (Finished)
                            {
                                return polymorphicExpression.Update(extraProperties, polymorphicExpression.Descriptors);
                            }
                        }
                        else
                        {
                            extraProperties
                                = new SimpleExtraPropertiesExpression(
                                    polymorphicExpression.Row, 
                                    Array.Empty<string>(), 
                                    Array.Empty<Expression>());
                        }

                        var navigation = path.Pop();

                        extraProperties = extraProperties.AddProperty(navigation.PropertyInfo.Name, includedExpression);

                        var descriptors = polymorphicExpression.Descriptors.ToArray();

                        for (var i = 0; i < descriptors.Length; i++)
                        {
                            Finished = false;

                            path.Push(navigation);

                            var descriptor = descriptors[i];

                            var parameter = descriptor.Materializer.Parameters.Single();

                            includedExpression
                                = new ExtraPropertyAccessExpression(
                                    parameter,
                                    navigation.PropertyInfo.Name,
                                    navigation.PropertyInfo.PropertyType);

                            var materializer
                                = Expression.Lambda(
                                    Visit(descriptor.Materializer.Body),
                                    descriptor.Materializer.Parameters);

                            descriptors[i]
                                = new PolymorphicTypeDescriptor(
                                    descriptor.Type,
                                    descriptor.Test,
                                    materializer);
                        }

                        Finished = true;

                        return polymorphicExpression.Update(extraProperties, descriptors);
                    }

                    default:
                    {
                        return base.VisitExtension(node);
                    }
                }
            }

            protected override Expression VisitNew(NewExpression node)
            {
                if (node.Members == null)
                {
                    return node;
                }

                var arguments = node.Arguments.ToArray();
                var currentMember = path.Pop();
                var foundMember = false;

                for (var i = 0; i < node.Arguments.Count; i++)
                {
                    var argument = arguments[i];
                    var member = node.Members[i];

                    if (member == currentMember.PropertyInfo)
                    {
                        foundMember = true;
                        arguments[i] = Visit(argument);
                        break;
                    }
                }

                if (!foundMember)
                {
                    path.Push(currentMember);
                }

                // TODO: Finding a new constructor to use that we can insert the include into?

                return node.Update(arguments);
            }

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                var newExpression = VisitAndConvert(node.NewExpression, nameof(VisitMemberInit));
                
                if (Finished)
                {
                    return node.Update(newExpression, node.Bindings);
                }

                var bindings = node.Bindings.ToList();
                var currentMember = path.Pop();
                var foundMember = false;

                for (var i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];

                    if (binding.Member == currentMember.PropertyInfo)
                    {
                        foundMember = true;

                        if (path.Count == 0)
                        {
                            bindings[i] = Expression.Bind(currentMember.PropertyInfo, includedExpression);
                            Finished = true;
                        }
                        else
                        {
                            bindings[i] = VisitMemberBinding(binding);
                        }

                        break;
                    }
                }

                if (!foundMember)
                {
                    if (currentMember.PropertyInfo.DeclaringType.IsAssignableFrom(node.Type))
                    {
                        bindings.Add(Expression.Bind(currentMember.PropertyInfo, includedExpression));
                        Finished = true;
                    }
                    else
                    {
                        path.Push(currentMember);
                    }
                }

                return node.Update(newExpression, bindings);
            }
        }
    }
}
