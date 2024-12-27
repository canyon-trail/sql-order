using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class ViewParsingTests()
{
    [Fact]
    public void SimpleView()
    {
        var sql = @"
 CREATE view [exampleschema].[exampleview]
as
select * from table1;
 ";

        sql.AssertParsesTo(
            new TableOrViewDefinition(new ObjectName("exampleschema", "exampleview"),
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("table1"), DependencyKind.TableOrView)
                )
            )
        );
    }

    [Fact]
    public void ViewWithCte()
    {
        var sql = @"
 CREATE view [exampleschema].[exampleview]
as
with x as (select * from table1)
select * from x;
 ";

        sql.AssertParsesTo(
            new TableOrViewDefinition(new ObjectName("exampleschema", "exampleview"),
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("table1"), DependencyKind.TableOrView)
                )
            )
        );
    }
}
