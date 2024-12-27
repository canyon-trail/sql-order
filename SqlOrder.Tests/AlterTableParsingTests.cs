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

        sql.AssertParsesTo(
            new Statement(
                Dependency.ArrayOf(
                    new ObjectName("dbo", "sometable").ToTableDependency(),
                    new ObjectName("otherschema", "othertable").ToTableDependency()
                )
            )
        );
    }
}
