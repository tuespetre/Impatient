using Dapper;
using Impatient.Query;
using Impatient.Query.Expressions;
using Impatient.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using static System.Linq.Enumerable;
using static System.Math;

namespace Impatient.Tests
{
    [TestClass]
    public class TablePerTypeInheritanceTests
    {
        private readonly StringBuilder commandLog = new StringBuilder();
        private readonly ImpatientQueryProvider impatient;

        private string SqlLog => commandLog.ToString();

        private static readonly IDictionary<Type, Expression> QueryExpressions;

        private static Expression BaseTypeQueryExpression => QueryExpressions[typeof(BaseAbstractType1)];

        private IQueryable<TSource> Query<TSource>()
        {
            return impatient.CreateQuery<TSource>(QueryExpressions[typeof(TSource)]);
        }

        #region helper types and functions

        public enum ImpatientInheritanceMode
        {
            Default,
            TablePerType,
        }

        public struct ImpatientTableDescriptor
        {
            public Type SourceType;
            public ImpatientInheritanceMode InheritanceMode;
            public string SchemaName;
            public string TableName;
            public IEnumerable<MemberInfo> PrimaryKeyMembers;
            public IEnumerable<ImpatientColumnDescriptor> ColumnDescriptors;
        }

        public struct ImpatientColumnDescriptor
        {
            public Type SourceType;
            public MemberInfo SourceMember;
            public string ColumnName;
            public bool IsNullable;

            public Type Type => GetMemberType(SourceMember);
        }

        public struct TreeNode<TValue>
        {
            private IEnumerable<TreeNode<TValue>> children;

            public TValue Value { get; }

            public IEnumerable<TreeNode<TValue>> Children => children ?? Empty<TreeNode<TValue>>();

            public TreeNode(TValue value, IEnumerable<TreeNode<TValue>> children)
            {
                Value = value;

                this.children = children ?? throw new ArgumentNullException(nameof(children));
            }

            public IEnumerable<TValue> Flatten()
            {
                yield return Value;

                foreach (var child in Children)
                {
                    foreach (var value in child.Flatten())
                    {
                        yield return value;
                    }
                }
            }

            public TreeNode<TResult> Transform<TResult>(Func<TreeNode<TValue>, TResult> selector)
            {
                return new TreeNode<TResult>(
                    value: selector(this),
                    children: Children.Select(c => c.Transform(selector)).ToArray());
            }

            public TAccumulate Aggregate<TAccumulate>(
                TAccumulate seed,
                Func<TAccumulate, TValue, TValue, TAccumulate> accumulator)
            {
                var result = seed;

                foreach (var child in Children)
                {
                    result = child.Aggregate(accumulator(result, Value, child.Value), accumulator);
                }

                return result;
            }
        }

        public static class TreeNode
        {
            public static IEnumerable<TreeNode<TValue>> Treeify<TValue>(IEnumerable<IEnumerable<TValue>> sequences)
            {
                return from s in sequences
                       where s.Any()
                       group s.Skip(1) by s.First() into sg
                       select new TreeNode<TValue>(
                           value: sg.Key,
                           children: Treeify(sg));
            }
        }

        private static bool IsNullableType(Type type)
        {
            return !type.GetTypeInfo().IsValueType;
        }

        private static Type MakeNullableType(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case PropertyInfo propertyInfo:
                {
                    return propertyInfo.PropertyType;
                }

                case FieldInfo fieldInfo:
                {
                    return fieldInfo.FieldType;
                }

                default:
                {
                    return typeof(void);
                }
            }
        }

        private static IEnumerable<Type> GetInheritancePath(Type from, Type to)
        {
            do
            {
                yield return from;

                from = from.GetTypeInfo().BaseType;
            }
            while (from != null && to.IsAssignableFrom(from));
        }

        private static IEnumerable<Expression> CreateQueryExpressions(
            IEnumerable<ImpatientTableDescriptor> tableDescriptors)
        {
            var polymorphicRoots = tableDescriptors.Where(t => t.InheritanceMode != ImpatientInheritanceMode.Default);

            var polymorphicRootTypes = polymorphicRoots.Select(t => t.SourceType).ToArray();

            var invalidHierarchy
                = polymorphicRootTypes.Any(t1 =>
                    polymorphicRootTypes.Any(t2 =>
                        t1 != t2 && t1.IsAssignableFrom(t2)));

            if (invalidHierarchy)
            {
                throw new ArgumentException("Invalid hierarchy model", nameof(tableDescriptors));
            }

            var polymorphicTableDescriptorGroups
                = from r in polymorphicRoots
                  from d in tableDescriptors
                  where r.SourceType.IsAssignableFrom(d.SourceType)
                  group d by r;

            foreach (var tableDescriptorGroup in polymorphicTableDescriptorGroups)
            {
                switch (tableDescriptorGroup.Key.InheritanceMode)
                {
                    case ImpatientInheritanceMode.TablePerType:
                    {
                        var queryExpressions
                            = CreateTablePerTypeQueryExpressions(
                                tableDescriptorGroup.Key.SourceType,
                                tableDescriptorGroup);

                        foreach (var queryExpression in queryExpressions)
                        {
                            yield return queryExpression;
                        }

                        break;
                    }
                }
            }
        }

        private struct TablePerTypeInfo
        {
            public Type Type;
            public IEnumerable<Type> Path;
            public ImpatientTableDescriptor TableDescriptor;
            public BaseTableExpression Table;
            public IEnumerable<SqlColumnExpression> Columns;
        }

        private static IEnumerable<Expression> CreateTablePerTypeQueryExpressions(
            Type rootType,
            IEnumerable<ImpatientTableDescriptor> tableDescriptors)
        {
            var inheritancePaths
                = (from t in tableDescriptors
                   select GetInheritancePath(t.SourceType, rootType) into p
                   where !p.First().GetTypeInfo().IsAbstract
                   orderby p.Count()
                   select p.Reverse()).ToArray();

            return from t in tableDescriptors
                   let p = (from p in inheritancePaths where p.Contains(t.SourceType) select p)
                   from r in TreeNode.Treeify(p)
                   select CreateTablePerTypeQueryExpression(t.SourceType, r.Transform(node =>
                   {
                       var descriptor = tableDescriptors.Single(d => d.SourceType == node.Value);

                       var table
                           = new BaseTableExpression(
                               descriptor.SchemaName,
                               descriptor.TableName,
                               descriptor.TableName.ToLower().Substring(0, 1),
                               descriptor.SourceType);

                       var columns
                           = from cd in descriptor.ColumnDescriptors
                             let nullable = cd.SourceType != rootType
                             let type = nullable ? MakeNullableType(cd.Type) : cd.Type
                             select new SqlColumnExpression(
                                 table,
                                 cd.ColumnName,
                                 type,
                                 nullable || cd.IsNullable);

                       return new TablePerTypeInfo
                       {
                           Type = node.Value,
                           Path = GetInheritancePath(node.Value, rootType),
                           TableDescriptor = descriptor,
                           Table = table,
                           Columns = columns.ToArray(),
                       };
                   }));
        }

        private static Expression CreateTablePerTypeQueryExpression(
            Type targetType,
            TreeNode<TablePerTypeInfo> hierarchyRoot)
        {
            var columnDescriptors
                = hierarchyRoot
                    .Flatten()
                    .SelectMany(x => x.TableDescriptor.ColumnDescriptors)
                    .Select((c, i) => new { c, i });

            var columnExpressions
                = hierarchyRoot
                    .Flatten()
                    .SelectMany(x => x.Columns);

            var tupleType = ValueTupleHelper.CreateTupleType(columnExpressions.Select(c => c.Type));
            var tupleNewExpression = ValueTupleHelper.CreateNewExpression(tupleType, columnExpressions);
            var tupleParameter = Expression.Parameter(tupleType);

            // TODO: Use ExpandParameters instead
            // Use ExpandParameters on a LambdaExpression representing
            // the materializer for the concrete type. Expand it the
            // LambdaExpression so that all of the bindings are bound
            // to the tuple's field accessors.

            var polymorphicTypeDescriptors
                = (from node in hierarchyRoot.Flatten()
                   where !node.Type.GetTypeInfo().IsAbstract
                   let testMember = node.TableDescriptor.PrimaryKeyMembers.First()
                   select new PolymorphicExpression.TypeDescriptor(
                       node.Type,
                       Expression.Lambda(
                         Expression.NotEqual(
                             ValueTupleHelper.CreateMemberExpression(
                                 tupleType,
                                 tupleParameter,
                                 (from x in columnDescriptors
                                  where x.c.SourceMember == testMember
                                  select x.i).Single()),
                             Expression.Constant(null, MakeNullableType(GetMemberType(testMember)))),
                         tupleParameter),
                       Expression.Lambda(
                         Expression.MemberInit(
                             Expression.New(node.Type),
                             (from x in columnDescriptors
                              where node.Path.Contains(x.c.SourceType)
                              group x by x.c.SourceMember.MetadataToken into xg
                              select xg.Last() into x
                              select Expression.Bind(
                                 x.c.SourceMember,
                                 Expression.Convert(
                                     ValueTupleHelper.CreateMemberExpression(tupleType, tupleParameter, x.i),
                                     GetMemberType(x.c.SourceMember))))),
                         tupleParameter))).ToArray();

            // TODO: Use ExpandParameters instead
            // Use ExpandParameters on a LambdaExpression representing
            // the primary key accessor for the root type. Expand it
            // once for the parent table and once for the child table
            // and use an Equal expression to compare the two.

            var tableExpression
                = hierarchyRoot
                    .Aggregate<TableExpression>(
                        hierarchyRoot.Value.Table,
                        (accumulate, parent, child) =>
                        {
                            var keyComparisons
                                = from k in hierarchyRoot.Value.TableDescriptor.PrimaryKeyMembers
                                  join l in parent.TableDescriptor.ColumnDescriptors
                                  on k.MetadataToken equals l.SourceMember.MetadataToken
                                  join r in child.TableDescriptor.ColumnDescriptors
                                  on k.MetadataToken equals r.SourceMember.MetadataToken
                                  select Expression.Equal(
                                      new SqlColumnExpression(parent.Table, l.ColumnName, l.Type, l.IsNullable),
                                      new SqlColumnExpression(child.Table, r.ColumnName, r.Type, r.IsNullable));

                            return new LeftJoinExpression(
                                accumulate,
                                child.Table,
                                keyComparisons.Aggregate(Expression.AndAlso),
                                hierarchyRoot.Value.Type);
                        });

            return new EnumerableRelationalQueryExpression(
                new SelectExpression(
                    new ServerProjectionExpression(
                        new PolymorphicExpression(
                            targetType,
                            tupleNewExpression,
                            polymorphicTypeDescriptors)),
                    tableExpression));
        }

        #endregion

        static TablePerTypeInheritanceTests()
        {
            var allTypes = new[]
            {
                typeof(BaseAbstractType1),
                typeof(DerivedConcreteType1),
                typeof(DerivedAbstractType1),
                typeof(DerivedConcreteType2),
                typeof(DerivedConcreteType3),
            };

            var tableDescriptors
                = (from type in allTypes
                   let keyProperties = (from n in new[] { "Id" }
                                        join p in type.GetRuntimeProperties() on n equals p.Name
                                        select p).ToArray()
                   select new ImpatientTableDescriptor
                   {
                       SourceType = type,
                       InheritanceMode = type == typeof(BaseAbstractType1) ? ImpatientInheritanceMode.TablePerType : ImpatientInheritanceMode.Default,
                       SchemaName = "dbo",
                       TableName = type.Name,
                       PrimaryKeyMembers = keyProperties,
                       ColumnDescriptors = (from member in keyProperties.Union(type.GetTypeInfo().DeclaredProperties)
                                            select new ImpatientColumnDescriptor
                                            {
                                                SourceType = type,
                                                SourceMember = member,
                                                ColumnName = member.Name,
                                                IsNullable = IsNullableType(GetMemberType(member))
                                            }).ToArray()
                   }).ToArray();

            QueryExpressions = CreateQueryExpressions(tableDescriptors).ToDictionary(e => e.Type.GenericTypeArguments[0]);
        }

        public TablePerTypeInheritanceTests()
        {
            impatient = new ImpatientQueryProvider(
                new TestImpatientConnectionFactory(@"Server=.\sqlexpress; Database=tempdb; Trusted_Connection=true"),
                new DefaultImpatientQueryCache(),
                new DefaultImpatientExpressionVisitorProvider())
            {
                DbCommandInterceptor = command =>
                {
                    if (commandLog.Length > 0)
                    {
                        commandLog.AppendLine().AppendLine();
                    }

                    commandLog.Append(command.CommandText);
                }
            };

            using (var connection = impatient.ConnectionFactory.CreateConnection())
            {
                connection.Execute(@"
DROP TABLE IF EXISTS DerivedConcreteType3;
DROP TABLE IF EXISTS DerivedConcreteType2;
DROP TABLE IF EXISTS DerivedConcreteType1;
DROP TABLE IF EXISTS DerivedAbstractType1;
DROP TABLE IF EXISTS BaseAbstractType1;

CREATE TABLE BaseAbstractType1
(
    [Id] int not null primary key,
    [Property_BaseAbstractType1] nvarchar(max) null
);

CREATE TABLE DerivedAbstractType1
(
    [Id] int not null primary key,
    [Property_DerivedAbstractType1] nvarchar(max) null,
    CONSTRAINT FK_DerivedAbstractType1_BaseAbstractType1
        FOREIGN KEY (Id) REFERENCES BaseAbstractType1 (Id)
);

CREATE TABLE DerivedConcreteType1
(
    [Id] int not null primary key,
    [Property_DerivedConcreteType1] nvarchar(max) null,
    CONSTRAINT FK_DerivedConcreteType1_BaseAbstractType1
        FOREIGN KEY (Id) REFERENCES BaseAbstractType1 (Id)
);

CREATE TABLE DerivedConcreteType2
(
    [Id] int not null primary key,
    [Property_DerivedConcreteType2] nvarchar(max) null,
    CONSTRAINT FK_DerivedConcreteType2_DerivedAbstractType1
        FOREIGN KEY (Id) REFERENCES DerivedAbstractType1 (Id)
);

CREATE TABLE DerivedConcreteType3
(
    [Id] int not null primary key,
    [Property_DerivedConcreteType3] nvarchar(max) null,
    CONSTRAINT FK_DerivedConcreteType3_DerivedAbstractType1
        FOREIGN KEY (Id) REFERENCES DerivedAbstractType1 (Id)
);

INSERT INTO BaseAbstractType1 (Id, Property_BaseAbstractType1)
VALUES
(1, 'a'),
(2, 'b'),
(3, 'c'),
(4, 'a'),
(5, 'b'),
(6, 'c'),
(7, 'a'),
(8, 'b'),
(9, 'c');

INSERT INTO DerivedAbstractType1 (Id, Property_DerivedAbstractType1)
VALUES
(4, 'a'),
(5, 'b'),
(6, 'c'),
(7, 'a'),
(8, 'b'),
(9, 'c');

INSERT INTO DerivedConcreteType1(Id, Property_DerivedConcreteType1)
VALUES
(1, 'a'),
(2, 'b'),
(3, 'c');

INSERT INTO DerivedConcreteType2(Id, Property_DerivedConcreteType2)
VALUES
(4, 'a'),
(5, 'b'),
(6, 'c');

INSERT INTO DerivedConcreteType3(Id, Property_DerivedConcreteType3)
VALUES
(7, 'a'),
(8, 'b'),
(9, 'c');
");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            commandLog.Clear();
        }

        [TestMethod]
        public void Select_simple()
        {
            var query = from b in impatient.CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression)
                        select b;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [b].[Id] AS [Item1], [b].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d_0].[Id] AS [Item5], [d_0].[Property_DerivedAbstractType1] AS [Item6], [d_1].[Id] AS [Item7], [d_1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d_2].[Id] AS [Rest.Item2], [d_2].[Property_DerivedConcreteType3] AS [Rest.Item3]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_0] ON [b].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_1] ON [d_0].[Id] = [d_1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d_2] ON [d_0].[Id] = [d_2].[Id]",
                SqlLog);
        }

        [TestMethod]
        public void Select_subtype_simple()
        {
            var query = from d in Query<DerivedAbstractType1>()
                        select d;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [b].[Id] AS [Item1], [b].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedAbstractType1] AS [Item4], [d_0].[Id] AS [Item5], [d_0].[Property_DerivedConcreteType2] AS [Item6], [d_1].[Id] AS [Item7], [d_1].[Property_DerivedConcreteType3] AS [Rest.Item1]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_0] ON [d].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d_1] ON [d].[Id] = [d_1].[Id]",
                SqlLog);
        }

        [TestMethod]
        public void OfType_simple()
        {
            var query = impatient.CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression).OfType<DerivedAbstractType1>();

            query.ToList();

            Assert.AreEqual(
                @"SELECT [b].[Id] AS [Item1], [b].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d_0].[Id] AS [Item5], [d_0].[Property_DerivedAbstractType1] AS [Item6], [d_1].[Id] AS [Item7], [d_1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d_2].[Id] AS [Rest.Item2], [d_2].[Property_DerivedConcreteType3] AS [Rest.Item3]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_0] ON [b].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_1] ON [d_0].[Id] = [d_1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d_2] ON [d_0].[Id] = [d_2].[Id]
WHERE ([d_1].[Id] IS NOT NULL) OR ([d_2].[Id] IS NOT NULL)",
                SqlLog);
        }

        [TestMethod]
        public void TypeIs_simple()
        {
            var query = impatient
                .CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression)
                .Where(b => b is DerivedAbstractType1)
                .Select(b => b is DerivedConcreteType3);

            query.ToList();

            Assert.AreEqual(
                @"SELECT CAST((CASE WHEN [d].[Id] IS NOT NULL THEN 1 ELSE 0 END) AS BIT)
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d_0] ON [b].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_1] ON [b].[Id] = [d_1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_2] ON [d_1].[Id] = [d_2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d] ON [d_1].[Id] = [d].[Id]
WHERE ([d_2].[Id] IS NOT NULL) OR ([d].[Id] IS NOT NULL)",
                SqlLog);
        }

        [TestMethod]
        public void TypeEqual_simple()
        {
            var parameter = Expression.Parameter(typeof(BaseAbstractType1), "b");

            var predicate = Expression.Lambda(Expression.TypeEqual(parameter, typeof(DerivedAbstractType1)), parameter);
            var selector = Expression.Lambda(Expression.TypeEqual(parameter, typeof(DerivedConcreteType3)), parameter);

            var query = impatient
                .CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression)
                .Where(predicate as Expression<Func<BaseAbstractType1, bool>>)
                .Select(selector as Expression<Func<BaseAbstractType1, bool>>);

            query.ToList();

            Assert.AreEqual(
                @"SELECT CAST((CASE WHEN [d].[Id] IS NOT NULL THEN 1 ELSE 0 END) AS BIT)
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d_0] ON [b].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_1] ON [b].[Id] = [d_1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_2] ON [d_1].[Id] = [d_2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d] ON [d_1].[Id] = [d].[Id]
WHERE ([d_2].[Id] IS NOT NULL) OR ([d].[Id] IS NOT NULL)",
                SqlLog);
        }

        [TestMethod]
        public void TypeAs_simple()
        {
            var query = impatient
                .CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression)
                .Where(b => b is DerivedAbstractType1)
                .Select(b => b as DerivedConcreteType3);

            var results = query.ToList();

            Assert.AreEqual(6, results.Count);

            Assert.IsNull(results[0]);
            Assert.IsNull(results[1]);
            Assert.IsNull(results[2]);
            Assert.IsNotNull(results[3]);
            Assert.IsNotNull(results[4]);
            Assert.IsNotNull(results[5]);

            Assert.AreEqual(
                @"SELECT [b].[Id] AS [Item1], [b].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d_0].[Id] AS [Item5], [d_0].[Property_DerivedAbstractType1] AS [Item6], [d_1].[Id] AS [Item7], [d_1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d_2].[Id] AS [Rest.Item2], [d_2].[Property_DerivedConcreteType3] AS [Rest.Item3]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_0] ON [b].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_1] ON [d_0].[Id] = [d_1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d_2] ON [d_0].[Id] = [d_2].[Id]
WHERE ([d_1].[Id] IS NOT NULL) OR ([d_2].[Id] IS NOT NULL)",
                SqlLog);
        }

        [TestMethod]
        public void MemberAccess_simple()
        {
            var query = impatient
                .CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression)
                .Select(b => b.Property_BaseAbstractType1);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [b].[Property_BaseAbstractType1]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_0] ON [b].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_1] ON [d_0].[Id] = [d_1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d_2] ON [d_0].[Id] = [d_2].[Id]",
                SqlLog);
        }

        [TestMethod]
        public void Cast_up_simple()
        {
            var query = impatient
                .CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression)
                .OfType<DerivedConcreteType1>()
                .Cast<BaseAbstractType1>()
                .Select(b => b.Property_BaseAbstractType1);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [b].[Property_BaseAbstractType1]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_0] ON [b].[Id] = [d_0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_1] ON [d_0].[Id] = [d_1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d_2] ON [d_0].[Id] = [d_2].[Id]
WHERE [d].[Id] IS NOT NULL",
                SqlLog);
        }

        [TestMethod]
        public void Select_after_subquery_pushdown()
        {
            var query = impatient
                .CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression)
                .Take(10)
                .Where(b => b.Property_BaseAbstractType1 != null);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [b].[Item1] AS [Item1], [b].[Item2] AS [Item2], [b].[Item3] AS [Item3], [b].[Item4] AS [Item4], [b].[Item5] AS [Item5], [b].[Item6] AS [Item6], [b].[Item7] AS [Item7], [b].[Rest.Item1] AS [Rest.Item1], [b].[Rest.Item2] AS [Rest.Item2], [b].[Rest.Item3] AS [Rest.Item3]
FROM (
    SELECT TOP (10) [b_0].[Id] AS [Item1], [b_0].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d_0].[Id] AS [Item5], [d_0].[Property_DerivedAbstractType1] AS [Item6], [d_1].[Id] AS [Item7], [d_1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d_2].[Id] AS [Rest.Item2], [d_2].[Property_DerivedConcreteType3] AS [Rest.Item3]
    FROM [dbo].[BaseAbstractType1] AS [b_0]
    LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b_0].[Id] = [d].[Id]
    LEFT JOIN [dbo].[DerivedAbstractType1] AS [d_0] ON [b_0].[Id] = [d_0].[Id]
    LEFT JOIN [dbo].[DerivedConcreteType2] AS [d_1] ON [d_0].[Id] = [d_1].[Id]
    LEFT JOIN [dbo].[DerivedConcreteType3] AS [d_2] ON [d_0].[Id] = [d_2].[Id]
) AS [b]
WHERE [b].[Item2] IS NOT NULL",
                SqlLog);
        }

        private abstract class BaseAbstractType1
        {
            public int Id { get; set; }

            public string Property_BaseAbstractType1 { get; set; }
        }

        private abstract class DerivedAbstractType1 : BaseAbstractType1
        {
            public string Property_DerivedAbstractType1 { get; set; }
        }

        private class DerivedConcreteType1 : BaseAbstractType1
        {
            public string Property_DerivedConcreteType1 { get; set; }
        }

        private class DerivedConcreteType2 : DerivedAbstractType1
        {
            public string Property_DerivedConcreteType2 { get; set; }
        }

        private class DerivedConcreteType3 : DerivedAbstractType1
        {
            public string Property_DerivedConcreteType3 { get; set; }
        }
    }
}
