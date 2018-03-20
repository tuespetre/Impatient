using Impatient.Metadata;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal static class ModelHelper
    {
        public static LambdaExpression CreateKeySelector(Type type, IReadOnlyList<IProperty> properties)
        {
            var parameter = Expression.Parameter(type);

            if (properties.Count == 1)
            {
                return Expression.Lambda(
                    Expression.MakeMemberAccess(
                        parameter, 
                        GetMemberInfo(properties[0])), 
                    parameter);
            }
            else
            {
                var tuple = ValueTupleHelper.CreateTupleType(properties.Select(prop => prop.ClrType));
                var reads = properties.Select(p => Expression.MakeMemberAccess(parameter, GetMemberInfo(p)));

                return Expression.Lambda(ValueTupleHelper.CreateNewExpression(tuple, reads), parameter);
            }
        }

        public static IEnumerable<PrimaryKeyDescriptor> CreatePrimaryKeyDescriptors(IModel model)
        {
            return from t in model.GetEntityTypes()
                   where !t.IsOwned()
                   let k = t.FindPrimaryKey()
                   select new PrimaryKeyDescriptor
                   {
                       TargetType = t.ClrType,
                       KeySelector = CreateKeySelector(k.DeclaringEntityType.ClrType, k.Properties)
                   };
        }

        public static IEnumerable<NavigationDescriptor> CreateNavigationDescriptors(IModel model)
        {
            var fks = from t in model.GetEntityTypes()
                      from f in t.GetForeignKeys()
                      where !f.IsOwnership
                      select f;

            foreach (var fk in fks.Distinct())
            {
                var principal = CreateKeySelector(fk.PrincipalEntityType.ClrType, fk.PrincipalKey.Properties);
                var dependent = CreateKeySelector(fk.DeclaringEntityType.ClrType, fk.Properties);

                if (fk.PrincipalToDependent != null)
                {
                    yield return new NavigationDescriptor
                    {
                        Type = fk.PrincipalEntityType.ClrType,
                        Member = GetMemberInfo(fk.PrincipalToDependent),
                        IsNullable = !fk.IsRequired,
                        Expansion = CreateQueryable(fk.DeclaringEntityType.ClrType, model),
                        OuterKeySelector = principal,
                        InnerKeySelector = dependent
                    };
                }

                if (fk.DependentToPrincipal != null)
                {
                    yield return new NavigationDescriptor
                    {
                        Type = fk.DeclaringEntityType.ClrType,
                        Member = GetMemberInfo(fk.DependentToPrincipal),
                        IsNullable = false,
                        Expansion = CreateQueryable(fk.PrincipalEntityType.ClrType, model),
                        OuterKeySelector = dependent,
                        InnerKeySelector = principal
                    };
                }
            }
        }

        public static Expression CreateQueryable(Type elementType, IModel model)
        {
            // TODO: Owned entities split over multiple tables

            // TODO: Distinct entities split over a single table

            // TODO: Handle shadow properties

            // TODO: Handle model properties mapped to CLR fields

            // TODO: Lazy loading / proxies

            var targetType = model.GetEntityTypes().SingleOrDefault(t => t.ClrType == elementType);
            var rootType = targetType.RootType();
            var derivedTypes = targetType.GetDerivedTypes().ToArray();

            Expression materializer;

            var schema = rootType.Relational().Schema ?? model.Relational().DefaultSchema;
            var table = new BaseTableExpression(schema, rootType.Relational().TableName, "t", rootType.ClrType);
            var hierarchy = targetType.GetDerivedTypes().Prepend(targetType).ToArray();
            var keyParameter = Expression.Parameter(targetType.ClrType);

            var keyExpression
                = Expression.Lambda(
                    Expression.NewArrayInit(
                        typeof(object),
                        from p in targetType.FindPrimaryKey().Properties
                        let c = Expression.MakeMemberAccess(keyParameter, GetMemberInfo(p))
                        select Expression.Convert(c, typeof(object))),
                    keyParameter);

            if (hierarchy.Length == 1)
            {
                // No inheritance

                materializer 
                    = Expression.MemberInit(
                        GetNewExpression(targetType),
                        GetBindings(targetType, p => GetSqlColumnExpression(table, p)));
            }
            else if (hierarchy.All(t => t.Relational().Schema == rootType.Relational().Schema && t.Relational().TableName == rootType.Relational().TableName))
            {
                // Table-per-hierarchy inheritance

                var properties
                    = (from t in hierarchy
                       from p in IterateProperties(t)
                       group p by GetRelationalId(p) into g
                       select (id: g.Key, property: g.First())).ToArray();

                var columns
                    = (from p in properties.Select(p => p.property)
                       select GetSqlColumnExpression(table, p)).ToArray();

                var tupleType = ValueTupleHelper.CreateTupleType(columns.Select(c => c.Type));
                var tupleNewExpression = ValueTupleHelper.CreateNewExpression(tupleType, columns);
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

                    var bindings 
                        = GetBindings(type, p => Expression.Convert(
                            ValueTupleHelper.CreateMemberExpression(
                                tupleType,
                                tupleParameter,
                                Array.FindIndex(properties, q => q.id.Equals(GetRelationalId(p)))),
                            GetMemberInfo(p).GetMemberType()));
                    
                    var descriptorMaterializer
                        = Expression.Lambda(
                             Expression.MemberInit(
                                 GetNewExpression(type),
                                 bindings),
                             tupleParameter);

                    descriptors.Add(new PolymorphicTypeDescriptor(type.ClrType, test, descriptorMaterializer));
                }

                materializer = new PolymorphicExpression(targetType.ClrType, tupleNewExpression, descriptors);
            }
            else
            {
                // Waiting on EF Core:

                // TODO: Table-per-type polymorphism

                // TODO: Table-per-concrete polymorphism

                throw new NotSupportedException();
            }

            var projection 
                = new ServerProjectionExpression(
                    new EntityMaterializationExpression(
                        targetType,
                        EntityState.Detached,
                        keyExpression,
                        materializer));

            var selectExpression = new SelectExpression(projection, table);

            if (targetType.QueryFilter != null)
            {
                selectExpression 
                    = selectExpression.AddToPredicate(new QueryFilterExpression(
                        targetType.QueryFilter.ExpandParameters(materializer)));
            }

            return new EnumerableRelationalQueryExpression(selectExpression);
        }

        private static NewExpression GetNewExpression(IEntityType type)
        {
            // TODO: Constructor selection with property-parameter mappings

            // TODO: Constructor selection with dependency injection

            return Expression.New(type.ClrType);
        }

        private static IEnumerable<MemberBinding> GetBindings(IEntityType type, Func<IProperty, Expression> func)
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
                    var newExpression = GetNewExpression(navigation.GetTargetType());
                    var bindings = GetBindings(navigation.GetTargetType(), func);

                    yield return Expression.Bind(
                        GetMemberInfo(navigation), 
                        Expression.MemberInit(newExpression, bindings));
                }
            }
        }

        private static IEnumerable<IProperty> IterateProperties(IEntityType type)
        {
            foreach (var property in type.GetProperties())
            {
                if (!property.IsShadowProperty)
                {
                    yield return property;
                }
            }

            foreach (var navigation in type.GetNavigations())
            {
                if (navigation.ForeignKey.IsOwnership)
                {
                    foreach (var property in IterateProperties(navigation.GetTargetType()))
                    {
                        yield return property;
                    }
                }
            }
        }

        private static SqlColumnExpression GetSqlColumnExpression(AliasedTableExpression table, IProperty property)
        {
            return new SqlColumnExpression(table, property.Relational().ColumnName, property.ClrType, property.IsNullable);
        }

        private static (string, string, string) GetRelationalId(IProperty property)
        {
            var table = property.DeclaringEntityType.Relational();

            return (table.Schema, table.TableName, property.Relational().ColumnName);
        }

        private static MemberInfo GetMemberInfo(IPropertyBase property)
        {
            return property.PropertyInfo ?? (MemberInfo)property.FieldInfo;
        }
    }
}
