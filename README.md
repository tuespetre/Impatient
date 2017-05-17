# Impatient

> Ain't nobody got time for data

================================================================================

## Summary

The (current) goal of this project is to provide a reusable, composable, and 
performant implementation of `System.Linq.IQueryProvider` that targets 
relational database providers. The aim, then, is not to provide a complete
ORM solution but rather a robust component that can be used to build data
access layers and ORM solutions.

### Distinguishing Features

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
  
### Examples

The examples displayed here will be from (or derived from) the tests in this
project.

- **Querying a table**

  ```
var table = new BaseTableExpression("dbo", "MyClass1", "m", typeof(MyClass1));

var queryExpression 
  = new EnumerableRelationalQueryExpression(
      new SelectExpression(
        new ServerProjectionExpression(
            Expression.MemberInit(
                Expression.New(typeof(MyClass1)),
                from property in new[]
                {
                    typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Prop1)),
                    typeof(MyClass1).GetRuntimeProperty(nameof(MyClass1.Prop2))
                }
                let column = new SqlColumnExpression(table, property.Name, property.PropertyType)
                select Expression.Bind(property, column))),
        table));
        
var impatient = services.GetRequiredService<ImpatientQueryProvider>();

var results = (from m in impatient.CreateQuery<MyClass1>(queryExpression)
               where m.Prop1 == "This example is really contrived"
               select m).ToList();
  ```