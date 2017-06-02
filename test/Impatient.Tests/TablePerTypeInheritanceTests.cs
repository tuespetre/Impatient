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

namespace Impatient.Tests
{
    [TestClass]
    public class TablePerTypeInheritanceTests
    {
        private readonly StringBuilder commandLog = new StringBuilder();
        private readonly ImpatientQueryProvider impatient;

        private string SqlLog => commandLog.ToString();

        private static readonly Expression BaseTypeQueryExpression;

        public struct TreeNode<TValue>
        {
            public TValue Value;
            public IEnumerable<TreeNode<TValue>> Children;

            public IEnumerable<TResult> Flatten<TResult>(Func<TreeNode<TValue>, TResult> selector)
            {
                yield return selector(this);

                foreach (var value in Children.SelectMany(c => c.Flatten(selector)))
                {
                    yield return value;
                }
            }

            public TreeNode<TResult> Transform<TResult>(Func<TreeNode<TValue>, TResult> selector)
            {
                return new TreeNode<TResult>
                {
                    Value = selector(this),
                    Children = Children.Select(c => c.Transform(selector)),
                };
            }

            public TAccumulate Aggregate<TAccumulate>(
                TAccumulate seed,
                Func<TAccumulate, TreeNode<TValue>, TreeNode<TValue>, TAccumulate> accumulator)
            {
                var me = this;

                return Children.Aggregate(
                    seed,
                    (result, next) => next.Aggregate(
                        accumulator(result, me, next),
                        accumulator));
            }
        }

        public static class ValueTupleHelper
        {
            public static Type CreateTupleType(IEnumerable<Type> types)
            {
                var result = default(Type);
                var remainder = types.Count() % 7;

                types = types.Reverse().ToArray().AsEnumerable();

                if (remainder > 0)
                {
                    var genericTupleType
                        = remainder == 1 ? typeof(ValueTuple<>)
                        : remainder == 2 ? typeof(ValueTuple<,>)
                        : remainder == 3 ? typeof(ValueTuple<,,>)
                        : remainder == 4 ? typeof(ValueTuple<,,,>)
                        : remainder == 5 ? typeof(ValueTuple<,,,,>)
                        : typeof(ValueTuple<,,,,,>);

                    result
                        = genericTupleType.MakeGenericType(
                            types
                                .Take(remainder)
                                .Reverse()
                                .ToArray());

                    types = types.Skip(remainder);
                }
                else
                {
                    result
                        = typeof(ValueTuple<,,,,,,>).MakeGenericType(
                            types
                                .Take(7)
                                .Reverse()
                                .ToArray());

                    types = types.Skip(7);
                }

                while (types.Any())
                {
                    result
                        = typeof(ValueTuple<,,,,,,,>).MakeGenericType(
                            types
                                .Take(7)
                                .Reverse()
                                .Concat(Enumerable.Repeat(result, 1))
                                .ToArray());

                    types = types.Skip(7);
                }

                return result;
            }

            public static NewExpression CreateNewExpression(Type type, IEnumerable<Expression> arguments)
            {
                var typeInfo = type.GetTypeInfo();
                var constructor = typeInfo.DeclaredConstructors.Single();
                var fields = typeInfo.DeclaredFields.ToArray();
                var newArguments = new List<Expression>(fields.Length);

                foreach (var argument in arguments.TakeWhile((b, i) => i < 7 && i < fields.Length))
                {
                    newArguments.Add(argument);
                }

                if (fields.Length == 8)
                {
                    newArguments.Add(
                        CreateNewExpression(
                            fields[7].FieldType,
                            arguments.Skip(7)));
                }

                return Expression.New(constructor, newArguments, fields);
            }

            public static Expression CreateMemberExpression(Type type, Expression expression, int index)
            {
                for (var i = 0; i < index / 7; i++)
                {
                    var restField = type.GetRuntimeField("Rest");
                    expression = Expression.MakeMemberAccess(expression, restField);
                    type = restField.FieldType;
                }

                var itemField = type.GetTypeInfo().DeclaredFields.ElementAt(index % 7);

                return Expression.MakeMemberAccess(expression, itemField);
            }
        }

        static TablePerTypeInheritanceTests()
        {
            #region helper functions

            IEnumerable<Type> MakeChain(Type type)
            {
                do
                {
                    yield return type;

                    type = type.GetTypeInfo().BaseType;
                }
                while (type != typeof(object));
            }

            IEnumerable<TreeNode<TValue>> MakeTree<TValue>(IEnumerable<IEnumerable<TValue>> chains)
            {
                return from c in chains
                       where c.Any()
                       group c.Skip(1) by c.First() into cg
                       select new TreeNode<TValue>
                       {
                           Value = cg.Key,
                           Children = MakeTree(cg),
                       };
            }

            #endregion

            var concreteTypes = new[]
            {
                typeof(DerivedConcreteType1),
                typeof(DerivedConcreteType2),
                typeof(DerivedConcreteType3),
            };

            var hierarchyRoot = MakeTree(concreteTypes.Select(t => MakeChain(t).Reverse()).OrderBy(c => c.Count())).Single();
            var keyPropertyNames = new[] { "Id" };

            // TODO: propertyType needs to be dynamic
            var bindings = (from type in hierarchyRoot.Flatten(n => n.Value)
                            let table = new BaseTableExpression("dbo", type.Name, type.Name.ToLower().Substring(0, 1), type)
                            let k = (from n in keyPropertyNames select type.GetRuntimeProperty(n))
                            from property in k.Union(type.GetTypeInfo().DeclaredProperties)
                            let propertyType = property.PropertyType == typeof(int) ? typeof(int?) : property.PropertyType
                            let column = new SqlColumnExpression(table, property.Name, propertyType, isNullable: true)
                            select (type: type, table: table, property: property, column: column)).ToArray();

            var tupleType = ValueTupleHelper.CreateTupleType(bindings.Select(b => b.column.Type));
            var tupleNewExpression = ValueTupleHelper.CreateNewExpression(tupleType, bindings.Select(b => b.column));
            var tupleParameter = Expression.Parameter(tupleType);

            var descriptors = new List<PolymorphicExpression.TypeDescriptor>();

            foreach (var concreteType in concreteTypes)
            {
                var currentChain = MakeChain(concreteType).ToArray();
                var memberBindings = new List<MemberBinding>();
                var boundPropertyTokens = new HashSet<int>();

                for (var i = 0; i < bindings.Length; i++)
                {
                    var binding = bindings[i];

                    if (!currentChain.Contains(binding.type)
                        || !boundPropertyTokens.Add(binding.property.MetadataToken))
                    {
                        continue;
                    }

                    memberBindings.Add(
                        Expression.Bind(
                            binding.property,
                            Expression.Convert(
                                ValueTupleHelper.CreateMemberExpression(tupleType, tupleParameter, i),
                                binding.property.PropertyType)));
                }

                var keyProperty = concreteType.GetRuntimeProperty(keyPropertyNames.First());
                var keyIndex = bindings.ToList().FindIndex(b => b.property == keyProperty);

                // TODO: typeof(int?) needs to be dynamic
                var test
                    = Expression.Lambda(
                        Expression.NotEqual(
                            ValueTupleHelper.CreateMemberExpression(tupleType, tupleParameter, keyIndex),
                            Expression.Constant(null, typeof(int?))),
                        tupleParameter);

                var materializer
                    = Expression.Lambda(
                        Expression.MemberInit(
                            Expression.New(concreteType),
                            memberBindings),
                        tupleParameter);

                descriptors.Add(new PolymorphicExpression.TypeDescriptor(concreteType, test, materializer));
            }

            // TODO: The Equal expression needs to be dynamic
            var tableExpression
                = hierarchyRoot
                    .Transform(
                        (node) =>
                        {
                            return bindings.First(b => b.type == node.Value).table;
                        })
                    .Aggregate(
                        bindings.First().column.Table as TableExpression,
                        (accumulate, parent, child) =>
                        {
                            return new LeftJoinExpression(
                                accumulate,
                                child.Value,
                                Expression.Equal(
                                    new SqlColumnExpression(parent.Value, keyPropertyNames.First(), typeof(int), false),
                                    new SqlColumnExpression(child.Value, keyPropertyNames.First(), typeof(int), false)),
                                hierarchyRoot.Value);
                        });

            BaseTypeQueryExpression
                = new EnumerableRelationalQueryExpression(
                    new SelectExpression(
                        new ServerProjectionExpression(
                            new PolymorphicExpression(
                                typeof(BaseAbstractType1),
                                tupleNewExpression,
                                descriptors)),
                        tableExpression));
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
                @"SELECT [b].[Id] AS [Item1], [b].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d0].[Id] AS [Item5], [d0].[Property_DerivedAbstractType1] AS [Item6], [d1].[Id] AS [Item7], [d1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d2].[Id] AS [Rest.Item2], [d2].[Property_DerivedConcreteType3] AS [Rest.Item3]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d0] ON [b].[Id] = [d0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d1] ON [d0].[Id] = [d1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d2] ON [d0].[Id] = [d2].[Id]",
                SqlLog);
        }

        [TestMethod]
        public void OfType_simple()
        {
            var query = impatient.CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression).OfType<DerivedAbstractType1>();

            query.ToList();

            Assert.AreEqual(
                @"SELECT [b].[Id] AS [Item1], [b].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d0].[Id] AS [Item5], [d0].[Property_DerivedAbstractType1] AS [Item6], [d1].[Id] AS [Item7], [d1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d2].[Id] AS [Rest.Item2], [d2].[Property_DerivedConcreteType3] AS [Rest.Item3]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d0] ON [b].[Id] = [d0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d1] ON [d0].[Id] = [d1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d2] ON [d0].[Id] = [d2].[Id]
WHERE ([d1].[Id] IS NOT NULL) OR ([d2].[Id] IS NOT NULL)",
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
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d0] ON [b].[Id] = [d0].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d1] ON [b].[Id] = [d1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d2] ON [d1].[Id] = [d2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d] ON [d1].[Id] = [d].[Id]
WHERE ([d2].[Id] IS NOT NULL) OR ([d].[Id] IS NOT NULL)",
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
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d0] ON [b].[Id] = [d0].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d1] ON [b].[Id] = [d1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d2] ON [d1].[Id] = [d2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d] ON [d1].[Id] = [d].[Id]
WHERE ([d2].[Id] IS NOT NULL) OR ([d].[Id] IS NOT NULL)",
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
                @"SELECT [b].[Id] AS [Item1], [b].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d0].[Id] AS [Item5], [d0].[Property_DerivedAbstractType1] AS [Item6], [d1].[Id] AS [Item7], [d1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d2].[Id] AS [Rest.Item2], [d2].[Property_DerivedConcreteType3] AS [Rest.Item3]
FROM [dbo].[BaseAbstractType1] AS [b]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b].[Id] = [d].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d0] ON [b].[Id] = [d0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d1] ON [d0].[Id] = [d1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d2] ON [d0].[Id] = [d2].[Id]
WHERE ([d1].[Id] IS NOT NULL) OR ([d2].[Id] IS NOT NULL)",
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
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d0] ON [b].[Id] = [d0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d1] ON [d0].[Id] = [d1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d2] ON [d0].[Id] = [d2].[Id]",
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
LEFT JOIN [dbo].[DerivedAbstractType1] AS [d0] ON [b].[Id] = [d0].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [d1] ON [d0].[Id] = [d1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [d2] ON [d0].[Id] = [d2].[Id]
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
    SELECT TOP (10) [b0].[Id] AS [Item1], [b0].[Property_BaseAbstractType1] AS [Item2], [d].[Id] AS [Item3], [d].[Property_DerivedConcreteType1] AS [Item4], [d0].[Id] AS [Item5], [d0].[Property_DerivedAbstractType1] AS [Item6], [d1].[Id] AS [Item7], [d1].[Property_DerivedConcreteType2] AS [Rest.Item1], [d2].[Id] AS [Rest.Item2], [d2].[Property_DerivedConcreteType3] AS [Rest.Item3]
    FROM [dbo].[BaseAbstractType1] AS [b0]
    LEFT JOIN [dbo].[DerivedConcreteType1] AS [d] ON [b0].[Id] = [d].[Id]
    LEFT JOIN [dbo].[DerivedAbstractType1] AS [d0] ON [b0].[Id] = [d0].[Id]
    LEFT JOIN [dbo].[DerivedConcreteType2] AS [d1] ON [d0].[Id] = [d1].[Id]
    LEFT JOIN [dbo].[DerivedConcreteType3] AS [d2] ON [d0].[Id] = [d2].[Id]
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
