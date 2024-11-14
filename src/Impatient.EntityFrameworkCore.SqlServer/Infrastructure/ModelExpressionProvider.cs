using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;
using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Projection;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public class ModelExpressionProvider
{
    private static readonly MethodInfo efPropertyMethodInfo
        = ReflectionExtensions.GetGenericMethodDefinition<object, string>(obj => EF.Property<string>(obj, "key"));

    private static readonly MethodInfo queryableWhereMethodInfo
        = ReflectionExtensions.GetGenericMethodDefinition((IQueryable<bool> e) => e.Where(x => x));

    private static readonly MethodInfo queryableCastMethodInfo
        = ReflectionExtensions.GetMethodInfo(() => Queryable.Cast<object>(default)).GetGenericMethodDefinition();

    private readonly IRelationalTypeMappingSource relationalTypeMappingSource;

    public ModelExpressionProvider(IRelationalTypeMappingSource relationalTypeMappingSource)
    {
        this.relationalTypeMappingSource = relationalTypeMappingSource ?? throw new ArgumentNullException(nameof(relationalTypeMappingSource));
    }

    private static LambdaExpression CreateNavigationKeySelector(Type type, IReadOnlyList<IProperty> properties)
    {
        var entityParameter = Expression.Parameter(type, type.Name.ToLowerInvariant().Substring(0, 1));

        var expressions = new Expression[properties.Count];

        for (var i = 0; i < expressions.Length; i++)
        {
            var p = properties[i];

            if (p.IsShadowProperty())
            {
                expressions[i] = Expression.Call(
                     efPropertyMethodInfo.MakeGenericMethod(p.ClrType),
                     entityParameter,
                     Expression.Constant(p.Name));

                continue;
            }

            var member = p.GetSemanticReadableMemberInfo();

            if (member is FieldInfo)
            {
                expressions[i] = Expression.MakeMemberAccess(
                    entityParameter,
                    member);

                continue;
            }
            else if (member is PropertyInfo property)
            {
                var indexParameters = property.GetIndexParameters();

                if (indexParameters.Length == 0)
                {
                    expressions[i] = Expression.MakeMemberAccess(
                        entityParameter,
                        member);

                    continue;
                }
                else if (indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(string))
                {
                    expressions[i] = Expression.MakeIndex(
                        entityParameter,
                        property,
                        [Expression.Constant(p.Name)]);

                    continue;
                }
            }

            throw new InvalidOperationException("Unable to create foreign key selector");
        }

        return Expression.Lambda(
            properties.Count == 1
                ? expressions[0]
                : Expression.NewArrayInit(
                    typeof(object),
                    from e in expressions
                    select Expression.Convert(e, typeof(object))),
            entityParameter);
    }

    private static LambdaExpression CreateMaterializationKeySelector(IEntityType targetType)
    {
        var primaryKey = targetType.FindPrimaryKey();

        if (primaryKey is null)
        {
            return null;
        }

        var entityParameter = Expression.Parameter(targetType.ClrType, "entity");
        var shadowPropertiesParameter = Expression.Parameter(typeof(object[]), "shadow");

        return Expression.Lambda(
            Expression.NewArrayInit(
                typeof(object),
                targetType.FindPrimaryKey().Properties.Select(p =>
                {
                    if (p.IsShadowProperty())
                    {
                        return Expression.Convert(Expression.ArrayIndex(shadowPropertiesParameter, Expression.Constant(p.GetShadowIndex())), typeof(object));
                    }

                    var member = p.GetSemanticReadableMemberInfo();

                    if (member is FieldInfo)
                    {
                        return Expression.Convert(Expression.MakeMemberAccess(entityParameter, member), typeof(object));
                    }
                    else if (member is PropertyInfo property)
                    {
                        var indexParameters = property.GetIndexParameters();

                        if (indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(string))
                        {
                            return Expression.Convert(Expression.MakeIndex(entityParameter, property, [Expression.Constant(p.Name)]), typeof(object));
                        }
                        else if (indexParameters.Length == 0)
                        {
                            return Expression.Convert(Expression.MakeMemberAccess(entityParameter, member), typeof(object));
                        }
                    }

                    throw new InvalidOperationException("Unable to create materialization key selector");
                })),
            entityParameter,
            shadowPropertiesParameter);
    }

    public IEnumerable<PrimaryKeyDescriptor> CreatePrimaryKeyDescriptors(DbContext context)
    {
        return from t in context.Model.GetEntityTypes()
               let k = t.FindPrimaryKey()
               where k is not null
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
                    var source = type;
                    var target = navigation.TargetEntityType;

                    if (source.GetSchema() == target.GetSchema()
                        && source.GetTableName() == target.GetTableName())
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

            if (fk.PrincipalToDependent is not null)
            {
                yield return new NavigationDescriptor(
                    fk.PrincipalEntityType.ClrType,
                    fk.PrincipalToDependent.GetSemanticReadableMemberInfo(),
                    principal,
                    dependent,
                    true,
                    CreateQueryExpression(fk.DeclaringEntityType, context));
            }

            if (fk.DependentToPrincipal is not null)
            {
                yield return new NavigationDescriptor(
                    fk.DeclaringEntityType.ClrType,
                    fk.DependentToPrincipal.GetSemanticReadableMemberInfo(),
                    dependent,
                    principal,
                    !fk.IsRequired,
                    CreateQueryExpression(fk.PrincipalEntityType, context));
            }
        }
    }

    public Expression CreateQueryExpression(IEntityType targetType, DbContext context)
    {
        Expression queryExpression;

        if (targetType.GetDefiningQuery() is LambdaExpression definingQueryLambda)
        {
            queryExpression = definingQueryLambda.Body;
        }
        else
        {
            switch (targetType.GetMappingStrategy())
            {
                case null:
                    queryExpression = CreateNonPolymorphicQueryExpression(targetType);
                    break;
                case "TPH":
                    queryExpression = CreateTPHQueryExpression(targetType);
                    break;
                case "TPC":
                    queryExpression = CreateTPCQueryExpression(targetType);
                    break;
                case "TPT":
                    queryExpression = CreateTPTQueryExpression(targetType);
                    break;
                default:
                    throw new NotSupportedException($"Impatient does not support \"{targetType.GetMappingStrategy()}\" mapping strategy");
            }
        }

        var currentType = targetType;
        var recast = false;

        while (currentType is not null)
        {
            if (currentType.GetQueryFilter() is not null)
            {
                var filterBody = currentType.GetQueryFilter().Body;

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
                                currentType.GetQueryFilter().Parameters)));

                recast |= currentType != targetType;
            }

            currentType = currentType.BaseType;
        }

        if (recast)
        {
            queryExpression
                = Expression.Call(
                    queryableCastMethodInfo.MakeGenericMethod(targetType.ClrType),
                    queryExpression);
        }

        return queryExpression;
    }

    private EnumerableRelationalQueryExpression CreateNonPolymorphicQueryExpression(IEntityType targetType)
    {
        ThrowForUnsupportedMappings(targetType);

        if (targetType.GetTableMappings().Any())
        {
            return CreateNonPolymorphicQueryExpressionFromTableMappings(targetType);
        }

        if (targetType.GetViewMappings().Any())
        {
            return CreateNonPolymorphicQueryExpressionFromViewMappings(targetType);
        }

        throw new InvalidOperationException("Could not find appropriate mappings for the entity type");
    }

    private static void ThrowForUnsupportedMappings(IEntityType targetType)
    {
        if (targetType.GetSqlQueryMappings().Any())
        {
            throw new NotSupportedException("Impatient does not support querying entities mapped to SQL strings");
        }

        if (IterateTableMappings(targetType, includeDerived: true).Any() || targetType.GetViewMappings().Any())
        {
            return;
        }

        if (targetType.GetFunctionMappings().Any())
        {
            throw new NotImplementedException("Impatient does not yet implement entities mapped to functions");
        }

        throw new InvalidOperationException("Could not find appropriate mappings for the entity type");
    }

    private EnumerableRelationalQueryExpression CreateNonPolymorphicQueryExpressionFromTableMappings(IEntityType targetType)
    {
        var properties
            = IterateProperties(targetType, includeDerived: false)
                .Distinct()
                .ToArray();

        var tableMappings
            = IterateTableMappings(targetType, includeDerived: false)
                .GroupBy(m => m.Table, (t, m) => m.First())
                .ToArray();

        var tables = tableMappings.Select(m => m.Table).Distinct().ToArray();

        var principalTable
            = new BaseTableExpression(
                tableMappings[0].Table.Schema,
                tableMappings[0].Table.Name,
                tableMappings[0].Table.Name[..1].ToLower(),
                tableMappings[0].TypeBase.ClrType);

        var tableLookup = new Dictionary<ITableBase, AliasedTableExpression> { [tableMappings[0].Table] = principalTable };

        TableExpression queryTable = principalTable;

        foreach (var tableMapping in tableMappings.Skip(1))
        {
            var nonPrincipalTable
                = new BaseTableExpression(
                    tableMapping.Table.Schema,
                    tableMapping.Table.Name,
                    tableMapping.Table.Name[..1].ToLower(),
                    tableMapping.TypeBase.ClrType);

            tableLookup[tableMapping.Table] = nonPrincipalTable;

            var predicate =
                tableMappings[0].Table.PrimaryKey.Columns
                    .Zip(tableMapping.Table.PrimaryKey.Columns)
                    .Select(t => Expression.Equal(
                        new SqlColumnExpression(principalTable, t.First.Name, t.First.ProviderClrType, false, null),
                        new SqlColumnExpression(nonPrincipalTable, t.Second.Name, t.Second.ProviderClrType, false, null)))
                    .Aggregate(Expression.AndAlso);

            queryTable = new LeftJoinTableExpression(queryTable, nonPrincipalTable, predicate, targetType.ClrType);
        }

        var propertyExpressions
            = (from p in properties
               from m in p.GetTableColumnMappings()
               where tables.Contains(m.Column.Table)
               group m.Column by p into g
               let p = g.Key
               let c = g.First()
               let t = tableLookup[c.Table]
               let x = MakeColumnExpression(t, c.Name, p)
               select (p, x)).ToDictionary(t => t.p, t => (Expression)t.x);

        var materializer
            = CreateEntityMaterializationExpression(
                targetType,
                tableLookup,
                propertyExpressions);

        var projection = new ServerProjectionExpression(materializer);

        var selectExpression = new SelectExpression(projection, queryTable);

        return new EnumerableRelationalQueryExpression(selectExpression);
    }

    private EnumerableRelationalQueryExpression CreateNonPolymorphicQueryExpressionFromViewMappings(IEntityType targetType)
    {
        var viewMappings = targetType.GetViewMappings().ToArray();

        var queryTable
            = new BaseTableExpression(
                viewMappings[0].View.Schema,
                viewMappings[0].View.Name,
                viewMappings[0].View.Name[..1].ToLower(),
                viewMappings[0].TypeBase.ClrType);

        var viewLookup = new Dictionary<ITableBase, AliasedTableExpression> { [viewMappings[0].View] = queryTable };

        var propertyExpressions
            = (from p in IterateProperties(targetType, includeDerived: false).Distinct()
               from m in p.GetViewColumnMappings()
               group m.Column by p into g
               let p = g.Key
               let c = g.First()
               let t = viewLookup[c.View]
               let x = MakeColumnExpression(t, c.Name, p)
               select (p, x)).ToDictionary(t => t.p, t => (Expression)t.x);

        var materializer
            = CreateEntityMaterializationExpression(
                targetType,
                viewLookup,
                propertyExpressions);

        var projection = new ServerProjectionExpression(materializer);

        var selectExpression = new SelectExpression(projection, queryTable);

        return new EnumerableRelationalQueryExpression(selectExpression);
    }

    private EnumerableRelationalQueryExpression CreateTPHQueryExpression(IEntityType targetType)
    {
        ThrowForUnsupportedMappings(targetType);

        var tableMappings
            = IterateTableMappings(targetType, includeDerived: true)
                .GroupBy(m => m.Table, (t, m) => m.First())
                .ToArray();

        Debug.Assert(tableMappings.Count(m => m.IsSharedTablePrincipal == true) < 2);

        var principalTable
            = new BaseTableExpression(
                tableMappings[0].Table.Schema,
                tableMappings[0].Table.Name,
                tableMappings[0].Table.Name[..1].ToLower(),
                targetType.ClrType);

        var tableLookup = new Dictionary<ITableBase, AliasedTableExpression> { [tableMappings[0].Table] = principalTable };

        TableExpression queryTable = principalTable;

        foreach (var mapping in tableMappings.Skip(1))
        {
            var nonPrincipalTable
                = new BaseTableExpression(
                    mapping.Table.Schema,
                    mapping.Table.Name,
                    mapping.Table.Name[..1].ToLower(),
                    mapping.TypeBase.ClrType);

            tableLookup[mapping.Table] = nonPrincipalTable;

            var predicate =
                tableMappings[0].Table.PrimaryKey.Columns
                    .Zip(mapping.Table.PrimaryKey.Columns)
                    .Select(t => Expression.Equal(
                        new SqlColumnExpression(principalTable, t.First.Name, t.First.ProviderClrType, false, null),
                        new SqlColumnExpression(nonPrincipalTable, t.Second.Name, t.Second.ProviderClrType, false, null)))
                    .Aggregate(Expression.AndAlso);

            queryTable = new LeftJoinTableExpression(queryTable, nonPrincipalTable, predicate, targetType.ClrType);
        }

        var hierarchy = targetType.GetDerivedTypesInclusive();

        var propertyMappings
            = (from p in IterateProperties(targetType, includeDerived: true).Distinct()
               from m in p.GetTableColumnMappings()
               select (Property: p, Mapping: m)).ToArray();

        var columnExpressions
            = (from p in propertyMappings
               group p.Property by p.Mapping.Column into properties
               let column = properties.Key
               let table = tableLookup[column.Table]
               let expression = MakeColumnExpression(table, column.Name, properties.First())
               select (expression, properties)).ToArray();

        var tupleType = ValueTupleHelper.CreateTupleType(columnExpressions.Select(c => c.expression.IsNullable ? c.expression.Type.AsNullableType() : c.expression.Type));
        var tupleParameter = Expression.Parameter(tupleType);

        var propertyExpressions
            = (from p in propertyMappings
               group p by p.Property into g
               let p = g.Key
               let expr = Expression.Convert(
                   ValueTupleHelper.CreateMemberExpression(
                       tupleType,
                       tupleParameter,
                       Array.FindIndex(columnExpressions, c => c.properties.Contains(p))),
                   p.ClrType)
               select (p, expr)).ToDictionary(x => x.p, x => (Expression)x.expr);

        var concreteTypes = hierarchy.Where(t => !t.IsAbstract()).ToArray();
        var descriptors = new PolymorphicTypeDescriptor[concreteTypes.Length];

        for (var i = 0; i < concreteTypes.Length; i++)
        {
            var concreteType = concreteTypes[i];

            var test
                = Expression.Lambda(
                    Expression.Equal(
                        ValueTupleHelper.CreateMemberExpression(
                            tupleType,
                            tupleParameter,
                            Array.FindIndex(columnExpressions, c => c.properties.Contains(concreteType.FindDiscriminatorProperty()))),
                        Expression.Constant(concreteType.GetDiscriminatorValue())),
                    tupleParameter);

            var descriptorMaterializer
                = Expression.Lambda(
                    CreateEntityMaterializationExpression(concreteType, tableLookup, propertyExpressions),
                    tupleParameter);

            descriptors[i] = new PolymorphicTypeDescriptor(concreteType.ClrType, test, descriptorMaterializer);
        }

        var materializer = new PolymorphicExpression(
            targetType.ClrType,
            ValueTupleHelper.CreateNewExpression(
                tupleType, 
                columnExpressions.Select(c => 
                    c.expression.IsNullable 
                        ? Expression.Convert(c.expression, c.expression.Type.AsNullableType()) 
                        : (Expression)c.expression)),
            descriptors).Filter(targetType.ClrType);

        var projection = new ServerProjectionExpression(materializer);

        var selectExpression = new SelectExpression(projection, queryTable);

        // for TPH, add a predicate to constrain discriminator values.

        var discriminatingType = targetType;

        while (discriminatingType is not null)
        {
            var discriminatorProperty = discriminatingType.FindDiscriminatorProperty();

            if (discriminatorProperty is not null)
            {
                selectExpression
                   = selectExpression.AddToPredicate(
                       new SqlInExpression(
                           MakeColumnExpression(
                               principalTable,
                               discriminatorProperty.GetTableColumnMappings().Select(m => m.Column).Distinct().Single().Name,
                               discriminatorProperty),
                           Expression.NewArrayInit(
                               discriminatorProperty.ClrType,
                               from t in discriminatingType.GetDerivedTypesInclusive()
                               where !t.IsAbstract()
                               select Expression.Constant(
                                    t.GetDiscriminatorValue(),
                                    discriminatorProperty.ClrType))));
            }

            discriminatingType = FindSameTabledPrincipalType(discriminatingType);
        }

        return new EnumerableRelationalQueryExpression(selectExpression);
    }

    private EnumerableRelationalQueryExpression CreateTPCQueryExpression(IEntityType targetType)
    {
        ThrowForUnsupportedMappings(targetType);

        var concreteTypes = targetType.GetConcreteDerivedTypesInclusive().ToArray();

        if (concreteTypes.Length == 1)
        {
            return CreateNonPolymorphicQueryExpressionFromTableMappings(concreteTypes[0]);
        }

        var targetProperties = IterateProperties(targetType, includeDerived: true).Distinct().ToArray();

        var tupleType = ValueTupleHelper.CreateTupleType(targetProperties.Select(p => p.ClrType.AsNullableType()).Append(typeof(string)));
        var tupleParameter = Expression.Parameter(tupleType);

        var selectExpressions = new List<SelectExpression>();

        foreach (var concreteType in concreteTypes)
        {
            var tableMappings
                = IterateTableMappings(concreteType, includeDerived: false)
                    .GroupBy(m => m.Table, (t, m) => m.First())
                    .ToArray();

            var tables = tableMappings.Select(m => m.Table).Distinct().ToArray();

            var principalTable
                = new BaseTableExpression(
                    tableMappings[0].Table.Schema,
                    tableMappings[0].Table.Name,
                    tableMappings[0].Table.Name[..1].ToLower(),
                    tableMappings[0].TypeBase.ClrType);

            var tableLookup = new Dictionary<ITableBase, AliasedTableExpression> { [tableMappings[0].Table] = principalTable };

            TableExpression queryTable = principalTable;

            foreach (var tableMapping in tableMappings.Skip(1))
            {
                var nonPrincipalTable
                    = new BaseTableExpression(
                        tableMapping.Table.Schema,
                        tableMapping.Table.Name,
                        tableMapping.Table.Name[..1].ToLower(),
                        tableMapping.TypeBase.ClrType);

                tableLookup[tableMapping.Table] = nonPrincipalTable;

                var predicate =
                    tableMappings[0].Table.PrimaryKey.Columns
                        .Zip(tableMapping.Table.PrimaryKey.Columns)
                        .Select(t => Expression.Equal(
                            new SqlColumnExpression(principalTable, t.First.Name, t.First.ProviderClrType, false, null),
                            new SqlColumnExpression(nonPrincipalTable, t.Second.Name, t.Second.ProviderClrType, false, null)))
                        .Aggregate(Expression.AndAlso);

                queryTable = new LeftJoinTableExpression(queryTable, nonPrincipalTable, predicate, targetType.ClrType);
            }

            var concreteProperties = IterateProperties(concreteType, includeDerived: false).Distinct().ToArray();

            var columnExpressions = new List<Expression>();

            foreach (var (p1, p2) in from p1 in targetProperties
                                     join p2 in concreteProperties on p1 equals p2 into lp2
                                     from p2 in lp2.DefaultIfEmpty()
                                     select (p1, p2))
            {
                if (p2 is null)
                {
                    columnExpressions.Add(Expression.Constant(null, p1.ClrType.AsNullableType()));
                }
                else
                {
                    var columnMapping = p2.GetTableColumnMappings().First(m => tableLookup.ContainsKey(m.TableMapping.Table));

                    columnExpressions.Add(
                        Expression.Convert(
                            MakeColumnExpression(tableLookup[columnMapping.TableMapping.Table], columnMapping.Column.Name, p2),
                            p2.ClrType.AsNullableType()));
                }
            }

            selectExpressions.Add(
                new SelectExpression(
                    new ServerProjectionExpression(
                        ValueTupleHelper.CreateNewExpression(
                            tupleType,
                            columnExpressions.Append(Expression.Constant(concreteType.GetDiscriminatorValue())))),
                    queryTable));
        }

        // TODO: rewrite UnionTableExpression and UnionAllTableExpression to accept more than two SelectExpressions

        var unionAllExpression = new UnionAllTableExpression(selectExpressions[0], selectExpressions[1]);
        var selectExpression
            = new SelectExpression(
                new ServerProjectionExpression(
                    new ProjectionReferenceRewritingExpressionVisitor(unionAllExpression)
                        .Visit(selectExpressions[0].Projection.Flatten().Body)),
                unionAllExpression);

        for (var i = 2; i < selectExpressions.Count; i++)
        {
            unionAllExpression = new UnionAllTableExpression(selectExpression, selectExpressions[i]);
            selectExpression
                = new SelectExpression(
                    new ServerProjectionExpression(
                        new ProjectionReferenceRewritingExpressionVisitor(unionAllExpression)
                            .Visit(selectExpression.Projection.Flatten().Body)),
                    unionAllExpression);
        }

        var propertyExpressions
            = (from x in targetProperties.Select((p, i) => (p, i))
               let p = x.p
               let expr = Expression.Convert(
                   ValueTupleHelper.CreateMemberExpression(
                       tupleType,
                       tupleParameter,
                       x.i),
                   p.ClrType)
               select (p, expr)).ToDictionary(x => x.p, x => (Expression)x.expr);

        var descriptors = new PolymorphicTypeDescriptor[concreteTypes.Length];

        for (var i = 0; i < concreteTypes.Length; i++)
        {
            var concreteType = concreteTypes[i];

            var test
                = Expression.Lambda(
                    Expression.Equal(
                        ValueTupleHelper.CreateMemberExpression(tupleType, tupleParameter, targetProperties.Length),
                        Expression.Constant(concreteType.GetDiscriminatorValue())),
                    tupleParameter);

            var descriptorMaterializer
                = Expression.Lambda(
                    CreateEntityMaterializationExpression(concreteType, null, propertyExpressions),
                    tupleParameter);

            descriptors[i] = new PolymorphicTypeDescriptor(concreteType.ClrType, test, descriptorMaterializer);
        }

        var materializer
            = new PolymorphicExpression(
                targetType.ClrType,
                selectExpression.Projection.Flatten().Body,
                descriptors)
                .Filter(targetType.ClrType);

        var projection = new ServerProjectionExpression(materializer);

        return new EnumerableRelationalQueryExpression(new SelectExpression(projection, unionAllExpression));
    }

    private EnumerableRelationalQueryExpression CreateTPTQueryExpression(IEntityType targetType)
    {
        ThrowForUnsupportedMappings(targetType);

        // Construct an inner join for the target type without any derived types 

        var targetTableMappings
            = IterateTableMappings(targetType, includeDerived: false)
                .GroupBy(m => m.Table, (t, m) => m.First())
                .ToArray();

        var tableLookup = new Dictionary<ITableBase, AliasedTableExpression>();

        var outerTable = targetTableMappings[0].Table;

        var outerTableExpression
            = new BaseTableExpression(
                outerTable.Schema,
                outerTable.Name,
                outerTable.Name[..1].ToLower(),
                targetType.ClrType);

        tableLookup[outerTable] = outerTableExpression;

        TableExpression queryTable = outerTableExpression;

        foreach (var innerMapping in targetTableMappings.Skip(1))
        {
            var innerTableExpression
                = new BaseTableExpression(
                    innerMapping.Table.Schema,
                    innerMapping.Table.Name,
                    innerMapping.Table.Name[..1].ToLower(),
                    innerMapping.TypeBase.ClrType);

            tableLookup[innerMapping.Table] = innerTableExpression;

            var predicate =
                outerTable.PrimaryKey.Columns
                    .Zip(innerMapping.Table.PrimaryKey.Columns)
                    .Select(t => Expression.Equal(
                        new SqlColumnExpression(outerTableExpression, t.First.Name, t.First.ProviderClrType, false, null),
                        new SqlColumnExpression(innerTableExpression, t.Second.Name, t.Second.ProviderClrType, false, null)))
                    .Aggregate(Expression.AndAlso);

            queryTable = new InnerJoinTableExpression(queryTable, innerTableExpression, predicate, targetType.ClrType);

            outerTable = innerMapping.Table;
            outerTableExpression = innerTableExpression;
        }

        // Construct left joins for concrete derived types of the target type

        var derivedTypes = IterateDerivedTypes(targetType).ToArray();

        foreach (var derivedType in derivedTypes)
        {
            var derivedOuterTable = outerTable;
            var derivedOuterTableExpression = outerTableExpression;

            var tableMappings
                = IterateTableMappings(derivedType, includeDerived: false)
                    .GroupBy(m => m.Table, (t, m) => m.First())
                    .ToArray();

            foreach (var innerMapping in tableMappings)
            {
                if (tableLookup.ContainsKey(innerMapping.Table))
                {
                    continue;
                }

                var innerTableExpression
                    = new BaseTableExpression(
                        innerMapping.Table.Schema,
                        innerMapping.Table.Name,
                        innerMapping.Table.Name[..1].ToLower(),
                        innerMapping.TypeBase.ClrType);

                tableLookup[innerMapping.Table] = innerTableExpression;

                var predicate =
                    derivedOuterTable.PrimaryKey.Columns
                        .Zip(innerMapping.Table.PrimaryKey.Columns)
                        .Select(t => Expression.Equal(
                            new SqlColumnExpression(derivedOuterTableExpression, t.First.Name, t.First.ProviderClrType, false, null),
                            new SqlColumnExpression(innerTableExpression, t.Second.Name, t.Second.ProviderClrType, false, null)))
                        .Aggregate(Expression.AndAlso);

                queryTable = new LeftJoinTableExpression(queryTable, innerTableExpression, predicate, targetType.ClrType);

                derivedOuterTable = innerMapping.Table;
                derivedOuterTableExpression = innerTableExpression;
            }
        }

        // Construct the projection / materializer

        var hierarchy = targetType.GetDerivedTypesInclusive().ToArray();

        var propertyMappings
            = (from p in IterateProperties(targetType, includeDerived: true).Distinct()
               from m in p.GetTableColumnMappings().OrderBy(m => Array.IndexOf(hierarchy, m.TableMapping.TypeBase))
               where tableLookup.ContainsKey(m.Column.Table)
               select (Property: p, Mapping: m)).Distinct().ToArray();

        var columnExpressions
            = (from p in propertyMappings
               group p.Property by p.Mapping.Column into properties
               let property = properties.First()
               let column = properties.Key
               let table = tableLookup[column.Table]
               let expression = new SqlColumnExpression(table, column.Name, property.ClrType.AsNullableType(), true, GetColumnTypeMapping(property))
               select (expression, properties)).ToArray();

        var tupleType = ValueTupleHelper.CreateTupleType(columnExpressions.Select(c => c.expression.Type.AsNullableType()).Append(typeof(string)));
        var tupleParameter = Expression.Parameter(tupleType);

        var propertyExpressions
            = (from p in propertyMappings
               group p by p.Property into g
               let p = g.Key
               let expr = Expression.Convert(
                   ValueTupleHelper.CreateMemberExpression(
                       tupleType,
                       tupleParameter,
                       Array.FindIndex(columnExpressions, c => c.properties.Contains(p))),
                   p.ClrType)
               select (p, expr)).ToDictionary(x => x.p, x => (Expression)x.expr);

        var concreteTypes = hierarchy.Where(t => !t.IsAbstract()).ToArray();
        var descriptors = new PolymorphicTypeDescriptor[concreteTypes.Length];

        for (var i = 0; i < concreteTypes.Length; i++)
        {
            var concreteType = concreteTypes[i];

            var test
                = Expression.Lambda(
                    Expression.Equal(
                        ValueTupleHelper.CreateMemberExpression(
                            tupleType,
                            tupleParameter,
                            columnExpressions.Length),
                        Expression.Constant(concreteType.GetDiscriminatorValue())),
                    tupleParameter);

            var descriptorMaterializer
                = Expression.Lambda(
                    CreateEntityMaterializationExpression(concreteType, tableLookup, propertyExpressions),
                    tupleParameter);

            descriptors[i] = new PolymorphicTypeDescriptor(concreteType.ClrType, test, descriptorMaterializer);
        }

        var discriminatorExpression
            = new SqlCaseExpression(
                from t in concreteTypes.Reverse()
                from m in t.GetTableMappings().TakeLast(1)
                let c = m.Table.PrimaryKey.Columns[0]
                select Expression.NotEqual(
                    new SqlColumnExpression(
                        tableLookup[m.Table],
                        c.Name,
                        c.ProviderClrType.AsNullableType(),
                        true,
                        null),
                    Expression.Constant(null, c.ProviderClrType.AsNullableType())),
                from t in concreteTypes.Reverse()
                select Expression.Constant(t.GetDiscriminatorValue()),
                null,
                typeof(string));

        var materializer = new PolymorphicExpression(
            targetType.ClrType,
            ValueTupleHelper.CreateNewExpression(tupleType, columnExpressions.Select(c => c.expression).Cast<Expression>().Append(discriminatorExpression)),
            descriptors).Filter(targetType.ClrType);

        var projection = new ServerProjectionExpression(materializer);

        var selectExpression = new SelectExpression(projection, queryTable);

        return new EnumerableRelationalQueryExpression(selectExpression);
    }

    private static ExtendedNewExpression CreateNewExpression(ITypeBase type, Dictionary<IProperty, Expression> propertyExpressions)
    {
        var instantiationBinding = type.ConstructorBinding;

        switch (instantiationBinding)
        {
            case ConstructorBinding constructorBinding:
            {
                return CreateNewExpression(type, constructorBinding, propertyExpressions);
            }

            case FactoryMethodBinding factoryMethodBinding:
            {
                return CreateNewExpression(type, factoryMethodBinding, propertyExpressions);
            }

            case DefaultValueBinding defaultValueBinding:
            {
                Debug.Assert(defaultValueBinding.ParameterBindings.Count == 0);

                return new ExtendedNewExpression(type.ClrType);
            }

            case null:
            {
                return new ExtendedNewExpression(type.ClrType);
            }

            default:
            {
                throw new NotSupportedException($"The {instantiationBinding.GetType().Name} is not supported.");
            }
        }
    }

    private static ExtendedNewExpression CreateNewExpression(ITypeBase type, ConstructorBinding constructorBinding, Dictionary<IProperty, Expression> propertyExpressions)
    {
        var constructor = constructorBinding.Constructor;
        var arguments = new Expression[constructor.GetParameters().Length];
        var readableMembers = new MemberInfo[arguments.Length];
        var writableMembers = new MemberInfo[arguments.Length];

        for (var i = 0; i < arguments.Length; i++)
        {
            var binding = constructorBinding.ParameterBindings[i];

            arguments[i] = GetBindingExpression(type, binding, propertyExpressions);

            if (binding.ConsumedProperties.ElementAtOrDefault(0) is IPropertyBase property)
            {
                readableMembers[i] = property.GetSemanticReadableMemberInfo();
                writableMembers[i] = property.GetWritableMemberInfo();
            }
        }

        return new ExtendedNewExpression(type.ClrType, constructor, arguments, readableMembers, writableMembers);
    }

    private static EFCoreProxyNewExpression CreateNewExpression(ITypeBase type, FactoryMethodBinding factoryMethodBinding, Dictionary<IProperty, Expression> propertyExpressions)
    {
        var factoryInstance
            = typeof(FactoryMethodBinding)
                .GetField("_factoryInstance", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(factoryMethodBinding);

        var factoryMethod
            = typeof(FactoryMethodBinding)
                .GetField("_factoryMethod", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(factoryMethodBinding) as MethodInfo;

        if (factoryInstance is null && (factoryMethod is null || !factoryMethod.IsStatic))
        {
            throw new NotSupportedException();
        }

        var bindings = factoryMethodBinding.ParameterBindings;

        if (!(bindings.Count == 3
            && bindings[0] is EntityTypeParameterBinding entityTypeParameterBinding
            && bindings[1] is ServiceParameterBinding serviceParameterBinding
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
            = factoryMethodBinding.RuntimeType
                .GetConstructor(innerBindings.Select(b => b.ParameterType).ToArray());

        var arguments = new Expression[innerBindings.Count];
        var readableMembers = new MemberInfo[arguments.Length];
        var writableMembers = new MemberInfo[arguments.Length];

        for (var i = 0; i < arguments.Length; i++)
        {
            var binding = innerBindings[i];

            arguments[i] = GetBindingExpression(type, binding, propertyExpressions);

            if (binding.ConsumedProperties.ElementAtOrDefault(0) is IPropertyBase property)
            {
                readableMembers[i] = property.GetSemanticReadableMemberInfo();
                writableMembers[i] = property.GetWritableMemberInfo();
            }
        }

        return new EFCoreProxyNewExpression(
            factoryInstance is null ? null : Expression.Constant(factoryInstance),
            factoryMethod,
            [
                GetBindingExpression(type, entityTypeParameterBinding, propertyExpressions),
                GetBindingExpression(type, serviceParameterBinding, propertyExpressions)
            ],
            constructor,
            arguments,
            readableMembers,
            writableMembers);
    }

    private static Expression CreateEntityMaterializationExpression(
        IEntityType entityType,
        Dictionary<ITableBase, AliasedTableExpression> tableLookup,
        Dictionary<IProperty, Expression> propertyExpressions,
        bool isOptional = false)
    {
        var properties
            = (from p in entityType.GetProperties()
               where !p.IsShadowProperty()
               where !p.IsIndexerProperty() // TODO: include indexer properties
               select p).ToList();

        var complexProperties
            = (from p in entityType.GetComplexProperties()
               select p).ToList();

        var navigations
            = (from n in entityType.GetNavigations()
               where n.ForeignKey.IsOwnership && TablesMatch(entityType, n.TargetEntityType)
               where !n.IsOnDependent && (!n.IsCollection || n.TargetEntityType.IsMappedToJson())
               select n).ToList();

        var services
            = (from s in entityType.GetServiceProperties()
               select s).ToList();

        var newExpression = CreateNewExpression(entityType, propertyExpressions);

        if (newExpression.WritableMembers is not null)
        {
            properties.RemoveAll(p => newExpression.WritableMembers.Contains(p.GetWritableMemberInfo()));
            navigations.RemoveAll(p => newExpression.WritableMembers.Contains(p.GetWritableMemberInfo()));
            services.RemoveAll(p => newExpression.WritableMembers.Contains(p.GetWritableMemberInfo()));
        }

        var arguments = new Expression[properties.Count + complexProperties.Count + navigations.Count + services.Count];
        var readableMembers = new MemberInfo[arguments.Length];
        var writableMembers = new MemberInfo[arguments.Length];

        var c = properties.Count;
        var d = 0;

        for (var i = 0; i < c; i++)
        {
            var property = properties[i - d];

            arguments[i] = propertyExpressions[property];
            readableMembers[i] = property.GetSemanticReadableMemberInfo();
            writableMembers[i] = property.GetWritableMemberInfo();
        }

        c += complexProperties.Count;
        d += properties.Count;

        for (var i = d; i < c; i++)
        {
            var complexProperty = complexProperties[i - d];

            arguments[i] = CreateComplexMaterializationExpression(complexProperty.ComplexType, tableLookup, propertyExpressions);
            readableMembers[i] = complexProperty.GetSemanticReadableMemberInfo();
            writableMembers[i] = complexProperty.GetWritableMemberInfo();
        }

        c += navigations.Count;
        d += complexProperties.Count;

        for (var i = d; i < c; i++)
        {
            var navigation = navigations[i - d];

            Debug.Assert(navigation.ForeignKey.IsOwnership);

            var ownedType = navigation.TargetEntityType;

            if (ownedType.IsMappedToJson())
            {
                // TODO: properly handle nullability, type mapping, possibly property path names need work?
                arguments[i] = new SqlColumnExpression(
                    tableLookup[entityType.GetTableMappings().Single().Table],
                    ownedType.GetContainerColumnName(),
                    navigation.ClrType,
                    true,
                    null);
            }
            else
            {
                arguments[i]
                    = CreateEntityMaterializationExpression(
                        ownedType,
                        tableLookup,
                        propertyExpressions,
                        !navigation.ForeignKey.IsRequiredDependent);
            }

            readableMembers[i] = navigation.GetSemanticReadableMemberInfo();
            writableMembers[i] = navigation.GetWritableMemberInfo();
        }

        c += services.Count;
        d += navigations.Count;

        for (var i = d; i < c; i++)
        {
            var service = services[i - d];

            arguments[i] = GetBindingExpression(entityType, service.ParameterBinding, propertyExpressions);
            readableMembers[i] = service.GetSemanticReadableMemberInfo();
            writableMembers[i] = service.GetWritableMemberInfo();
        }

        Expression materializer
            = new ExtendedMemberInitExpression(
                entityType.ClrType,
                newExpression,
                arguments,
                readableMembers,
                writableMembers);

        var keySelector
            = CreateMaterializationKeySelector(entityType);

        if (keySelector is not null)
        {
            var shadowProperties
                = from p in entityType.GetProperties()
                  where p.IsShadowProperty()
                  select p;

            materializer
                = new EntityMaterializationExpression(
                    entityType,
                    QueryTrackingBehavior.NoTracking,
                    keySelector,
                    shadowProperties,
                    shadowProperties.Select(s => propertyExpressions[s]),
                    materializer);
        }

        if (isOptional)
        {
            var primaryKey = entityType.FindPrimaryKey();

            if (primaryKey is not null)
            {
                var sentinelProperty = primaryKey.Properties[0];

                var sameTabledPrincipalType = FindSameTabledPrincipalType(entityType);

                if (sameTabledPrincipalType is not null && sameTabledPrincipalType != entityType)
                {
                    sentinelProperty = entityType.GetProperties().Except(primaryKey.Properties).First();
                }

                var sentinelExpression = propertyExpressions[sentinelProperty].UnwrapInnerExpression();

                materializer = new DefaultIfEmptyExpression(materializer, sentinelExpression.AsNullable());
            }
        }

        return materializer;
    }

    private static Expression CreateComplexMaterializationExpression(
        IComplexType complexType,
        Dictionary<ITableBase, AliasedTableExpression> tableLookup,
        Dictionary<IProperty, Expression> propertyExpressions)
    {
        var properties
            = (from p in complexType.GetProperties()
               where !p.IsShadowProperty()
               where !p.IsIndexerProperty() // TODO: include indexer properties
               select p).ToList();

        var complexProperties
            = (from p in complexType.GetComplexProperties()
               select p).ToList();

        var newExpression = CreateNewExpression(complexType, propertyExpressions);

        if (newExpression.WritableMembers is not null)
        {
            properties.RemoveAll(p => newExpression.WritableMembers.Contains(p.GetWritableMemberInfo()));
        }

        var arguments = new Expression[properties.Count + complexProperties.Count];
        var readableMembers = new MemberInfo[arguments.Length];
        var writableMembers = new MemberInfo[arguments.Length];

        var c = properties.Count;
        var d = 0;

        for (var i = 0; i < c; i++)
        {
            var property = properties[i - d];

            arguments[i] = propertyExpressions[property];
            readableMembers[i] = property.GetSemanticReadableMemberInfo();
            writableMembers[i] = property.GetWritableMemberInfo();
        }

        c += complexProperties.Count;
        d += properties.Count;

        for (var i = d; i < c; i++)
        {
            var complexProperty = complexProperties[i - d];

            arguments[i] = CreateComplexMaterializationExpression(complexProperty.ComplexType, tableLookup, propertyExpressions);
            readableMembers[i] = complexProperty.GetSemanticReadableMemberInfo();
            writableMembers[i] = complexProperty.GetWritableMemberInfo();
        }

        Expression materializer
            = new ExtendedMemberInitExpression(
                complexType.ClrType,
                newExpression,
                arguments,
                readableMembers,
                writableMembers);

        return materializer;
    }

    private static Expression GetBindingExpression(ITypeBase type, ParameterBinding binding, Dictionary<IProperty, Expression> propertyExpressions)
    {
        switch (binding)
        {
            case DependencyInjectionMethodParameterBinding service:
            {
                return new ContextServiceDelegateInjectionExpression(
                    service.ParameterType,
                    service.ServiceType,
                    service.Method);
            }

            case DependencyInjectionParameterBinding service:
            {
                return new ContextServiceInjectionExpression(service.ParameterType);
            }

            case PropertyParameterBinding _
            when binding.ConsumedProperties[0] is IProperty property:
            {
                return propertyExpressions[property];
            }

            case EntityTypeParameterBinding:
            {
                return new EntityTypeInjectionExpression((IEntityType)type);
            }

            case ContextParameterBinding:
            {
                return new ContextServiceInjectionExpression(binding.ParameterType);
            }

            default:
            {
                throw new NotSupportedException($"The {binding.GetType().Name} is not supported.");
            }
        }
    }

    private static IEnumerable<ITableMapping> IterateTableMappings(IEntityType type, bool includeDerived)
    {
        foreach (var mapping in type.GetTableMappings()
                                    .OrderByDescending(m => m.IsSharedTablePrincipal ?? true)
                                    .ThenByDescending(m => m.IsSplitEntityTypePrincipal ?? true))
        {
            yield return mapping;
        }

        if (includeDerived)
        {
            foreach (var derived in type.GetDirectlyDerivedTypes())
            {
                foreach (var mapping in IterateTableMappings(derived, true))
                {
                    yield return mapping;
                }
            }
        }

        foreach (var owned in type.GetNavigations().Where(n => n.ForeignKey.IsOwnership))
        {
            if (!owned.IsOnDependent && !owned.IsCollection && !owned.TargetEntityType.IsMappedToJson())
            {
                foreach (var mapping in IterateTableMappings(owned.TargetEntityType, true))
                {
                    yield return mapping;
                }
            }
        }
    }

    private static IEnumerable<IProperty> IterateProperties(IEntityType type, bool includeDerived)
    {
        foreach (var property in type.GetFlattenedProperties())
        {
            yield return property;
        }

        if (includeDerived)
        {
            foreach (var derived in type.GetDirectlyDerivedTypes())
            {
                foreach (var property in IterateProperties(derived, true))
                {
                    yield return property;
                }
            }
        }

        foreach (var owned in type.GetNavigations().Where(n => n.ForeignKey.IsOwnership))
        {
            if (!TablesMatch(type, owned.TargetEntityType))
            {
                continue;
            }

            if (!owned.IsOnDependent && !owned.IsCollection && !owned.TargetEntityType.IsMappedToJson())
            {
                foreach (var property in IterateProperties(owned.TargetEntityType, true))
                {
                    yield return property;
                }
            }
        }
    }

    /// <summary>
    /// Recursively enumerates through the derived types of the given type.
    /// Sometimes IEntityType.GetDerivedTypesInclusive will return types in a "bad" order,
    /// e.g. [Eagle,Bird,Kiwi] when it "should" be [Bird,Eagle,Kiwi]
    /// </summary>
    private static IEnumerable<IEntityType> IterateDerivedTypes(IEntityType type)
    {
        foreach (var derived in type.GetDirectlyDerivedTypes())
        {
            yield return derived;

            foreach (var derived2 in IterateDerivedTypes(derived))
            {
                yield return derived2;
            }
        }
    }

    private static bool TablesMatch(IEntityType type, IEntityType ownedType)
    {
        var tables1 = IterateTableMappings(type, false).Select(m => m.Table);
        var tables2 = IterateTableMappings(ownedType, false).Select(m => m.Table);

        return !tables2.Except(tables1).Any();
    }

    private SqlColumnExpression MakeColumnExpression(AliasedTableExpression table, string columnName, IProperty property)
    {
        var typeMapping = GetColumnTypeMapping(property);

        var nullable = GetColumnNullability(property);

        return new SqlColumnExpression(
            table,
            columnName,
            property.ClrType,
            nullable,
            typeMapping);
    }

    private ITypeMapping GetColumnTypeMapping(IProperty property)
    {
        ITypeMapping typeMapping = default;

        var sourceMapping = relationalTypeMappingSource.FindMapping(property);

        if (sourceMapping is not null)
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

        return typeMapping;
    }

    private static bool GetColumnNullability(IProperty property)
    {
        // If the property is nullable, it can be null, who would've thought

        if (property.IsNullable)
        {
            return true;
        }

        if (property.DeclaringType is IEntityType declaringEntityType)
        {
            // If the property is declared by a derived type or a type owned by a derived type, it can be null

            if (IsDerivedTypeOrOwnedByDerivedType(declaringEntityType))
            {
                return true;
            }

            // If the property is declared by an owned type of a root type and not declared nullable, it can't be null

            if (declaringEntityType.IsOwned())
            {
                return false;
            }

            // If the property is part of a foreign key within the same table as the principal type,
            // but the principal type is derived, it can be null

            var tableId = GetRelationalId(declaringEntityType);

            foreach (var foreignKey in declaringEntityType.GetForeignKeys().Where(fk => fk.Properties.Contains(property)))
            {
                var principalType = foreignKey.PrincipalEntityType;

                if (tableId.Equals(GetRelationalId(principalType))
                    && principalType != principalType.GetRootType())
                {
                    return true;
                }
            }
        }
        else if (property.DeclaringType is IComplexType declaringComplexType)
        {
            if (IsDerivedTypeOrOwnedByDerivedType(declaringComplexType.ContainingEntityType))
            {
                return true;
            }

            return declaringComplexType.ComplexProperty.IsNullable;
        }

        return false;
    }

    private static bool IsDerivedTypeOrOwnedByDerivedType(IEntityType entityType)
    {
        var resolvedEntityType = entityType;

        while (resolvedEntityType.IsOwned())
        {
            var ownership = resolvedEntityType.FindOwnership();

            if (!ownership.IsRequiredDependent)
            {
                return true;
            }

            resolvedEntityType = ownership.PrincipalToDependent.DeclaringEntityType;
        }

        if (resolvedEntityType != resolvedEntityType.GetRootType())
        {
            return true;
        }

        return false;
    }

    private static IEntityType FindSameTabledPrincipalType(IEntityType dependentType)
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
                && principalType != principalType.GetRootType()
                && principalType.GetRootType() != dependentType.GetRootType())
            {
                return principalType;
            }
        }

        return null;
    }

    private static (string, string) GetRelationalId(IEntityType entityType)
    {
        // TODO: why would there be more than one?
        // TODO: why would there be zero?
        var mapping = entityType.GetTableMappings().FirstOrDefault(m => m.IncludesDerivedTypes);

        if (mapping is not null)
        {
            return (mapping.Table.Schema, mapping.Table.Name);
        }

        return (entityType.GetSchema(), entityType.GetTableName());
    }

    private static (string, string, string) GetRelationalId(IProperty property)
    {
        // TODO: why would there be more than one? and in some cases apparently identical?
        // TODO: why would there be zero?
        // see test: Collection_projection_on_base_type_split
        var mapping = property.GetTableColumnMappings().First();

        return (mapping.Column.Table.Schema, mapping.Column.Table.Name, mapping.Column.Name);
    }
}
