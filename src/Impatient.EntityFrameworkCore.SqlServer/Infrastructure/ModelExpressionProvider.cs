using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    using System.Linq;

    public class ModelExpressionProvider
    {
        private static readonly MethodInfo efPropertyMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition<object, string>(obj => EF.Property<string>(obj, "key"));

        private static readonly MethodInfo queryableWhereMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IQueryable<bool> e) => e.Where(x => x));

        private readonly IRelationalTypeMappingSource relationalTypeMappingSource;

        public ModelExpressionProvider(IRelationalTypeMappingSource relationalTypeMappingSource)
        {
            this.relationalTypeMappingSource = relationalTypeMappingSource ?? throw new ArgumentNullException(nameof(relationalTypeMappingSource));
        }

        private LambdaExpression CreateNavigationKeySelector(Type type, IReadOnlyList<IProperty> properties)
        {
            var entityParameter = Expression.Parameter(type, type.Name.ToLowerInvariant().Substring(0, 1));

            var expressions
                = (from p in properties
                   select p.IsShadowProperty
                     ? (Expression)Expression.Call(
                         efPropertyMethodInfo.MakeGenericMethod(p.ClrType),
                         entityParameter,
                         Expression.Constant(p.Name))
                     : Expression.MakeMemberAccess(entityParameter, p.GetReadableMemberInfo())).ToArray();

            return Expression.Lambda(
                properties.Count == 1
                    ? expressions[0]
                    : Expression.NewArrayInit(
                        typeof(object),
                        from e in expressions
                        select Expression.Convert(e, typeof(object))),
                entityParameter);
        }

        private LambdaExpression CreateMaterializationKeySelector(IEntityType type)
        {
            var primaryKey = type.FindPrimaryKey();

            if (primaryKey == null)
            {
                return null;
            }

            var entityParameter = Expression.Parameter(type.ClrType, "entity");
            var shadowPropertiesParameter = Expression.Parameter(typeof(object[]), "shadow");

            return Expression.Lambda(
                Expression.NewArrayInit(
                    typeof(object),
                    from p in type.FindPrimaryKey().Properties
                    select Expression.Convert(
                        p.IsShadowProperty
                            ? Expression.ArrayIndex(shadowPropertiesParameter, Expression.Constant(p.GetShadowIndex()))
                            : (Expression)Expression.MakeMemberAccess(entityParameter, p.GetReadableMemberInfo()),
                        typeof(object))),
                entityParameter,
                shadowPropertiesParameter);
        }

        public IEnumerable<PrimaryKeyDescriptor> CreatePrimaryKeyDescriptors(DbContext context)
        {
            return from t in context.Model.GetEntityTypes()
                   //where !t.IsOwned()
                   let k = t.FindPrimaryKey()
                   where k != null
                   select new PrimaryKeyDescriptor(
                       t.ClrType,
                       CreateNavigationKeySelector(k.DeclaringEntityType.ClrType, k.Properties));
        }

        public IEnumerable<NavigationDescriptor> CreateNavigationDescriptors(DbContext context)
        {
            var fks = new List<IForeignKey>();

            foreach (var type in context.Model.GetEntityTypes())
            {
                foreach (var navigation in type.GetNavigations())
                {
                    if (navigation.ForeignKey.IsOwnership)
                    {
                        var source = type.Relational();
                        var target = navigation.GetTargetType().Relational();

                        if (source.Schema == target.Schema
                            && source.TableName == target.TableName)
                        {
                            continue;
                        }
                    }

                    fks.Add(navigation.ForeignKey);
                }
            }

            foreach (var fk in fks.Distinct())
            {
                var principal = CreateNavigationKeySelector(fk.PrincipalEntityType.ClrType, fk.PrincipalKey.Properties);
                var dependent = CreateNavigationKeySelector(fk.DeclaringEntityType.ClrType, fk.Properties);

                if (fk.PrincipalToDependent != null)
                {
                    yield return new NavigationDescriptor(
                        fk.PrincipalEntityType.ClrType,
                        fk.PrincipalToDependent.GetReadableMemberInfo(),
                        principal,
                        dependent,
                        true,
                        CreateQueryExpression(fk.DeclaringEntityType.ClrType, context));
                }

                if (fk.DependentToPrincipal != null)
                {
                    yield return new NavigationDescriptor(
                        fk.DeclaringEntityType.ClrType,
                        fk.DependentToPrincipal.GetReadableMemberInfo(),
                        dependent,
                        principal,
                        !fk.IsRequired,
                        CreateQueryExpression(fk.PrincipalEntityType.ClrType, context));
                }
            }
        }

        private static bool IsTablePerHierarchy(IEntityType rootType, IEnumerable<IEntityType> hierarchy)
        {
            return hierarchy.All(t =>
                t.Relational().Schema == rootType.Relational().Schema
                && t.Relational().TableName == rootType.Relational().TableName);
        }

        public Expression CreateQueryExpression(Type elementType, DbContext context)
        {
            // TODO: (EF Core 2.0) Distinct entities split over a single table

            // TODO: (EF Core 2.0) Handle model properties mapped to CLR fields

            // TODO: (EF Core 2.1) Lazy loading / proxies

            var targetType = context.Model.GetEntityTypes().SingleOrDefault(t => t.ClrType == elementType);

            Expression queryExpression;

            if (targetType.DefiningQuery != null)
            {
                queryExpression = targetType.DefiningQuery.Body;

                goto ApplyQueryFilters;
            }

            var rootType = targetType.RootType();

            var schemaName = rootType.Relational().Schema ?? context.Model.Relational().DefaultSchema;
            var tableName = rootType.Relational().TableName;

            var table
                = new BaseTableExpression(
                    schemaName,
                    tableName,
                    tableName.Substring(0, 1).ToLower(),
                    rootType.ClrType);

            var materializer = CreateMaterializer(targetType, table);

            var projection = new ServerProjectionExpression(materializer);

            var selectExpression = new SelectExpression(projection, table);

            var discriminatingType = targetType;

            while (discriminatingType != null)
            {
                var discriminatorProperty = discriminatingType.Relational().DiscriminatorProperty;

                if (discriminatorProperty != null)
                {
                    selectExpression
                       = selectExpression.AddToPredicate(
                           new SqlInExpression(
                               MakeColumnExpression(
                                   table,
                                   discriminatorProperty),
                               Expression.NewArrayInit(
                                   discriminatorProperty.ClrType,
                                   from t in discriminatingType.GetDerivedTypesInclusive()
                                   where !t.IsAbstract()
                                   select Expression.Constant(
                                        t.Relational().DiscriminatorValue,
                                        discriminatorProperty.ClrType))));
                }

                discriminatingType = FindSameTabledPrincipalType(discriminatingType);
            }

            queryExpression = new EnumerableRelationalQueryExpression(selectExpression);

            ApplyQueryFilters:

            var currentType = targetType;

            while (currentType != null)
            {
                if (currentType.QueryFilter != null)
                {
                    var filterBody = currentType.QueryFilter.Body;
                    
                    var repointer 
                        = new QueryFilterRepointingExpressionVisitor(
                            DbContextParameter.GetInstance(context.GetType()));

                    filterBody = repointer.Visit(filterBody);

                    // Use a method call instead of adding to the SelectExpression
                    // so the rewriting visitors are guaranteed to get their hands on the 
                    // filter.
                    queryExpression
                        = Expression.Call(
                            queryableWhereMethodInfo.MakeGenericMethod(currentType.ClrType),
                            queryExpression,
                            Expression.Quote(
                                Expression.Lambda(
                                    new QueryFilterExpression(filterBody),
                                    currentType.QueryFilter.Parameters)));
                }

                currentType = currentType.BaseType;
            }

            return queryExpression;
        }

        private Expression CreateMaterializer(IEntityType targetType, BaseTableExpression table)
        {
            var rootType = targetType.RootType();
            var hierarchy = rootType.GetDerivedTypes().Prepend(rootType).ToArray();
            var keyExpression = CreateMaterializationKeySelector(targetType);

            if (hierarchy.Length == 1)
            {
                // No inheritance

                return CreateMaterializationExpression(targetType, table, p => MakeColumnExpression(table, p));
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

                Expression MakeTupleColumnExpression(IProperty property)
                {
                    return Expression.Convert(
                        ValueTupleHelper.CreateMemberExpression(
                            tupleType,
                            tupleParameter,
                            Array.FindIndex(properties, q => q.id.Equals(GetRelationalId(property)))),
                        property.ClrType);
                };

                var concreteTypes = hierarchy.Where(t => !t.ClrType.IsAbstract).ToArray();
                var descriptors = new PolymorphicTypeDescriptor[concreteTypes.Length];

                for (var i = 0; i < concreteTypes.Length; i++)
                {
                    var type = concreteTypes[i];
                    var relational = type.Relational();

                    var test
                        = Expression.Lambda(
                            Expression.Equal(
                                ValueTupleHelper.CreateMemberExpression(
                                    tupleType, 
                                    tupleParameter,
                                    Array.FindIndex(properties, p => p.property == relational.DiscriminatorProperty)),
                                Expression.Constant(relational.DiscriminatorValue)),
                            tupleParameter);

                    var descriptorMaterializer
                        = Expression.Lambda(
                            CreateMaterializationExpression(type, table, MakeTupleColumnExpression),
                            tupleParameter);

                    descriptors[i] = new PolymorphicTypeDescriptor(type.ClrType, test, descriptorMaterializer);
                }

                return new PolymorphicExpression(
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
        }

        private ExtendedNewExpression CreateNewExpression(IEntityType type, BaseTableExpression table, Func<IProperty, Expression> makeColumnExpression)
        {
            var constructorBinding = (ConstructorBinding)type[nameof(ConstructorBinding)];

            switch (constructorBinding)
            {
                case DirectConstructorBinding directConstructorBinding:
                {
                    return CreateNewExpression(type, directConstructorBinding, table, makeColumnExpression);
                }

                case FactoryMethodConstructorBinding factoryMethodConstructorBinding:
                {
                    return CreateNewExpression(type, factoryMethodConstructorBinding, table, makeColumnExpression);
                }

                case null:
                {
                    return new ExtendedNewExpression(type.ClrType);
                }

                default:
                {
                    throw new NotSupportedException($"The {constructorBinding.GetType().Name} is not supported.");
                }
            }
        }

        private ExtendedNewExpression CreateNewExpression(IEntityType type, DirectConstructorBinding directConstructorBinding, BaseTableExpression table, Func<IProperty, Expression> makeColumnExpression)
        {
            var constructor = directConstructorBinding.Constructor;
            var arguments = new Expression[constructor.GetParameters().Length];
            var readableMembers = new MemberInfo[arguments.Length];
            var writableMembers = new MemberInfo[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                var binding = directConstructorBinding.ParameterBindings[i];

                arguments[i] = GetBindingExpression(type, binding, makeColumnExpression);

                if (binding.ConsumedProperties.ElementAtOrDefault(0) is IPropertyBase property)
                {
                    readableMembers[i] = property.GetReadableMemberInfo();
                    writableMembers[i] = property.GetWritableMemberInfo();
                }
            }

            return new ExtendedNewExpression(constructor, arguments, readableMembers, writableMembers);
        }

        private ExtendedNewExpression CreateNewExpression(IEntityType type, FactoryMethodConstructorBinding factoryMethodConstructorBinding, BaseTableExpression table, Func<IProperty, Expression> makeColumnExpression)
        {
            var factoryInstance
                = typeof(FactoryMethodConstructorBinding)
                    .GetField("_factoryInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(factoryMethodConstructorBinding);

            var factoryMethod
                = typeof(FactoryMethodConstructorBinding)
                    .GetField("_factoryMethod", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(factoryMethodConstructorBinding) as MethodInfo;

            if (factoryInstance is null && (factoryMethod is null || !factoryMethod.IsStatic))
            {
                throw new NotSupportedException();
            }

            var bindings = factoryMethodConstructorBinding.ParameterBindings;

            if (!(bindings.Count == 3
                && bindings[0] is EntityTypeParameterBinding entityTypeParameterBinding
                && bindings[1] is DefaultServiceParameterBinding defaultServiceParameterBinding
                && bindings[2] is ObjectArrayParameterBinding objectArrayParameterBinding))
            {
                throw new NotSupportedException();
            }

            var innerBindings
                = typeof(ObjectArrayParameterBinding)
                    .GetField("_bindings", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.GetValue(objectArrayParameterBinding) as IReadOnlyList<ParameterBinding>;

            if (innerBindings is null)
            {
                throw new NotSupportedException();
            }

            var constructor 
                = factoryMethodConstructorBinding.RuntimeType
                    .GetConstructor(innerBindings.Select(b => b.ParameterType).ToArray());

            var arguments = new Expression[innerBindings.Count];
            var readableMembers = new MemberInfo[arguments.Length];
            var writableMembers = new MemberInfo[arguments.Length];

            for (var i = 0; i < arguments.Length; i++)
            {
                var binding = innerBindings[i];

                arguments[i] = GetBindingExpression(type, binding, makeColumnExpression);

                if (binding.ConsumedProperties.ElementAtOrDefault(0) is IPropertyBase property)
                {
                    readableMembers[i] = property.GetReadableMemberInfo();
                    writableMembers[i] = property.GetWritableMemberInfo();
                }
            }

            return new EFCoreProxyNewExpression(
                factoryInstance is null ? null : Expression.Constant(factoryInstance),
                factoryMethod,
                new[]
                {
                    GetBindingExpression(type, entityTypeParameterBinding, makeColumnExpression),
                    GetBindingExpression(type, defaultServiceParameterBinding, makeColumnExpression)
                },
                constructor,
                arguments,
                readableMembers,
                writableMembers);
        }

        private Expression CreateMaterializationExpression(IEntityType type, BaseTableExpression table, Func<IProperty, Expression> makeColumnExpression)
        {
            var properties
                = (from p in type.GetProperties()
                   where !p.IsShadowProperty
                   select p).ToList();

            var navigations
                = (from n in type.GetNavigations()
                   where n.ForeignKey.IsOwnership && !n.IsDependentToPrincipal()
                   select n).ToList();

            var services
                = (from s in type.GetServiceProperties()
                   select s).ToList();

            var newExpression = CreateNewExpression(type, table, makeColumnExpression);

            properties.RemoveAll(p => newExpression.WritableMembers.Contains(p.GetWritableMemberInfo()));
            navigations.RemoveAll(p => newExpression.WritableMembers.Contains(p.GetWritableMemberInfo()));
            services.RemoveAll(p => newExpression.WritableMembers.Contains(p.GetWritableMemberInfo()));

            var arguments = new Expression[properties.Count + navigations.Count + services.Count];
            var readableMembers = new MemberInfo[arguments.Length];
            var writableMembers = new MemberInfo[arguments.Length];

            var c = properties.Count;
            var d = 0;

            for (var i = 0; i < c; i++)
            {
                var property = properties[i - d];

                arguments[i] = makeColumnExpression(property);
                readableMembers[i] = property.GetReadableMemberInfo();
                writableMembers[i] = property.GetWritableMemberInfo();
            }

            c += navigations.Count;
            d += properties.Count;

            for (var i = d; i < c; i++)
            {
                var navigation = navigations[i - d];

                arguments[i] = CreateMaterializationExpression(navigation.GetTargetType(), table, makeColumnExpression);
                readableMembers[i] = navigation.GetReadableMemberInfo();
                writableMembers[i] = navigation.GetWritableMemberInfo();
            }

            c += services.Count;
            d += navigations.Count;

            for (var i = d; i < c; i++)
            {
                var service = services[i - d];

                arguments[i] = GetBindingExpression(type, service.GetParameterBinding(), makeColumnExpression);
                readableMembers[i] = service.GetReadableMemberInfo();
                writableMembers[i] = service.GetWritableMemberInfo();
            }

            var materializer
                = new ExtendedMemberInitExpression(
                    type.ClrType,
                    newExpression,
                    arguments,
                    readableMembers,
                    writableMembers);

            var keySelector
                = CreateMaterializationKeySelector(type);

            if (keySelector != null)
            {
                var shadowProperties
                    = from p in type.GetProperties()
                      where p.IsShadowProperty
                      select (property: p, expression: makeColumnExpression(p));

                return new EntityMaterializationExpression(
                    type,
                    IdentityMapMode.IdentityMap,
                    keySelector,
                    shadowProperties.Select(s => s.property),
                    shadowProperties.Select(s => s.expression),
                    materializer);
            }

            return materializer;
        }

        private Expression GetBindingExpression(IEntityType type, ParameterBinding binding, Func<IProperty, Expression> makeColumnExpression)
        {
            switch (binding)
            {
                case ServiceMethodParameterBinding service:
                {
                    return new ContextServiceDelegateInjectionExpression(
                        service.ParameterType,
                        service.ServiceType,
                        service.Method);
                }

                case DefaultServiceParameterBinding service:
                {
                    return new ContextServiceInjectionExpression(service.ParameterType);
                }

                case PropertyParameterBinding _
                when binding.ConsumedProperties[0] is IProperty property:
                {
                    return makeColumnExpression(property);
                }

                case EntityTypeParameterBinding _:
                {
                    return new EntityTypeInjectionExpression(type);
                }

                case ContextParameterBinding _:
                {
                    return new ContextServiceInjectionExpression(binding.ParameterType);
                }

                default:
                {
                    throw new NotSupportedException($"The {binding.GetType().Name} is not supported.");
                }
            }
        }

        private IEnumerable<IProperty> IterateAllProperties(IEntityType type)
        {
            foreach (var property in type.GetProperties())
            {
                yield return property;
            }

            foreach (var navigation in type.GetNavigations())
            {
                if (navigation.ForeignKey.IsOwnership && !navigation.IsDependentToPrincipal())
                {
                    var targetType = navigation.GetTargetType();

                    if (targetType.Relational().Schema != type.Relational().Schema
                        || targetType.Relational().TableName != type.Relational().TableName)
                    {
                        continue;
                    }

                    foreach (var property in IterateAllProperties(navigation.GetTargetType()))
                    {
                        yield return property;
                    }
                }
            }
        }

        private Expression MakeColumnExpression(AliasedTableExpression table, IProperty property)
        {
            ITypeMapping typeMapping = default;

            var sourceMapping = relationalTypeMappingSource.FindMapping(property);

            if (!(sourceMapping is null))
            {
                typeMapping
                    = new AdHocTypeMapping(
                        sourceMapping.ClrType,
                        sourceMapping.Converter?.ProviderClrType ?? sourceMapping.ClrType,
                        sourceMapping.DbType,
                        sourceMapping.StoreType,
                        sourceMapping.Converter?.ConvertFromProviderExpression,
                        sourceMapping.Converter?.ConvertToProviderExpression);
            }

            var nullable = GetColumnNullability(property);

            return new SqlColumnExpression(
                table,
                property.Relational().ColumnName,
                property.ClrType,
                nullable,
                typeMapping);
        }

        private bool GetColumnNullability(IProperty property)
        {
            // If the property is nullable, it can be null, who would've thought

            if (property.IsNullable)
            {
                return true;
            }

            // If the property is declared by a derived type or a type owned by a derived type, it can be null

            var resolvedEntityType = property.DeclaringEntityType;

            while (resolvedEntityType.IsOwned())
            {
                resolvedEntityType = resolvedEntityType.FindOwnership().PrincipalToDependent.DeclaringEntityType;
            }

            if (resolvedEntityType != resolvedEntityType.RootType())
            {
                return true;
            }

            // If the property is declared by an owned type of a root type and not declared nullable, it can't be null

            if (property.DeclaringEntityType.IsOwned())
            {
                return false;
            }

            // If the property is part of a foreign key within the same table as the principal type,
            // but the principal type is derived, it can be null

            var tableId = GetRelationalId(property.DeclaringEntityType);

            foreach (var foreignKey in property.DeclaringEntityType.GetForeignKeys())
            {
                var principalType = foreignKey.PrincipalEntityType;

                if (tableId.Equals(GetRelationalId(principalType))
                    && principalType != principalType.RootType())
                {
                    return true;
                }
            }

            return false;
        }

        private IEntityType FindSameTabledPrincipalType(IEntityType dependentType)
        {
            var tableId = GetRelationalId(dependentType);

            if (dependentType.FindOwnership() is IForeignKey ownership)
            {
                if (tableId.Equals(GetRelationalId(ownership.PrincipalEntityType)))
                {
                    return ownership.PrincipalEntityType;
                }
                else
                {
                    return null;
                }
            }

            foreach (var foreignKey in dependentType.GetForeignKeys())
            {
                var principalType = foreignKey.PrincipalEntityType;

                if (tableId.Equals(GetRelationalId(principalType))
                    && principalType != principalType.RootType()
                    && principalType.RootType() != dependentType.RootType())
                {
                    return principalType;
                }
            }

            return null;
        }

        private static (string, string) GetRelationalId(IEntityType entityType)
        {
            var relational = entityType.Relational();

            return (relational.Schema, relational.TableName);
        }

        private static (string, string, string) GetRelationalId(IProperty property)
        {
            var relational = property.DeclaringEntityType.Relational();

            return (relational.Schema, relational.TableName, property.Relational().ColumnName);
        }
    }
}
