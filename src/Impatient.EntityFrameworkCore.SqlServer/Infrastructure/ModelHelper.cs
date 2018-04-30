using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    using Impatient.Extensions;

    internal static class ModelHelper
    {
        private static readonly MethodInfo efPropertyMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition<object, string>(obj => EF.Property<string>(obj, "key"));

        private static readonly MethodInfo queryableWhere
            = ReflectionExtensions.GetGenericMethodDefinition((IQueryable<bool> e) => e.Where(x => x));

        private static LambdaExpression CreateNavigationKeySelector(Type type, IReadOnlyList<IProperty> properties)
        {
            var entityParameter = Expression.Parameter(type);

            var expressions
                = (from p in properties
                   select p.IsShadowProperty
                     ? (Expression)Expression.Call(
                         efPropertyMethodInfo.MakeGenericMethod(p.ClrType),
                         entityParameter,
                         Expression.Constant(p.Name))
                     : Expression.MakeMemberAccess(entityParameter, p.GetMemberInfo(false, false))).ToArray();

            return Expression.Lambda(
                properties.Count == 1
                    ? expressions[0]
                    : Expression.NewArrayInit(
                        typeof(object),
                        from e in expressions
                        select Expression.Convert(e, typeof(object))),
                entityParameter);
        }

        private static LambdaExpression CreateMaterializationKeySelector(IEntityType type)
        {
            var entityParameter = Expression.Parameter(type.ClrType);
            var shadowPropertiesParameter = Expression.Parameter(typeof(object[]));

            return Expression.Lambda(
                Expression.NewArrayInit(
                    typeof(object),
                    from p in type.FindPrimaryKey().Properties
                    select Expression.Convert(
                        p.IsShadowProperty
                            ? Expression.ArrayIndex(shadowPropertiesParameter, Expression.Constant(p.GetShadowIndex()))
                            : (Expression)Expression.MakeMemberAccess(entityParameter, p.GetMemberInfo(false, false)),
                        typeof(object))),
                entityParameter,
                shadowPropertiesParameter);
        }

        public static IEnumerable<PrimaryKeyDescriptor> CreatePrimaryKeyDescriptors(IModel model)
        {
            return from t in model.GetEntityTypes()
                   where !t.IsOwned()
                   let k = t.FindPrimaryKey()
                   select new PrimaryKeyDescriptor(
                       t.ClrType,
                       CreateNavigationKeySelector(k.DeclaringEntityType.ClrType, k.Properties));
        }

        public static IEnumerable<NavigationDescriptor> CreateNavigationDescriptors(IModel model)
        {
            var fks = from t in model.GetEntityTypes()
                      from f in t.GetForeignKeys()
                      where !f.IsOwnership
                      select f;

            var hashset = new HashSet<MemberInfo>();

            foreach (var fk in fks.Distinct())
            {
                var principal = CreateNavigationKeySelector(fk.PrincipalEntityType.ClrType, fk.PrincipalKey.Properties);
                var dependent = CreateNavigationKeySelector(fk.DeclaringEntityType.ClrType, fk.Properties);

                if (fk.PrincipalToDependent != null)
                {
                    yield return new NavigationDescriptor(
                        fk.PrincipalEntityType.ClrType,
                        GetMemberInfo(fk.PrincipalToDependent),
                        principal,
                        dependent,
                        true,
                        CreateQueryExpression(fk.DeclaringEntityType.ClrType, model));
                }

                if (fk.DependentToPrincipal != null)
                {
                    yield return new NavigationDescriptor(
                        fk.DeclaringEntityType.ClrType,
                        GetMemberInfo(fk.DependentToPrincipal),
                        dependent,
                        principal,
                        !fk.IsRequired,
                        CreateQueryExpression(fk.PrincipalEntityType.ClrType, model));
                }
            }
        }

        private static bool IsTablePerHierarchy(IEntityType rootType, IEnumerable<IEntityType> hierarchy)
        {
            return hierarchy.All(t =>
                t.Relational().Schema == rootType.Relational().Schema
                && t.Relational().TableName == rootType.Relational().TableName);
        }

        public static Expression CreateQueryExpression(Type elementType, IModel model)
        {
            // TODO: (EF Core 2.0) Owned entities split over multiple tables

            // TODO: (EF Core 2.0) Distinct entities split over a single table

            // TODO: (EF Core 2.0) Handle model properties mapped to CLR fields

            // TODO: (EF Core 2.1) Lazy loading / proxies

            var targetType = model.GetEntityTypes().SingleOrDefault(t => t.ClrType == elementType);
            var rootType = targetType.RootType();

            Expression materializer;

            var schema = rootType.Relational().Schema ?? model.Relational().DefaultSchema;

            var table
                = new BaseTableExpression(
                    schema,
                    rootType.Relational().TableName,
                    rootType.Relational().TableName.Substring(0, 1).ToLower(),
                    rootType.ClrType);

            var hierarchy = rootType.GetDerivedTypes().Prepend(rootType).ToArray();
            var keyExpression = CreateMaterializationKeySelector(targetType);

            if (hierarchy.Length == 1)
            {
                // No inheritance

                var shadowProperties
                    = from s in targetType.GetProperties()
                      where s.IsShadowProperty
                      select (property: s, column: MakeColumnExpression(table, s));

                materializer
                    = new EntityMaterializationExpression(
                        targetType,
                        IdentityMapMode.IdentityMap,
                        keyExpression,
                        shadowProperties.Select(s => s.property),
                        shadowProperties.Select(s => s.column),
                        Expression.MemberInit(
                            GetNewExpression(targetType),
                            GetMemberAssignments(targetType, p => MakeColumnExpression(table, p))));
            }
            else if (IsTablePerHierarchy(rootType, hierarchy))
            {
                // Table-per-hierarchy inheritance

                var properties
                    = (from t in hierarchy
                       from p in IterateAllProperties(t)
                       group p by GetRelationalId(p) into g
                       select (id: g.Key, property: g.First())).ToArray();

                var columns
                    = (from p in properties.Select(p => p.property)
                       select MakeColumnExpression(table, p)).ToArray();

                var tupleType = ValueTupleHelper.CreateTupleType(columns.Select(c => c.Type));
                var tupleParameter = Expression.Parameter(tupleType);

                var descriptors = new List<PolymorphicTypeDescriptor>();

                foreach (var type in hierarchy.Where(t => !t.ClrType.IsAbstract))
                {
                    var discriminator
                        = Array.FindIndex(
                            properties,
                            p => p.property == type.Relational().DiscriminatorProperty);

                    var test
                        = Expression.Lambda(
                            Expression.Equal(
                                ValueTupleHelper.CreateMemberExpression(tupleType, tupleParameter, discriminator),
                                Expression.Constant(type.Relational().DiscriminatorValue)),
                            tupleParameter);

                    var shadowProperties
                        = from s in type.GetProperties()
                          where s.IsShadowProperty
                          let e = ValueTupleHelper.CreateMemberExpression(
                              tupleType,
                              tupleParameter,
                              Array.FindIndex(properties, p => p.id.Equals(GetRelationalId(s))))
                          select (property: s, expression: e);

                    var bindings
                        = GetMemberAssignments(type, p =>
                            Expression.Convert(
                                ValueTupleHelper.CreateMemberExpression(
                                    tupleType,
                                    tupleParameter,
                                    Array.FindIndex(properties, q => q.id.Equals(GetRelationalId(p)))),
                                p.ClrType));

                    var descriptorMaterializer
                        = Expression.Lambda(
                            new EntityMaterializationExpression(
                                targetType,
                                IdentityMapMode.IdentityMap,
                                keyExpression,
                                shadowProperties.Select(s => s.property),
                                shadowProperties.Select(s => s.expression),
                                Expression.MemberInit(GetNewExpression(type), bindings)),
                            tupleParameter);

                    descriptors.Add(new PolymorphicTypeDescriptor(type.ClrType, test, descriptorMaterializer));
                }

                materializer
                    = new PolymorphicExpression(
                        targetType.ClrType,
                        ValueTupleHelper.CreateNewExpression(tupleType, columns),
                        descriptors).Filter(targetType.ClrType);
            }
            else
            {
                // Waiting on EF Core:

                // TODO: (EF Core ?.?) Table-per-type polymorphism

                // TODO: (EF Core ?.?) Table-per-concrete polymorphism

                throw new NotSupportedException();
            }

            var projection = new ServerProjectionExpression(materializer);

            var selectExpression = new SelectExpression(projection, table);

            if (materializer is PolymorphicExpression polymorphicExpression
                && polymorphicExpression.Type != rootType.ClrType)
            {
                var predicate
                    = polymorphicExpression
                        .Descriptors
                        .Select(d => d.Test.ExpandParameters(polymorphicExpression.Row))
                        .Aggregate(Expression.OrElse);

                selectExpression = selectExpression.AddToPredicate(predicate);
            }

            var queryExpression = (Expression)new EnumerableRelationalQueryExpression(selectExpression);

            var currentType = targetType;

            while (currentType != null)
            {
                if (currentType.QueryFilter != null)
                {
                    // Use a method call instead of adding to the SelectExpression
                    // so the rewriting visitors are guaranteed to get their hands on the 
                    // filter.
                    queryExpression
                        = Expression.Call(
                            queryableWhere.MakeGenericMethod(currentType.ClrType),
                            queryExpression,
                            Expression.Quote(
                                Expression.Lambda(
                                    new QueryFilterExpression(currentType.QueryFilter.Body),
                                    currentType.QueryFilter.Parameters)));
                }

                currentType = currentType.BaseType;
            }

            return queryExpression;
        }

        private static NewExpression GetNewExpression(IEntityType type)
        {
            // TODO: (EF Core 2.1) Constructor selection with property-parameter mappings

            // TODO: (EF Core 2.1) Constructor selection with dependency injection

            return Expression.New(type.ClrType);
        }

        private static IEnumerable<MemberAssignment> GetMemberAssignments(IEntityType type, Func<IProperty, Expression> func)
        {
            foreach (var property in type.GetProperties())
            {
                if (!property.IsShadowProperty)
                {
                    yield return Expression.Bind(
                        GetMemberInfo(property),
                        func(property));
                }
            }

            foreach (var navigation in type.GetNavigations())
            {
                if (navigation.ForeignKey.IsOwnership)
                {
                    var navigationType = navigation.GetTargetType();
                    var newExpression = GetNewExpression(navigationType);
                    var bindings = GetMemberAssignments(navigationType, func);

                    var shadowProperties
                        = from s in navigationType.GetProperties()
                          where s.IsShadowProperty
                          select (property: s, expression: func(s));

                    yield return Expression.Bind(
                        GetMemberInfo(navigation),
                        new EntityMaterializationExpression(
                            navigationType,
                            IdentityMapMode.IdentityMap,
                            CreateMaterializationKeySelector(navigationType),
                            shadowProperties.Select(s => s.property),
                            shadowProperties.Select(s => s.expression),
                            Expression.MemberInit(newExpression, bindings)));
                }
            }
        }

        private static IEnumerable<IProperty> IterateAllProperties(IEntityType type)
        {
            foreach (var property in type.GetProperties())
            {
                yield return property;
            }

            foreach (var navigation in type.GetNavigations())
            {
                if (navigation.ForeignKey.IsOwnership)
                {
                    foreach (var property in IterateAllProperties(navigation.GetTargetType()))
                    {
                        yield return property;
                    }
                }
            }
        }

        private static Expression MakeColumnExpression(AliasedTableExpression table, IProperty property)
        {
            return new SqlColumnExpression(
                table,
                property.Relational().ColumnName,
                property.ClrType,
                property.IsNullable || property.DeclaringEntityType.RootType() != property.DeclaringEntityType);
        }

        private static (string, string, string) GetRelationalId(IProperty property)
        {
            var table = property.DeclaringEntityType.Relational();

            return (table.Schema, table.TableName, property.Relational().ColumnName);
        }

        private static MemberInfo GetMemberInfo(IPropertyBase property)
        {
            if (property.PropertyInfo != null)
            {
                return property.PropertyInfo.DeclaringType.GetProperty(property.PropertyInfo.Name);
            }

            if (property.FieldInfo != null)
            {
                return property.FieldInfo.DeclaringType.GetField(property.FieldInfo.Name);
            }

            return null;
        }
    }
}
