using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using Snapshooter.Xunit;
using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class StoredProcParsingTests()
{
    [Fact]
    public void ScalarReturn()
    {
        var sql = @"
create procedure [myschema].[myproc]
as
begin
    return 1
end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("myschema", "myproc"),
                Dependency.ArrayOf(
                    ObjectName.Schema("myschema").ToSchemaDependency()
                )
            ),
        });
    }

    [Fact]
    public void SimpleSelect()
    {
        var sql = @"
create procedure [myschema].[myproc]
as
begin
    select * from myschema.sometable;
end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(new ObjectName("myschema", "myproc"),
                Dependency.ArrayOf(
                    ObjectName.Schema("myschema").ToSchemaDependency(),
                    new Dependency(new ObjectName("myschema", "sometable"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void SelectIntoTempTable()
    {
        var sql = @"
create procedure [myschema].[myproc]
as
begin
    create table #sometable (
        foo int not null,
        bar text null
    );

    insert into #sometable
    select * from myschema.sometable;

    select * from #sometable;
end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(new ObjectName("myschema", "myproc"),
                Dependency.ArrayOf(
                    ObjectName.Schema("myschema").ToSchemaDependency(),
                    new Dependency(new ObjectName("myschema", "sometable"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void SimpleInsert()
    {
        var sql = @"
create procedure [myschema].[myproc]
as
begin
    set nocount on;

    insert into myschema.sometable (c1, c2)
    values (1, 2);
end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(new ObjectName("myschema", "myproc"),
                Dependency.ArrayOf(
                    ObjectName.Schema("myschema").ToSchemaDependency(),
                    new Dependency(new ObjectName("myschema", "sometable"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void InsertWithCte()
    {
        var sql = @"
create procedure [myschema].[myproc]
as
begin
    set nocount on;

    with ignoreme as (select * from myschema.othertable)
    insert into myschema.sometable (c1, c2)
    select a, b from ignoreme;
end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(new ObjectName("myschema", "myproc"),
                Dependency.ArrayOf(
                    ObjectName.Schema("myschema").ToSchemaDependency(),
                    new Dependency(new ObjectName("myschema", "sometable"), DependencyKind.TableOrView),
                    new Dependency(new ObjectName("myschema", "othertable"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void ParameterTypeDependency()
    {
        var sql = @"
create procedure [myschema].[myproc]
    @arg sometype
as
begin
    return 1
end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("myschema", "myproc"),
                Dependency.ArrayOf(
                    ObjectName.Schema("myschema").ToSchemaDependency(),
                    new Dependency(new ObjectName("dbo", "sometype"), DependencyKind.UserDefinedType)
                )
            ),
        });
    }

    [Fact]
    public void VariableTypeDependency()
    {
        var sql = @"
CREATE   proc myproc
as
begin
	set nocount on;

	begin try

		begin tran;

		declare @sometablevar as myschema.sometabletype;

        insert into @sometablevar select * from sometable;

		commit tran;

	end try
	begin catch

		while @@TRANCOUNT > 0
		rollback tran;

		throw;

	end catch;

end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = SqlParser.ParseInternal(sql).Simplify().Stringify();

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("dbo", "myproc"),
                Dependency.ArrayOf(
                    new ObjectName("myschema", "sometabletype").ToUserDefinedTypeDependency(),
                    new ObjectName("dbo", "sometable").ToTableDependency()
                )
            ),
        }, reason);
    }

    [Fact]
    public void UpdateFromAlias()
    {
        var sql = @"
CREATE   proc myproc
as
begin

    update x set foo = 1
    from table1 x;

end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = SqlParser.ParseInternal(sql).Simplify().Stringify();

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("dbo", "myproc"),
                Dependency.ArrayOf(
                    new ObjectName("dbo", "table1").ToTableDependency()
                )
            ),
        }, reason);
    }

    [Fact]
    public void UpdateFromAliasWithJoin()
    {
        var sql = @"
CREATE   proc myproc
as
begin

    update x set foo = 1
    from table1 x
	inner join table2 on x.a = table2.a
	;

end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = SqlParser.ParseInternal(sql).Simplify().Stringify();

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("dbo", "myproc"),
                Dependency.ArrayOf(
                    new ObjectName("dbo", "table1").ToTableDependency(),
                    new ObjectName("dbo", "table2").ToTableDependency()
                )
            ),
        }, reason);
    }

    [Fact]
    public void UpdateFromRightAliasWithJoin()
    {
        var sql = @"
CREATE   proc myproc
as
begin

	update	x
	set		c1 = 1
	from	table1 a
			inner join table2 x on a.c = x.c
	;

end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = SqlParser.ParseInternal(sql).Simplify().Stringify();

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("dbo", "myproc"),
                Dependency.ArrayOf(
                    new ObjectName("dbo", "table1").ToTableDependency(),
                    new ObjectName("dbo", "table2").ToTableDependency()
                )
            ),
        }, reason);
    }

    [Fact]
    public void MergeWithCte()
    {
        var sql = @"
CREATE   proc myproc
as
begin

	with _x as (select * from table1)
	merge into table2
	using (select * from _x) src
	on src.c = table2.c
	when matched then update set c2 = src.c2;
	

end;
";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = SqlParser.ParseInternal(sql).Simplify().Stringify();

        results.Should().BeEquivalentTo(new[] {
            new ProcedureDefinition(
                new ObjectName("dbo", "myproc"),
                Dependency.ArrayOf(
                    new ObjectName("dbo", "table1").ToTableDependency(),
                    new ObjectName("dbo", "table2").ToTableDependency()
                )
            ),
        }, reason);
    }
}
