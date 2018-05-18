# Impatient

> Ain't nobody got time for data
--------------------------------

- [Introduction](#introduction)
- [Getting Started with Impatient.EntityFrameworkCore.SqlServer](#getting-started)
- [Versioning & Compatibility](#versioning)
- [More About EF Core](#ef-core)
	- [Implementation Differences](#implementation-differences)
	- [Unsupported Features](#unsupported-features)
- [Sample Query Translations](#sample-translations) (coming soon)
- How Impatient Translates Queries
	- [A Primer](#primer)
	- [The Query Processing Pipeline](#pipeline)

## <a name="introduction"></a> Introduction

`Impatient` is a library that provides the infrastructure needed to build powerful
LINQ query providers for SQL databases. It offers support for:

- Navigation properties

- Almost every standard LINQ query operator, including (but not limited to):

	- `Select`/`Where` and their 'index argument' variants
	- `OrderBy`/`ThenBy` and their `Descending` variants
	- `Join`/`GroupJoin`/`SelectMany`/`GroupBy`
	- `Average`/`Count`/`LongCount`/`Max`/`Min`/`Sum`
	- `All`/`Any`/`Contains`
	- `First`/`Last`/`Single`/`ElementAt` and their `OrDefault` variants
	- `Concat`/`Except`/`Intersect`/`Union`
	- `SkipWhile`/`TakeWhile` and their 'index argument' variants
	- `Distinct`
	- `Reverse`
	- `SequenceEqual`
	- `Zip`

- Database native JSON support (like SQL Server's `FOR JSON`):

	- Materialize *nested collections* and complex-type columns within results instead of issuing n+1 queries
	- Query against properties of JSON objects
	- Query against elements of JSON arrays, treating them as if they were any other queryable sequence

`Impatient.EntityFrameworkCore.SqlServer` is a project that takes Impatient and
extends it to provide a substitution for EF Core's default `IQueryCompiler`. It is 
backed by EF Core's own specification test suites with over 5500 passing tests,
many of which demonstrate `Impatient`'s ability to translate some of the toughest 
LINQ queries that other query providers and ORMs cannot.
    
## <a name="getting-started"></a> Getting Started with Impatient.EntityFrameworkCore.SqlServer

1. Install `Impatient.EntityFrameworkCore.SqlServer`

2. Add `UseImpatientQueryCompiler()` to your `DbContextOptions`:

	```diff
	 services.AddDbContext<NorthwindDbContext>(options =>
	 {
		 options
			 .UseSqlServer(connectionString)
	+        .UseImpatientQueryCompiler();
	 });
	```

3. Cross arms, tap foot, run queries

## <a name="versioning"></a> Versioning & Compatibility

**The only currently supported database engine is SQL Server 2016 or newer** 
but it is a goal of the project to be extensible enough to foster the development
of support for other database engines.

The `Impatient` package is currently using an 'unstable' version number because 
it is expected that the API will see some significant changes yet before settling down.

The `Impatient.EntityFrameworkCore.SqlServer` package will be versioned according 
to the minor version of EF Core that is supports; that is, a 2.0.x version will 
support a 2.0.x version of EF Core, a 2.1.x version will support a 2.1.x version 
of EF Core, and so on. Until further notice, the `UseImpatientQueryCompiler`
extension method is the only designated stable public API in this package.

## <a name="ef-core"></a> More About EF Core

### <a name="implementation-differences"></a> Implementation Differences
  
- Impatient currently provides a naive implementation of async queries that
  does not make use of the cancellation token argument. Improvements to this area
  are planned.

- Impatient takes a pessimistic approach to change tracking when client 
  evaluation occurs. That means that when a selector is evaluated on the
  client where an entity is passed as an argument to some kind of expression
  like a method call or a constructor, Impatient is going to add that entity
  to the change tracker whereas EF Core's query compiler would not.

- The "manual left join" pattern of `GroupBy`/`SelectMany`/`DefaultIfEmpty`
  will not propagate nullability for navigations. This may or may not be 
  supported in the future. For now, the recommended course of action would be
  to either use navigations all the way or manual joins all the way.
  (See: the EFCore unit test `Manually_created_left_join_propagates_nullability_to_navigations`.)

- Calling `OrderBy` two times in a row will not perform a stable sort. 
  This is `Queryable`; we don't have to match the implementation of `Enumerable` here
  and it doesn't really make sense to. (See: the EFCore unit test `OrderBy_Multiple`.)

- Any exceptions thrown at runtime by expressions being applied to `DbParameter`
  values will be wrapped in an `InvalidOperationException` and rethrown with the
  original exception as the `InnerException`. This will probably not be an issue
  for anybody, but it is a difference nonetheless.
      
### <a name="unsupported-features"></a> Unsupported Features

- Certain translations like `System.Convert` calls and others are not
  implemented *yet* but are definitely planned for the future. The 
  section on sample translations will eventually be updated to enumerate
  all of the supported translations.

- Client evaluation warnings (and throwing behavior) are not supported
  but there are plans to implement support for it.

- Warnings for and forced client evaluation of exception-throwing aggregates 
  used in subqueries are not supported nor are there plans to support them.

- `ROW_NUMBER` paging is not supported nor are there plans to support it.
  Only the 'modern' `OFFSET`/`FETCH NEXT` is supported.

- "Null reference protection" is not supported nor are there plans to support it.

- `FromSql` is not supported nor are there plans to support it. If you want to use
  `FromSql` you should check out Dapper.

## <a name="sample-translations"></a> Sample Query Translations

> This section is coming soon! For now, here's a teaser taken from the test library:

```csharp
        [TestMethod]
        public void SequenceEqual_simple()
        {
            var m1s = impatient.CreateQuery<MyClass1>(MyClass1QueryExpression).Select(m1 => new { m1.Prop1, m1.Prop2 });
            var m2s = impatient.CreateQuery<MyClass2>(MyClass2QueryExpression).Select(m2 => new { m2.Prop1, m2.Prop2 });

            var result = m1s.SequenceEqual(m2s);

            Assert.IsTrue(result);

            Assert.AreEqual(
                @"SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM (
        SELECT [m1].[Prop1] AS [Prop1], [m1].[Prop2] AS [Prop2], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[MyClass1] AS [m1]
    ) AS [t]
    FULL JOIN (
        SELECT [m2].[Prop1] AS [Prop1], [m2].[Prop2] AS [Prop2], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[MyClass2] AS [m2]
    ) AS [t_0] ON [t].[$rownumber] = [t_0].[$rownumber]
    WHERE (([t].[$rownumber] IS NULL) OR ([t_0].[$rownumber] IS NULL)) OR (([t].[Prop1] <> [t_0].[Prop1]) OR ([t].[Prop2] <> [t_0].[Prop2]))
) THEN 0 ELSE 1 END) AS bit)",
                SqlLog);
        }
```

## How Impatient Translates Queries

### <a name="primer"></a> A Primer

*Note: this section assumes some familiarity with `IQueryable`, `Expression`, and `ExpressionVisitor`.*

A query begins with a `RelationalQueryExpression` which describes the resulting type of the query
and contains a child `SelectExpression`. The `SelectExpression` is the `Expression` equivalent of 
a SQL `SELECT` statement. Calls to `Queryable` methods like `Where`, `Select`, `GroupBy`, and so forth
are then interpreted by Impatient to compose upon the `RelationalQueryExpression`'s `SelectExpression`.

For instance, let's look at the Northwind database. We have a `Customers` table that we want to query.
The first thing we will do is define a basic `SelectExpression` representing the very basic SQL query
`SELECT [c].[CustomerID], [c].[CompanyName] /* and so on */ FROM [dbo].[Customers] AS [c]`:

```
var table = new BaseTableExpression("dbo", "Customer", "c", typeof(Customer));

var materializer
    = Expression.MemberInit(
        Expression.New(typeof(Customer)),
        from property in GetWhateverProperties(typeof(Customer))
        let column = new SqlColumnExpression(table, property.Name, property.PropertyType)
        select Expression.Bind(property, column));
        
var queryExpression
  = new EnumerableRelationalQueryExpression(
      new SelectExpression(
        new ServerProjectionExpression(materializer),
        table));
```

Then, we get an instance of our query provider and use `CreateQuery<Customer>(expression)` to 
get our `IQueryable<Customer>`:

```
var customers 
    = services
        .GetRequiredService<ImpatientQueryProvider>()
        .CreateQuery<Customer>(queryExpression);
```

Now that we have our customer query, we can... well, query on it:

```
var customers = 
    (from c in customers 
     where c.City == "Berlin" 
     select new { c.CustomerID, c.CompanyName, c.ContactName }).ToList();
```

At this point, Impatient will translate and compile the query and execute it. Let's
consider how it applies `Where` and `Select`. The expression tree contains a call
to `Queryable.Where(customers, c => c.City == "Berlin")`. Impatient grabs the query
expression from the first argument and the lambda expression from the second argument.

The projection expression from the query looks like this:

```
new Customer()
{
    CustomerID = [SqlColumnExpression],
    CompanyName = [SqlColumnExpression],
    ContactName = [SqlColumnExpression],
    City = [SqlColumnExpression],
    // and so on.
}
```

In order to interpret the `Where` predicate, Impatient replaces all instances of the 
parameter `c` with the current projection. So we get this:

```
new Customer()
{
    CustomerID = [SqlColumnExpression],
    CompanyName = [SqlColumnExpression],
    ContactName = [SqlColumnExpression],
    City = [SqlColumnExpression],
    // and so on.
}.City == "Berlin"
```

Impatient then simplifies everything it can in the expression. There is a member access
on the Customer to its `City` property, and because the Customer expression is a 
`MemberInitExpression` with `City` as a binding, we reduce the member access down to the
bound value:

```
[SqlColumnExpression] == "Berlin"
```

This is translatable to SQL, so it gets added to the `SelectExpression` and the call to
`Where` has now been replaced by a `RelationalQueryExpression`. The same process takes
place for the call to `Select`:

```
new { c.CustomerID, c.CompanyName, c.ContactName }
```

becomes

```
new { [SqlColumnExpression], [SqlColumnExpression], [SqlColumnExpression] }
```

which is translatable, so it replaces the projection on the `SelectExpression` and the
call to `Select` has now been replaced with a `RelationalQueryExpression`.

This is how all of the query operators are translated, although many are more involved,
like `GroupBy` or `SelectMany`.

### <a name="pipeline"></a> The Query Processing Pipeline

There are three main stages to processing a query expression.

1. Preparation

    The query expression is **parameterized**, meaning all constant expressions
    that are not literal constants are swapped out for parameter expressions.
    Literal constants are constants like numeric literals, string literals, and
    so forth. Basically, we look for closure instances and swap them out.
    
    The query expression is then **inlined**, meaning we look into all of the subtrees
    of type `IQueryable`, re-swap the constants back into place, and see if new query
    subtrees are produced. Those appear in lambda expressions, such as calls to `SelectMany`.
    If we successfully inline a query subtree, we will parameterize that subtree because
    it may reference new closures or other constants that were not present prior to inlining.
    
    Finally, the query expression is **hashed**, meaning we visit the entire tree and
    build a hash code based on the structure and semantics of the tree. This hash code is used
    to look up a previously compiled 'execution plan' for the query so it can just be run
    using the parameterized constants instead of having to complete the rest of the pipeline.

2. Composition

    The query expression is **composed** by a sequence of composing expression visitors.
    The default sequence of composing visitors consists of one that rewrites navigation properties
    into calls to the appropriate `Join`/`GroupJoin`/ etc. method calls, one that visits
    each call to a `Queryable` or `Enumerable` method with a lambda expression and uses
    the lambda's parameter name(s) to set the table aliases in the corresponding `SelectExpression`,
    and one that interprets calls to `Queryable` or `Enumerable` methods and uses them to
    compose (build) `SelectExpression`s. 
	
	That last visitor is in a sense *the* composing expression visitor, and it employs the use of 
	another sequence of expression visitors -- the **rewriting** expression visitors. They are named
	so because they rewrite certain types of expressions like `DateTime.Now` or `string.IsNullOrEmpty`
	into translatable forms using things like `SqlFunctionExpression`, `SqlAggregateExpression`, or
	`SqlInExpression`. These visitors are used every time a lambda's parameters are expanded, and
	afterward, the resulting expression is analyzed for translatability. The composing visitor then
	uses the outcome of that analysis to determine whether or not it needs to fall back to client 
	evaluation or in some cases take a hybrid approach.

	There is also a sequence of visitors known as the **optimizing** expression visitors. This sequence
	of visitors is applied before the composing visitors, between each of the composing visitors, and after
	all of the composing visitors. They consist of simple expression tree optimizations, like reducing
	or inverting some boolean-typed binary expressions, rewriting comparisons between conditional 
	expressions into a binary expression tree, or removing unnecessary type conversions. These visitors
	are run when they are run so that each composing visitor only needs to worry about applying
	such optimizations as needed for its own purposes.

3. Compilation

    The query expression is **compiled** by a sequence of compiling expression visitors.
    This is where the `SelectExpression`s are translated into SQL and the projection expressions
    are converted into materializer delegates that read from a `DbDataReader` to produce results.
    
    The compiled query expression is then used as the body of a lambda expression which is 
    given the parameters discovered in the preparation stage. The lambda expression is then compiled
    into an executable delegate and cached using the hash code from the preparation stage.