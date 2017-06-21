# Impatient

> Ain't nobody got time for data

## Summary

The (current) objective of this project is to provide a reusable, composable, and 
performant implementation of `System.Linq.IQueryProvider` that targets 
relational database providers. The aim, then, is not to provide a complete
ORM solution but rather a robust component that can be used to build data
access layers and ORM solutions. Accordingly, there are no plans for a high-level
modelling API, a change tracker, migrations, or other similar ORM features.

### Principles

-  Be as 'close to the metal' and 'out of the box' as possible: use 
  `Expression`s and `ExpressionVisitor`s rather than a bespoke
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

-  `GROUP BY` can be generated as expected:

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

	// Will produce a GROUP BY query at the server as well
	// as appropriate subqueries for when an aggregation 'escapes'
	// the GROUP BY context
	from X x in xs
	group x by x.z into xg
	join Y y in ys on xg.Key equals y.z
	let maxA = xg.Max(x => x.a)
	select new { maxA, y }
	```

- Object equality translation is supported via an API that
  allows consumers to define the key comparison expressions 
  the equality expressions should be rewritten into.

- Navigation property translation is supported via an API that
  allows consumers to define the corresponding key selectors and 
  query expressions to use in order to rewrite navigations into joins
  and subqueries. One-to-one optional navigations are not yet supported.

- Relational null semantics are partially addressed but still have some work to be done.

- `Enum` types are not yet dealt with.

- No `async` support is currently provided.

- The project is currently structured around SQL Server; other providers
  should be supported (including but not limited to Oracle, Postgres, 
  and Sqlite.)

- Translation of some common .NET APIs/idioms are supported:

    - `DateTime` (`DateTime.Now`, `date.AddDays(x)`, etc.)
	- `Nullable<T>` (`nullable.Value`, `nullable.HasValue`, `nullable.GetValueOrDefault()`)
	- `string` (`Trim`, `Concat`, `Length`)

- Some .NET APIs are not currently translated, like `Math` and `Convert`.

- Support for nested (serialized) collections in projections 
  (using `FOR JSON`, `FOR XML`, and similar) is a goal; the project 
  currently includes some rough implementation using `FOR JSON`.

- Currently unsupported `Queryable` operators:

  - `Aggregate`
  - Operators with `IComparer` and `IEqualityComparer` overloads

  It is unclear how exactly `Aggregate` could be translated considering
  that no relational database seems to offer a parallel (an aggregate
  function that can be expressed as a function expression with an optional
  seed value.) The operators with `IComparer` and `IEqualityComparer` overloads
  are also interesting as it is unclear how they were ever intended to be
  translatable to a remote data source of any kind.
  
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