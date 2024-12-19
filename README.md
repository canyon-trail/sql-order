# SQLOrder
SQLOrder is a .Net library that analyzes SQL files for dependencies so they can
be run in the correct order. Targets MS SQL Server. 

For instance, let's say you have these two files:

```sql
-- someTable.sql

create table someTable (
    id int not null
);
```
```sql
-- someProc.sql

create procedure someProc
    as
begin
select * from someTable;
end;
```
If you run these two scripts in the wrong order, you'll get an error since `someTable`
won't exist.

SQLOrder will analyze the SQL files you give it, build a dependency graph, and give you
back the files in the correct order so that you can easily build your schema from raw SQL files
without needing to manually keep up with the dependencies:

```csharp
using SqlOrder;

var sqlFiles = new[] {
    // likely you'll want to do a filesystem enumeration or something here
    new FileScript("someTable.sql"),
    new FileScript("someProc.sql"),
};

var orderer = new ScriptOrderer();
var results = await orderer.OrderScripts(sqlFiles, CancellationToken.None);
```

SqlOrder understands dependencies on:
* Schemas
* Tables and views
* User-defined functions
* Stored procedures
* User-defined data types
* Sequences

# User-defined Dependencies

In some cases, SqlOrder may not notice or understand a dependency. In those cases,
it would cause the ordering it provides to be incorrect, meaning that running the SQL
files in the order that SqlOrder provides would produce an error from SQL Server.
If you need to work around a situation like this, you can add user-defined dependencies
to the dependency model:

```csharp

var sqlFilesByName = new[] {
        // your files here
    }
    .ToDictionary(x => x.Name);

var orderer = new ScriptOrderer();

// badResults incorrectly has "scriptA.sql" first even though it depends on
// something in "scriptB.sql".
var badResults = await orderer.OrderScripts(sqlFilesByName.Values, CancellationToken.None);

var model = await orderer.BuildModel();
model.AddDependency(
    predecessor: sqlFilesByName["scriptB.sql"],
    successor: sqlFilesByName["scriptA.sql"],
);

var goodResults = model.All;
```
