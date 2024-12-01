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

    [Fact]
    public void WithSubquery()
    {
        var sql = @"
            create function DoTheThing()
            returns int
            begin
                return (
                    select count(*) from (
                        select thing from SomeTable
                    ) sub
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
    public void WithComplexQuery()
    {
        var sql = @"
            CREATE   FUNCTION dothething
            (
                @someparam INT
            )
            RETURNS TABLE
            RETURN
            (
                SELECT * FROM (
                    SELECT *
                    FROM innertable1 alias1
                    CROSS APPLY (
                        SELECT *
                        FROM (
                            SELECT alias1.somecolumn
                            WHERE alias1.somecolumn is not null
                            UNION ALL
                            SELECT COUNT(1)
                            FROM innertable2
                            WHERE alias2.col = alias1.col
                        ) alias2
                        ( somecolumn )
                    ) alias2
                    WHERE EXISTS (
                        SELECT 1
                        FROM innertable3 alias3
                        WHERE alias3.col = alias1.somecolumn
                    )
                    and not EXISTS (
                        SELECT 1
                        FROM innertable3 alias3
                        WHERE alias3.col = alias1.somecolumn
                    )
                ) a
            );
         ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = SqlParser.ParseInternal(sql).Simplify().Stringify();

        results.Should().BeEquivalentTo(new[] {
            new FunctionDefinition(ObjectName.NoSchema("DoTheThing"),
                Dependency.ArrayOf(
                    ObjectName.NoSchema("innertable1").ToTableDependency(),
                    ObjectName.NoSchema("innertable2").ToTableDependency(),
                    ObjectName.NoSchema("innertable3").ToTableDependency()
                )
            ),
        }, reason);
    }

    [Fact]
    public void SubqueryInCaseStatement()
    {
        var sql = @"
            CREATE FUNCTION somefunction ( )
            RETURNS TABLE
            AS RETURN
            (
            SELECT * FROM table1
            where 1 =
                CASE WHEN EXISTS (
                    SELECT 1 FROM othertable
                    )
                    THEN 1
                ELSE 0
            END
            );
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = SqlParser.ParseInternal(sql).Simplify().Stringify();

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("dbo", "somefunction"),
                Dependency.ArrayOf(
                    new ObjectName("dbo", "table1").ToTableDependency(),
                    new ObjectName("dbo", "othertable").ToTableDependency()
                )
            ),
        }, reason);
    }

    [Fact]
    public void SelectFromTableValuedFunction()
    {
        var sql = @"
            create function DoTheThing()
            returns int
            begin
                return (
                    select count(*) from myschema.SomeTableFunction(2, 3)
                )
            end
         ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new FunctionDefinition(ObjectName.NoSchema("DoTheThing"),
                Dependency.ArrayOf(
                    new Dependency(new ObjectName("myschema", "SomeTableFunction"), DependencyKind.Function)
                )
            ),
        });
    }
}
