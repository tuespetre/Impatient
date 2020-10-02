using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class IncludeRewritingExpressionVisitor : ExpressionVisitor
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
            private Stack<INavigation> path;

            public bool Finished { get; private set; }

            public CoreProjectionIncludeRewritingExpressionVisitor(
                Expression includedExpression,
                IEnumerable<INavigation> path)
            {
                this.includedExpression
                    = path.Last().GetSemanticReadableMemberInfo().GetMemberType().IsCollectionType()
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

                    case ExtendedNewExpression extendedNewExpression:
                    {
                        return VisitExtendedNew(extendedNewExpression);
                    }

                    case ExtendedMemberInitExpression extendedMemberInitExpression:
                    {
                        return VisitExtendedMemberInit(extendedMemberInitExpression);
                    }

                    case SimpleExtraPropertiesExpression extraPropertiesExpression:
                    {
                        var navigation = path.Pop();

                        try
                        {
                            for (var i = 0; i < extraPropertiesExpression.Names.Count; i++)
                            {
                                var name = extraPropertiesExpression.Names[i];

                                if (name == navigation.Name
                                    || name == $"<{navigation.DeclaringType.ClrType.Name}>{navigation.Name}")
                                {
                                    var expression = Visit(extraPropertiesExpression.Properties[i]);

                                    if (Finished)
                                    {
                                        return extraPropertiesExpression.SetProperty(name, expression);
                                    }
                                    else
                                    {
                                        return extraPropertiesExpression;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            path.Push(navigation);
                        }

                        return base.VisitExtension(node);
                    }

                    case PolymorphicExpression polymorphicExpression:
                    {
                        var row = Visit(polymorphicExpression.Row);

                        if (Finished)
                        {
                            return polymorphicExpression.Update(row, polymorphicExpression.Descriptors);
                        }

                        if (!(row is SimpleExtraPropertiesExpression extraProperties))
                        {
                            extraProperties
                                = new SimpleExtraPropertiesExpression(
                                    polymorphicExpression.Row,
                                    Array.Empty<string>(),
                                    Array.Empty<Expression>());
                        }

                        var navigation = path.Peek();

                        var member = navigation.GetSemanticReadableMemberInfo();

                        var hasCompatibleDescriptor
                            = polymorphicExpression.Descriptors
                                .Any(d => d.Type.IsAssignableFrom(member.DeclaringType));

                        if (!hasCompatibleDescriptor)
                        {
                            return polymorphicExpression.Update(row, polymorphicExpression.Descriptors);
                        }

                        var propertyName = member.Name;

                        if (member.DeclaringType.IsSubclassOf(node.Type))
                        {
                            propertyName = $"<{member.DeclaringType.Name}>{member.Name}";
                        }

                        extraProperties = extraProperties.SetProperty(propertyName, includedExpression);

                        var descriptors = polymorphicExpression.Descriptors.ToArray();

                        var finishedAny = false;

                        for (var i = 0; i < descriptors.Length; i++)
                        {
                            Finished = false;

                            var descriptor = descriptors[i];

                            var parameter = descriptor.Materializer.Parameters.Single();

                            includedExpression
                                = new ExtraPropertyAccessExpression(
                                    parameter,
                                    propertyName,
                                    member.GetMemberType());

                            var materializer
                                = Expression.Lambda(
                                    Visit(descriptor.Materializer.Body),
                                    descriptor.Materializer.Parameters);

                            descriptors[i]
                                = new PolymorphicTypeDescriptor(
                                    descriptor.Type,
                                    descriptor.Test,
                                    materializer);

                            finishedAny |= Finished;
                        }

                        if (finishedAny)
                        {
                            Finished = true;

                            return polymorphicExpression.Update(extraProperties, descriptors);
                        }

                        return polymorphicExpression.Update(row, polymorphicExpression.Descriptors);
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

                for (var i = 0; i < node.Arguments.Count; i++)
                {
                    var argument = arguments[i];
                    var member = node.Members[i];

                    if (member == currentMember.GetSemanticReadableMemberInfo())
                    {
                        arguments[i] = Visit(argument);
                        break;
                    }
                }

                path.Push(currentMember);

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
                var currentMemberInfo = currentMember.GetSemanticReadableMemberInfo();
                var foundMember = false;

                for (var i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];

                    if (binding.Member == currentMemberInfo)
                    {
                        foundMember = true;

                        if (path.Count == 0)
                        {
                            bindings[i] = Expression.Bind(currentMemberInfo, includedExpression);
                            Finished = true;
                        }
                        else
                        {
                            bindings[i] = VisitMemberBinding(binding);
                        }

                        break;
                    }
                }

                if (!foundMember && currentMemberInfo.DeclaringType.IsAssignableFrom(node.Type))
                {
                    bindings.Add(Expression.Bind(currentMember.GetWritableMemberInfo(), includedExpression));
                    Finished = true;
                }

                path.Push(currentMember);

                return node.Update(newExpression, bindings);
            }

            protected virtual Expression VisitExtendedNew(ExtendedNewExpression node)
            {
                var arguments = node.Arguments.ToArray();
                var currentMember = path.Pop();

                for (var i = 0; i < node.Arguments.Count; i++)
                {
                    var argument = arguments[i];
                    var member = node.ReadableMembers[i];

                    if (member == currentMember.GetSemanticReadableMemberInfo())
                    {
                        arguments[i] = Visit(argument);
                        break;
                    }
                }

                path.Push(currentMember);

                return node.Update(arguments);
            }

            protected virtual Expression VisitExtendedMemberInit(ExtendedMemberInitExpression node)
            {
                var newExpression = VisitAndConvert(node.NewExpression, nameof(VisitExtendedMemberInit));

                if (Finished)
                {
                    return node.Update(newExpression, node.Arguments);
                }

                var arguments = node.Arguments.ToList();
                var readableMembers = node.ReadableMembers.ToList();
                var writableMembers = node.WritableMembers.ToList();

                var currentMember = path.Pop();
                var currentMemberInfo = currentMember.GetSemanticReadableMemberInfo();
                var foundMember = false;

                for (var i = 0; i < arguments.Count; i++)
                {
                    var argument = arguments[i];

                    if (node.ReadableMembers[i] == currentMemberInfo)
                    {
                        foundMember = true;

                        if (path.Count == 0)
                        {
                            arguments[i] = includedExpression;
                            Finished = true;
                        }
                        else
                        {
                            arguments[i] = Visit(argument);
                        }

                        break;
                    }
                }

                path.Push(currentMember);

                if (!foundMember && currentMemberInfo.DeclaringType.IsAssignableFrom(node.Type))
                {
                    Finished = true;

                    var writableMemberInfo = currentMember.GetWritableMemberInfo();

                    arguments.Add(
                        writableMemberInfo.GetMemberType().IsCollectionType()
                            ? includedExpression.AsCollectionType()
                            : includedExpression);
                    
                    readableMembers.Add(currentMember.GetSemanticReadableMemberInfo());
                    writableMembers.Add(writableMemberInfo);

                    return new ExtendedMemberInitExpression(
                        node.Type,
                        node.NewExpression, 
                        arguments, 
                        readableMembers, 
                        writableMembers);
                }
                else
                {

                    return node.Update(newExpression, arguments);
                }
            }
        }
    }
}
