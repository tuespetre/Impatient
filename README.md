# Impatient

> Ain't nobody got time for data

--------------------------------------------------------------------------------

## Summary

The (current) objective of this project is to provide a reusable, composable, and 
performant implementation of `System.Linq.IQueryProvider` that targets 
relational database providers. The aim, then, is not to provide a complete
ORM solution but rather a robust component that can be used to build data
access layers and ORM solutions.

### Distinguishing features

A number of features distinguish this project from a complete ORM solution:

- **Object-relational mapping**

  An ORM solution will offer some kind of `context` or `session` object which
  can be used to generate a query against a table or set of tables based on
  a configured object-relational mapping (or 'model'.)
  
  Impatient does not expose any kind of mapping or modelling API, because 
  (simply put) Impatient does not care about such details. Instead, Impatient
  expects all relevant metadata to exist within the expression tree itself.
  
  Because of this, the following types of translations are not provided by
  Impatient:
  
  - Object equality translation: rewriting equality expressions
    between mapped types into equality expressions between the keys of said
    mapped types
    
  - Navigation property translation: rewriting member access expressions
    into appropriate `join` operators or subqueries (such as `order.Customer`
    being rewritten into a join between `orders` and `customers`)
    
  - `Include/ThenInclude`-style operators: see above
  
- **Change tracking and `INSERT`/`UPDATE`/`DELETE` operations**

  Impatient is here for querying and only querying; however, a third party
  that wanted to implement change tracking on top of Impatient should be able
  to do so.

### Principles

-  Be as 'close to the metal' as possible: use `Expression`s and 
   `ExpressionVisitor`s rather than using or concocting a separate
   AST model

-  Be immutable: all `Expression`s must be immutable and `ExpressionVisitor`s
   should possess as little state as possible.

-  Be as robust as possible: try to compile queries in such a way 
   that queries can 'never fail' 

### Various notes regarding supported translations and future goals

-  `LEFT JOIN` and `OUTER APPLY` can be generated as expected:

    ```
	// LEFT JOIN
	from X x in xs
	join Y y in ys on x.z equals y.z into yg
	from Y y in yg.DefaultIfEmpty()
	select new { x, y }

	// OUTER APPLY
	from X x in xs
	join Y y in ys on x.z equals y.z into yg
	from Y y in yg.Take(3).DefaultIfEmpty()
	select new { x, y }
	```

-  `GROUP BY` can be generated as expected with some caveats:

    ```
	// Will produce a GROUP BY query at the server:
	from X x in xs
	group x by x.z into xg
	let maxA = xg.Max(x => x.a)
	let minB = xg.Max(x => x.b)
	let countDistinctC = xg.Select(x => x.c).Distinct().Count()
	select new { xg.Key, maxA, minB, countDistinctC } into x
	join Y y in ys on x.Key equals y.z
	select new { x, y }

	// Will require a client GroupBy operation because
	// the IGrouping is not 'cut off' from the rest of the query:
	from X x in xs
	group x by x.z into xg
	join Y y in ys on xg.Key equals y.z
	let maxA = xg.Max(x => x.a)
	select new { maxA, y }
	```

	This may be addressable in the future but it will involve
	some intense rewriting logic.

- Relational null semantics have not yet been addressed -- this includes
  the simplest of things like `IS NULL` and `IS NOT NULL`, and translating
  `foo.NullableColumn != 1` into `[foo].[NullableColumn] IS NULL OR
  [foo].[NullableColumn] <> 1`

- `Enum` types are not yet dealt with.

- No `async` support is currently provided.

- The project is currently structured around SQL Server; other providers
  should be supported (including but not limited to Oracle, Postgres, 
  and Sqlite.)

- Translation of `DateTime`/`TimeSpan`/`Math` and similar methods and members
  is not yet supported.

- Configurable support for nested (serialized) collections in projections 
  (using `FOR JSON`, `FOR XML`, and similar) would be nice; the project 
  currently includes some rough implementation using `FOR JSON`.

- Eventual support for the full gamut of standard LINQ query operators, 
  **even `TakeWhile` and `Zip`.**

- Currently unsupported operators:

  - Aggregate
  - ElementAt
  - ElementAtOrDefault
  - SequenceEqual
  - SkipWhile
  - TakeWhile
  - Zip
  - Operators with index parameter overloads
  - Operators with `IComparer` and `IEqualityComparer` overloads
  
### Examples and explanations

#### Constructing a query

This example shows what goes into constructing a very basic query.
The bulk of the code written here would tend to be either written once and
hidden behind a property or method to provide easy access, or generated 
by another component that provides a high-level object-relational mapping 
interface.

```
// Define the base table we are querying with the schema name, table name, 
// default alias, and object type.
var table = new BaseTableExpression("dbo", "MyClass1", "m", typeof(MyClass1));

// Define the materialization expression. The SqlColumnExpression instances
// will be replaced with the appropriate calls to read values from a DataReader.
var materializer
    = Expression.MemberInit(
        Expression.New(typeof(MyClass1)),
        from property in new[]
        {
            typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Prop1)),
            typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Prop2))
        }
        let column = new SqlColumnExpression(table, property.Name, property.PropertyType)
        select Expression.Bind(property, column))

// Create a query expression, which itself receives a SelectExpression. The
// SelectExpression requires at minimum a ProjectionExpression; in this case
// we are querying a table so we supply a TableExpression as well.
var queryExpression
  = new EnumerableRelationalQueryExpression(
      new SelectExpression(
        new ServerProjectionExpression(materializer),
        table));

// Obtain an instance of the ImpatientQueryProvider from our service container.
var impatient = services.GetRequiredService<ImpatientQueryProvider>();

// Query away!
var results = (from m in impatient.CreateQuery<MyClass1>(queryExpression)
               where m.Prop1 == "This example is really contrived"
               select m).ToList();
```

#### Using different materialization patterns

In order to pass values into the constructor of an object rather than
property setters, the materialization expression can be written this way:

```
var properties = new[]
{
    typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Prop1)),
    typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Prop2)),
};

var materializer
    = Expression.New(
        constructor: typeof(MyClass1).GetConstructors().Single(),
        arguments: from p in properties select new SqlColumnExpression(table, p.Name, p.PropertyType),
        members: properties);
```

A materialization expression should consist only of (possibly nested) 
`MemberInitExpression`s and `NewExpression`s. Any `NewExpression` that 
appears must include the `Members`, and any `MemberInitExpression` that 
appears must not include a `MemberListBinding` node.

#### `ProjectionExpression`s

There are three concrete implementations of the abstract `ProjectionExpression`
class included with Impatient:

- `ServerProjectionExpression`

  This `ProjectionExpression` represents a projection that is entirely
  translatable, and will typically be composed by the consumer with a 
  `MemberInitExpression` or a `NewExpression`.
  
- `ClientProjectionExpression`

  This `ProjectionExpression` represents a projection that is
  partially translatable. It contains a `LambdaExpression` that is
  translatable and another `LambdaExpression` that is not translatable,
  which transforms the result of the translatable `LambdaExpression`.
  
- `CompositeProjectionExpression`

  This `ProjectionExpression` also represents a projection that is
  partially translatable. It contains two `ProjectionExpression`s
  and a `LambdaExpression` that combines the results of the 
  `ProjectionExpression`s.
  
The `ClientProjectionExpression` and `CompositeProjectionExpression` types
are mostly produced by Impatient as the result of processing `Join`,
`GroupJoin`, `SelectMany`, `Select`, and similar operators.