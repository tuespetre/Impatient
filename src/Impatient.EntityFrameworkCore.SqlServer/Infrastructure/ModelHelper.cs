using Impatient.Metadata;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal static class ModelHelper
    {
        public static LambdaExpression CreateKeySelector(Type type, IReadOnlyList<IProperty> properties)
        {
            var parameter = Expression.Parameter(type);

            if (properties.Count == 1)
            {
                return Expression.Lambda(Expression.Property(parameter, properties[0].PropertyInfo), parameter);
            }
            else
            {
                var tuple = ValueTupleHelper.CreateTupleType(properties.Select(prop => prop.ClrType));
                var reads = properties.Select(p => Expression.Property(parameter, p.PropertyInfo));

                return Expression.Lambda(ValueTupleHelper.CreateNewExpression(tuple, reads), parameter);
            }
        }

        public static IEnumerable<PrimaryKeyDescriptor> CreatePrimaryKeyDescriptors(IModel model)
        {
            return from t in model.GetEntityTypes()
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
                        Member = fk.PrincipalToDependent.PropertyInfo,
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
                        Member = fk.DependentToPrincipal.PropertyInfo,
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
            var targetType = model.GetEntityTypes().SingleOrDefault(t => t.ClrType == elementType);
            var rootType = targetType.RootType();
            var derivedTypes = targetType.GetDerivedTypes().ToArray();

            // 2.0: higher priority

            // TODO: Handle shadow properties

            // TODO: Handle model properties mapped to CLR fields

            // TODO: Owned entities in a single table

            // TODO: Owned entities split across tables

            // 2.1: lower priority

            // TODO: Constructor selection with property-parameter mappings

            // TODO: Constructor selection with dependency injection

            // TODO: Lazy loading / proxies

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
                        let c = Expression.MakeMemberAccess(keyParameter, p.PropertyInfo)
                        select Expression.Convert(c, typeof(object))),
                    keyParameter);

            if (hierarchy.Length == 1)
            {
                // No inheritance

                materializer
                    = Expression.MemberInit(
                        Expression.New(elementType),
                        from property in targetType.GetProperties()
                        let column = new SqlColumnExpression(table, property.Relational().ColumnName, property.ClrType, property.IsNullable)
                        select Expression.Bind(property.PropertyInfo, column));
            }
            else if (hierarchy.All(t => t.Relational().Schema == rootType.Relational().Schema && t.Relational().TableName == rootType.Relational().TableName))
            {
                // Table-per-hierarchy inheritance

                var properties
                    = (from t in hierarchy
                       from p in t.GetProperties()
                       let r = t.Relational()
                       group p by (r.Schema, r.TableName, p.Relational().ColumnName) into g
                       select g.First()).ToArray();

                var columns
                    = (from p in properties
                       select new SqlColumnExpression(table, p.Relational().ColumnName, p.ClrType, p.IsNullable)).ToArray();

                var tupleType = ValueTupleHelper.CreateTupleType(columns.Select(c => c.Type));
                var tupleNewExpression = ValueTupleHelper.CreateNewExpression(tupleType, columns);
                var tupleParameter = Expression.Parameter(tupleType);

                var descriptors
                    = (from type in hierarchy
                       where !type.ClrType.IsAbstract
                       let discriminator = Array.IndexOf(properties, type.Relational().DiscriminatorProperty)
                       select new PolymorphicTypeDescriptor(
                           type.ClrType,
                           Expression.Lambda(
                             Expression.Equal(
                                 ValueTupleHelper.CreateMemberExpression(tupleType, tupleParameter, discriminator),
                                 Expression.Constant(type.Relational().DiscriminatorValue)),
                             tupleParameter),
                           Expression.Lambda(
                             Expression.MemberInit(
                                 Expression.New(type.ClrType),
                                 (from p in type.GetProperties()
                                  let i = Array.IndexOf(properties, p)
                                  let accessor = ValueTupleHelper.CreateMemberExpression(tupleType, tupleParameter, i)
                                  select Expression.Bind(p.PropertyInfo, Expression.Convert(accessor, p.PropertyInfo.PropertyType)))),
                             tupleParameter))).ToArray();

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
    }
}
