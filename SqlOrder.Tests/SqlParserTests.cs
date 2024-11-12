namespace SqlOrder.Tests;

public sealed class SqlParserTests
{
    [Fact]
    public void SimpleTableDefinition()
    {
        var sql = "create table foo ( id int not null primary key, name text not null );";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[]
        {
            new TableOrViewDefinition(new ObjectName("dbo", "foo"))
        });
    }

    [Fact]
    public void MultipleTableDefinitions()
    {
        var sql = @"
            create table foo ( id int not null primary key, name text not null );
            create table bar ( id int not null primary key, name text not null );
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[]
        {
            new TableOrViewDefinition(new ObjectName("dbo", "foo")),
            new TableOrViewDefinition(new ObjectName("dbo", "bar")),
        });
    }

    [Fact]
    public void TableWithSchema()
    {
        var sql = @"
            create table dbo.foo ( id int not null primary key, name text not null );
            create table custom_schema.bar ( id int not null primary key, name text not null );
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[]
        {
            new TableOrViewDefinition(new ObjectName("dbo", "foo")),
            new TableOrViewDefinition(new ObjectName("custom_schema", "bar"),
                DefaultSchema.GetDependency("custom_schema")),
        });
    }

    [Fact]
    public void Functions()
    {
        var sql = @"
            create function foo() returns int begin return 0 end;
            go;
            create function custom_schema.foo() returns int begin return 0 end;
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[]
        {
            new FunctionDefinition(new ObjectName("dbo", "foo"), Dependency.EmptyArray),
            new FunctionDefinition(
                new ObjectName("custom_schema", "foo"),
                Dependency.ArrayOf(
                    new Dependency(ObjectName.Schema("custom_schema"), DependencyKind.Schema))),
        });
    }

    [Fact]
    public void User()
    {
        var sql = @"
            create user [derp]
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[]
        {
            new UserOrRoleDefinition(new ObjectName("dbo", "derp")),
        });
    }
}
