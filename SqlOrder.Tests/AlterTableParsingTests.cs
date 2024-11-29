using Microsoft.SqlServer.Management.SqlParser.Parser;
using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class AlterTableParsingTests
{
    [Fact]
    public void ForeignKey()
    {
        var sql = @"
            ALTER TABLE sometable
            ADD CONSTRAINT [fk_whatevs]
            FOREIGN KEY ([fkcolumn]) REFERENCES
            [otherschema].[othertable] ([keycolumn])
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new Statement(
                Dependency.ArrayOf(
                    new ObjectName("otherschema", "othertable").ToTableDependency()
                )
            ),
        });
    }
}
