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
}
