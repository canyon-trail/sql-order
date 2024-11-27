using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class FunctionParsingTests
{
    [Fact]
    public void FunctionWithSelectStatement()
    {
        var sql = @"
            create function DoTheThing()
            returns int
            begin
                return (
                    select count(*) from SomeTable
                )
            end
         ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new FunctionDefinition(ObjectName.NoSchema("DoTheThing"),
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("SomeTable"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void FunctionWithUnion()
    {
        var sql = @"
CREATE FUNCTION [dbo].[GetTheThing]
(
	@ID INT
)
RETURNS TABLE
RETURN
(
	SELECT 
		auaa.ID_AAA
	FROM dbo.TheTable
	UNION ALL
	SELECT 
		auaa.ID_AAA
	FROM dbo.TheOtherTable
);

";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new FunctionDefinition(ObjectName.NoSchema("GetTheThing"),
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("TheTable"), DependencyKind.TableOrView),
                    new Dependency(ObjectName.NoSchema("TheOtherTable"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void WithCte()
    {
        var sql = @"
CREATE FUNCTION [dbo].[GetTheThing]
(
	@ID INT
)
RETURNS TABLE
RETURN
(
    WITH _cte1 as (select * from dbo.TheTable)
	SELECT 
		auaa.ID_AAA
	FROM _cte1
	UNION ALL
	SELECT 
		auaa.ID_AAA
	FROM dbo.TheOtherTable
);

";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new FunctionDefinition(ObjectName.NoSchema("GetTheThing"),
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("TheTable"), DependencyKind.TableOrView),
                    new Dependency(ObjectName.NoSchema("TheOtherTable"), DependencyKind.TableOrView)
                )
            ),
        });
    }
}
