using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class SqlParserTests
{
    [Fact]
    public void SimpleTableDefinition()
    {
        var sql = "create table foo ( id int not null primary key, name text not null );";

        sql.AssertParsesTo(
            new TableOrViewDefinition(new ObjectName("dbo", "foo"))
        );
    }

    [Fact]
    public void MultipleTableDefinitions()
    {
        var sql = @"
            create table foo ( id int not null primary key, name text not null );
            create table bar ( id int not null primary key, name text not null );
        ";

        sql.AssertParsesTo(
            new TableOrViewDefinition(new ObjectName("dbo", "foo")),
            new TableOrViewDefinition(new ObjectName("dbo", "bar"))
        );
    }

    [Fact]
    public void TableWithSchema()
    {
        var sql = @"
            create table dbo.foo ( id int not null primary key, name text not null );
            create table custom_schema.bar ( id int not null primary key, name text not null );
        ";

        sql.AssertParsesTo(
            new TableOrViewDefinition(new ObjectName("dbo", "foo")),
            new TableOrViewDefinition(
                new ObjectName("custom_schema", "bar"),
                DefaultSchema.GetDependency("custom_schema")
            )
        );
    }

    [Fact]
    public void User()
    {
        var sql = @"
            create user [derp]
        ";

        sql.AssertParsesTo(
            new UserOrRoleDefinition(new ObjectName("dbo", "derp"))
        );
    }
}
