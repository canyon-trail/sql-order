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

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new TableOrViewDefinition(new ObjectName("exampleschema", "exampleview"),
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("table1"), DependencyKind.TableOrView)
                )
            ),
        });
    }
}
