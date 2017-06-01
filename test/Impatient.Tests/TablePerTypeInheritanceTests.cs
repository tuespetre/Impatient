using Dapper;
using Impatient.Query;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors;
using Impatient.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.SqlClient;
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

        private readonly Expression BaseTypeQueryExpression;

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

            var baseAbstractType1 = typeof(BaseAbstractType1);
            var baseAbstractType1Table = new BaseTableExpression("dbo", "BaseAbstractType1", "ba1", typeof(BaseAbstractType1));

            var derivedConcreteType1 = typeof(DerivedConcreteType1);
            var derivedConcreteType1Table = new BaseTableExpression("dbo", "DerivedConcreteType1", "dc1", typeof(DerivedConcreteType1));

            var derivedAbstractType1 = typeof(DerivedAbstractType1);
            var derivedAbstractType1Table = new BaseTableExpression("dbo", "DerivedAbstractType1", "da1", typeof(DerivedAbstractType1));

            var derivedConcreteType2 = typeof(DerivedConcreteType2);
            var derivedConcreteType2Table = new BaseTableExpression("dbo", "DerivedConcreteType2", "dc2", typeof(DerivedConcreteType2));
            
            var derivedConcreteType3 = typeof(DerivedConcreteType3);
            var derivedConcreteType3Table = new BaseTableExpression("dbo", "DerivedConcreteType3", "dc3", typeof(DerivedConcreteType3));
            
            Expression<Func<T>> GetExpression<T>(Expression<Func<T>> expression) => expression;

            var rowExpression
                = (GetExpression(() => new
                {
                    Id0 = default(int),
                    Property_BaseAbstractType1 = default(string),
                    Id1 = default(int?),
                    Property_DerivedConcreteType1 = default(string),
                    Id2 = default(int?),
                    Property_DerivedAbstractType1 = default(string),
                    Id3 = default(int?),
                    Property_DerivedConcreteType2 = default(string),
                    Id4 = default(int?),
                    Property_DerivedConcreteType3 = default(string),
                })
                    .Body as NewExpression)
                .Update(new[]
                {
                    new SqlColumnExpression(baseAbstractType1Table, "Id", typeof(int), false),
                    new SqlColumnExpression(baseAbstractType1Table, "Property_BaseAbstractType1", typeof(string), true),

                    new SqlColumnExpression(derivedConcreteType1Table, "Id", typeof(int?), true),
                    new SqlColumnExpression(derivedConcreteType1Table, "Property_DerivedConcreteType1", typeof(string), true),

                    new SqlColumnExpression(derivedAbstractType1Table, "Id", typeof(int?), true),
                    new SqlColumnExpression(derivedAbstractType1Table, "Property_DerivedAbstractType1", typeof(string), true),

                    new SqlColumnExpression(derivedConcreteType2Table, "Id", typeof(int?), true),
                    new SqlColumnExpression(derivedConcreteType2Table, "Property_DerivedConcreteType2", typeof(string), true),

                    new SqlColumnExpression(derivedConcreteType3Table, "Id", typeof(int?), true),
                    new SqlColumnExpression(derivedConcreteType3Table, "Property_DerivedConcreteType3", typeof(string), true),
                });

            var rowParam = Expression.Parameter(rowExpression.Type);

            var dct1expr
                = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(derivedConcreteType1),
                        from t in new[]
                        {
                            (nameof(DerivedConcreteType1.Id), "Id1"),
                            (nameof(DerivedConcreteType1.Property_BaseAbstractType1), "Property_BaseAbstractType1"),
                            (nameof(DerivedConcreteType1.Property_DerivedConcreteType1), "Property_DerivedConcreteType1"),
                        }
                        let p = derivedConcreteType1.GetProperty(t.Item1)
                        select Expression.Bind(
                            p,
                            Expression.Convert(
                                Expression.MakeMemberAccess(rowParam, rowParam.Type.GetProperty(t.Item2)),
                                p.PropertyType))),
                    rowParam);

            var dct2expr
                = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(derivedConcreteType2),
                        from t in new[]
                        {
                            (nameof(DerivedConcreteType2.Id), "Id3"),
                            (nameof(DerivedConcreteType2.Property_BaseAbstractType1), "Property_BaseAbstractType1"),
                            (nameof(DerivedConcreteType2.Property_DerivedAbstractType1), "Property_DerivedAbstractType1"),
                            (nameof(DerivedConcreteType2.Property_DerivedConcreteType2), "Property_DerivedConcreteType1"),
                        }
                        let p = derivedConcreteType2.GetProperty(t.Item1)
                        select Expression.Bind(
                            p,
                            Expression.Convert(
                                Expression.MakeMemberAccess(rowParam, rowParam.Type.GetProperty(t.Item2)),
                                p.PropertyType))),
                    rowParam);

            var dct3expr
                = Expression.Lambda(
                    Expression.MemberInit(
                        Expression.New(derivedConcreteType3),
                        from t in new[]
                        {
                            (nameof(DerivedConcreteType3.Id), "Id4"),
                            (nameof(DerivedConcreteType3.Property_BaseAbstractType1), "Property_BaseAbstractType1"),
                            (nameof(DerivedConcreteType3.Property_DerivedAbstractType1), "Property_DerivedAbstractType1"),
                            (nameof(DerivedConcreteType3.Property_DerivedConcreteType3), "Property_DerivedConcreteType3"),
                        }
                        let p = derivedConcreteType3.GetProperty(t.Item1)
                        select Expression.Bind(
                            p,
                            Expression.Convert(
                                Expression.MakeMemberAccess(rowParam, rowParam.Type.GetProperty(t.Item2)),
                                p.PropertyType))),
                    rowParam);

            var testParameter = Expression.Parameter(typeof(BaseAbstractType1), "x");

            var testExpression
                = Expression.Lambda(
                    Expression.NotEqual(
                        Expression.Convert(
                            Expression.MakeMemberAccess(
                                testParameter,
                                baseAbstractType1.GetProperty(nameof(BaseAbstractType1.Id))),
                            typeof(int?)),
                        Expression.Constant(null, typeof(int?))),
                    testParameter);

            BaseTypeQueryExpression
                = new EnumerableRelationalQueryExpression(
                    new SelectExpression(
                        new ServerProjectionExpression(
                            new PolymorphicExpression(
                                baseAbstractType1,
                                rowExpression,
                                new[]
                                {
                                    new PolymorphicExpression.TypeDescriptor(
                                        derivedConcreteType1,
                                        testExpression,
                                        dct1expr),
                                    new PolymorphicExpression.TypeDescriptor(
                                        derivedConcreteType2,
                                        testExpression,
                                        dct2expr),
                                    new PolymorphicExpression.TypeDescriptor(
                                        derivedConcreteType3,
                                        testExpression,
                                        dct3expr),
                                })),
                        new LeftJoinExpression(
                            new LeftJoinExpression(
                                new LeftJoinExpression(
                                    new LeftJoinExpression(
                                        baseAbstractType1Table,
                                        derivedConcreteType1Table,
                                        Expression.Equal(
                                            new SqlColumnExpression(baseAbstractType1Table, "Id", typeof(int), false),
                                            new SqlColumnExpression(derivedConcreteType1Table, "Id", typeof(int), false)),
                                        baseAbstractType1),
                                    derivedAbstractType1Table,
                                    Expression.Equal(
                                        new SqlColumnExpression(baseAbstractType1Table, "Id", typeof(int), false),
                                        new SqlColumnExpression(derivedAbstractType1Table, "Id", typeof(int), false)),
                                    baseAbstractType1),
                                derivedConcreteType2Table,
                                Expression.Equal(
                                    new SqlColumnExpression(derivedAbstractType1Table, "Id", typeof(int), false),
                                    new SqlColumnExpression(derivedConcreteType2Table, "Id", typeof(int), false)),
                                baseAbstractType1),
                            derivedConcreteType3Table,
                            Expression.Equal(
                                new SqlColumnExpression(derivedAbstractType1Table, "Id", typeof(int), false),
                                new SqlColumnExpression(derivedConcreteType3Table, "Id", typeof(int), false)),
                            baseAbstractType1)));

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
    [Discriminator] int not null,
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

INSERT INTO BaseAbstractType1 (Id, Discriminator, Property_BaseAbstractType1)
VALUES
(1, 1, 'a'),
(2, 1, 'b'),
(3, 1, 'c'),
(4, 2, 'a'),
(5, 2, 'b'),
(6, 2, 'c'),
(7, 3, 'a'),
(8, 3, 'b'),
(9, 3, 'c');

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
                @"SELECT [ba1].[Id] AS [Id0], [ba1].[Property_BaseAbstractType1] AS [Property_BaseAbstractType1], [dc1].[Id] AS [Id1], [dc1].[Property_DerivedConcreteType1] AS [Property_DerivedConcreteType1], [da1].[Id] AS [Id2], [da1].[Property_DerivedAbstractType1] AS [Property_DerivedAbstractType1], [dc2].[Id] AS [Id3], [dc2].[Property_DerivedConcreteType2] AS [Property_DerivedConcreteType2], [dc3].[Id] AS [Id4], [dc3].[Property_DerivedConcreteType3] AS [Property_DerivedConcreteType3]
FROM [dbo].[BaseAbstractType1] AS [ba1]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]",
                SqlLog);
        }

        [TestMethod]
        public void OfType_simple()
        {
            var query = impatient.CreateQuery<BaseAbstractType1>(BaseTypeQueryExpression).OfType<DerivedAbstractType1>();

            query.ToList();

            Assert.AreEqual(
                @"SELECT [ba1].[Id] AS [Id0], [ba1].[Property_BaseAbstractType1] AS [Property_BaseAbstractType1], [dc1].[Id] AS [Id1], [dc1].[Property_DerivedConcreteType1] AS [Property_DerivedConcreteType1], [da1].[Id] AS [Id2], [da1].[Property_DerivedAbstractType1] AS [Property_DerivedAbstractType1], [dc2].[Id] AS [Id3], [dc2].[Property_DerivedConcreteType2] AS [Property_DerivedConcreteType2], [dc3].[Id] AS [Id4], [dc3].[Property_DerivedConcreteType3] AS [Property_DerivedConcreteType3]
FROM [dbo].[BaseAbstractType1] AS [ba1]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]
WHERE ([dc2].[Id] IS NOT NULL) OR ([dc3].[Id] IS NOT NULL)",
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
                @"SELECT CAST((CASE WHEN [dc3].[Id] IS NOT NULL THEN 1 ELSE 0 END) AS BIT)
FROM [dbo].[BaseAbstractType1] AS [ba1]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]
WHERE ([dc2].[Id] IS NOT NULL) OR ([dc3].[Id] IS NOT NULL)",
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
                @"SELECT CAST((CASE WHEN [dc3].[Id] IS NOT NULL THEN 1 ELSE 0 END) AS BIT)
FROM [dbo].[BaseAbstractType1] AS [ba1]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]
WHERE ([dc2].[Id] IS NOT NULL) OR ([dc3].[Id] IS NOT NULL)",
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
                @"SELECT [ba1].[Id] AS [Id0], [ba1].[Property_BaseAbstractType1] AS [Property_BaseAbstractType1], [dc1].[Id] AS [Id1], [dc1].[Property_DerivedConcreteType1] AS [Property_DerivedConcreteType1], [da1].[Id] AS [Id2], [da1].[Property_DerivedAbstractType1] AS [Property_DerivedAbstractType1], [dc2].[Id] AS [Id3], [dc2].[Property_DerivedConcreteType2] AS [Property_DerivedConcreteType2], [dc3].[Id] AS [Id4], [dc3].[Property_DerivedConcreteType3] AS [Property_DerivedConcreteType3]
FROM [dbo].[BaseAbstractType1] AS [ba1]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]
WHERE ([dc2].[Id] IS NOT NULL) OR ([dc3].[Id] IS NOT NULL)",
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
                @"SELECT [ba1].[Property_BaseAbstractType1]
FROM [dbo].[BaseAbstractType1] AS [ba1]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]",
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
                @"SELECT [ba1].[Property_BaseAbstractType1]
FROM [dbo].[BaseAbstractType1] AS [ba1]
LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]
WHERE [dc1].[Id] IS NOT NULL",
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
                @"SELECT [b].[Id0] AS [Id0], [b].[Property_BaseAbstractType1] AS [Property_BaseAbstractType1], [b].[Id1] AS [Id1], [b].[Property_DerivedConcreteType1] AS [Property_DerivedConcreteType1], [b].[Id2] AS [Id2], [b].[Property_DerivedAbstractType1] AS [Property_DerivedAbstractType1], [b].[Id3] AS [Id3], [b].[Property_DerivedConcreteType2] AS [Property_DerivedConcreteType2], [b].[Id4] AS [Id4], [b].[Property_DerivedConcreteType3] AS [Property_DerivedConcreteType3]
FROM (
    SELECT TOP (10) [ba1].[Id] AS [Id0], [ba1].[Property_BaseAbstractType1] AS [Property_BaseAbstractType1], [dc1].[Id] AS [Id1], [dc1].[Property_DerivedConcreteType1] AS [Property_DerivedConcreteType1], [da1].[Id] AS [Id2], [da1].[Property_DerivedAbstractType1] AS [Property_DerivedAbstractType1], [dc2].[Id] AS [Id3], [dc2].[Property_DerivedConcreteType2] AS [Property_DerivedConcreteType2], [dc3].[Id] AS [Id4], [dc3].[Property_DerivedConcreteType3] AS [Property_DerivedConcreteType3]
    FROM [dbo].[BaseAbstractType1] AS [ba1]
    LEFT JOIN [dbo].[DerivedConcreteType1] AS [dc1] ON [ba1].[Id] = [dc1].[Id]
    LEFT JOIN [dbo].[DerivedAbstractType1] AS [da1] ON [ba1].[Id] = [da1].[Id]
    LEFT JOIN [dbo].[DerivedConcreteType2] AS [dc2] ON [da1].[Id] = [dc2].[Id]
    LEFT JOIN [dbo].[DerivedConcreteType3] AS [dc3] ON [da1].[Id] = [dc3].[Id]
) AS [b]
WHERE [b].[Property_BaseAbstractType1] IS NOT NULL",
                SqlLog);
        }

        private abstract class BaseAbstractType1
        {
            public int Id { get; set; }

            public int Discriminator { get; set; }

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
