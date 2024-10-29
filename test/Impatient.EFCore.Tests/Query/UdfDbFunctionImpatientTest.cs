using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class UdfDbFunctionImpatientTest : UdfDbFunctionTestBase<UdfDbFunctionImpatientTest.SqlServerUDFFixture>
{
    public UdfDbFunctionImpatientTest(SqlServerUDFFixture fixture) : base(fixture)
    {
        Fixture.ListLoggerFactory.Clear();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_CrossApply_Correlated_Select_Anonymous()
    {
        base.QF_CrossApply_Correlated_Select_Anonymous();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_CrossApply_Correlated_Select_QF_Type()
    {
        base.QF_CrossApply_Correlated_Select_QF_Type();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_CrossApply_Correlated_Select_Result()
    {
        base.QF_CrossApply_Correlated_Select_Result();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_CrossJoin_Not_Correlated()
    {
        base.QF_CrossJoin_Not_Correlated();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_CrossJoin_Parameter()
    {
        base.QF_CrossJoin_Parameter();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Join()
    {
        base.QF_Join();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_LeftJoin_Select_Anonymous()
    {
        base.QF_LeftJoin_Select_Anonymous();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_LeftJoin_Select_Result()
    {
        base.QF_LeftJoin_Select_Result();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_OuterApply_Correlated_Select_Anonymous()
    {
        base.QF_OuterApply_Correlated_Select_Anonymous();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_OuterApply_Correlated_Select_Entity()
    {
        base.QF_OuterApply_Correlated_Select_Entity();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_OuterApply_Correlated_Select_QF()
    {
        base.QF_OuterApply_Correlated_Select_QF();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections()
    {
        base.QF_Select_Correlated_Subquery_In_Anonymous_MultipleCollections();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Select_Correlated_Subquery_In_Anonymous_Nested()
    {
        base.QF_Select_Correlated_Subquery_In_Anonymous_Nested();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Select_Direct_In_Anonymous()
    {
        base.QF_Select_Direct_In_Anonymous();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Select_NonCorrelated_Subquery_In_Anonymous()
    {
        base.QF_Select_NonCorrelated_Subquery_In_Anonymous();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter()
    {
        base.QF_Select_NonCorrelated_Subquery_In_Anonymous_Parameter();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Stand_Alone()
    {
        base.QF_Stand_Alone();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void QF_Stand_Alone_Parameter()
    {
        base.QF_Stand_Alone_Parameter();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void Udf_with_argument_being_comparison_of_nullable_columns()
    {
        base.Udf_with_argument_being_comparison_of_nullable_columns();
    }

    [Fact(Skip = EFCoreSkipReasons.TestRunAborts)]
    public override void Udf_with_argument_being_comparison_to_null_parameter()
    {
        base.Udf_with_argument_being_comparison_to_null_parameter();
    }

    public class SqlServerUDFFixture : UdfFixtureBase
    {
        protected override string StoreName { get; } = "UDFDbFunctionSqlServerTests";

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance; 
        
        protected override void Seed(DbContext context)
        {
            base.Seed(context);

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[CustomerOrderCount] (@customerId int)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select count(id) from orders where customerId = @customerId);
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function[dbo].[StarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('*', @starCount) + @value
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function[dbo].[DollarValue] (@starCount int, @value nvarchar(max))
                                                    returns nvarchar(max)
                                                        as
                                                        begin
                                                    return replicate('$', @starCount) + @value
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[GetReportingPeriodStartDate] (@period int)
                                                    returns DateTime
                                                    as
                                                    begin
                                                        return '1998-01-01'
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[GetCustomerWithMostOrdersAfterDate] (@searchDate Date)
                                                    returns int
                                                    as
                                                    begin
                                                        return (select top 1 customerId
                                                                from orders
                                                                where orderDate > @searchDate
                                                                group by CustomerId
                                                                order by count(id) desc)
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[IsTopCustomer] (@customerId int)
                                                    returns bit
                                                    as
                                                    begin
                                                        if(@customerId = 1)
                                                            return 1

                                                        return 0
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[IdentityString] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return @s;
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[IdentityStringPropagatesNull] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return @s;
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[IdentityStringNonNullable] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return COALESCE(@s, 'NULL');
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[IdentityStringNonNullableFluent] (@s nvarchar(max))
                                                    returns nvarchar(max)
                                                    as
                                                    begin
                                                        return COALESCE(@s, 'NULL');
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[StringLength] (@s nvarchar(max))
                                                    returns int
                                                    as
                                                    begin
                                                        return LEN(@s);
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].GetCustomerOrderCountByYear(@customerId int)
                                                    returns @reports table
                                                    (
                                                        CustomerId int not null,
                                                        Count int not null,
                                                        Year int not null
                                                    )
                                                    as
                                                    begin
                                                        insert into @reports
                                                        select @customerId, count(id), year(orderDate)
                                                        from orders
                                                        where customerId = @customerId
                                                        group by customerId, year(orderDate)
                                                        order by year(orderDate)

                                                        return
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].GetCustomerOrderCountByYearOnlyFrom2000(@customerId int, @onlyFrom2000 bit)
                                                    returns @reports table
                                                    (
                                                        CustomerId int not null,
                                                        Count int not null,
                                                        Year int not null
                                                    )
                                                    as
                                                    begin
                                                        insert into @reports
                                                        select @customerId, count(id), year(orderDate)
                                                        from orders
                                                        where customerId = 1 AND (@onlyFrom2000 = 0 OR @onlyFrom2000 IS NULL OR (@onlyFrom2000 = 1 AND year(orderDate) = 2000))
                                                        group by customerId, year(orderDate)
                                                        order by year(orderDate)

                                                        return
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].GetTopTwoSellingProducts()
                                                    returns @products table
                                                    (
                                                        ProductId int not null,
                                                        AmountSold int
                                                    )
                                                    as
                                                    begin
                                                        insert into @products
                                                        select top 2 ProductID, sum(Quantity) as totalSold
                                                        from lineItem
                                                        group by ProductID
                                                        order by totalSold desc

                                                        return
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].GetTopSellingProductsForCustomer(@customerId int)
                                                    returns @products table
                                                    (
                                                        ProductId int not null,
                                                        AmountSold int
                                                    )
                                                    as
                                                    begin
                                                        insert into @products
                                                        select ProductID, sum(Quantity) as totalSold
                                                        from lineItem li
                                                        join orders o on o.id = li.orderId
                                                        where o.customerId = @customerId
                                                        group by ProductID

                                                        return
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].GetOrdersWithMultipleProducts(@customerId int)
                                                    returns @orders table
                                                    (
                                                        OrderId int not null,
                                                        CustomerId int not null,
                                                        OrderDate dateTime2
                                                    )
                                                    as
                                                    begin
                                                        insert into @orders
                                                        select o.id, @customerId, OrderDate
                                                        from orders o
                                                        join lineItem li on o.id = li.orderId
                                                        where o.customerId = @customerId
                                                        group by o.id, OrderDate
                                                        having count(productId) > 1

                                                        return
                                                    end");

            context.Database.ExecuteSqlRaw(
                @"create function [dbo].[AddValues] (@a int, @b int)
                                                    returns int
                                                    as
                                                    begin
                                                        return @a + @b;
                                                    end");

            context.SaveChanges();
        }
    }
}
