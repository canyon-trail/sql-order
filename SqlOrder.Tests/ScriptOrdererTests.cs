namespace SqlOrder.Tests;

public sealed class ScriptOrdererTests
{
    [Fact]
    public async Task OrdersSchemaBeforeFunction()
    {
        var func = new StringScript(
            "func",
            @"create function custom_schema.foo() returns int begin return 0 end;"
            );

        var schema = new StringScript("schema", "create schema custom_schema authorization dbo;");

        var testee = new ScriptOrderer();

        var result = await testee.OrderScripts([func, schema], CancellationToken.None);

        result.Should().BeEquivalentTo([schema, func], x => x.WithStrictOrdering());
    }

    [Fact]
    public async Task OrdersUserBeforeGrant()
    {
        var grant = new StringScript(
            "grant",
            @"grant all on [exampleschema] to [exampleuser];"
            );

        var schema = new StringScript(
            "schema",
            "create schema exampleschema authorization dbo;"
            );
        var user = new StringScript(
            "user",
            "create user [exampleuser];"
            );

        var testee = new ScriptOrderer();

        var result = await testee.OrderScripts([grant, schema, user], CancellationToken.None);

        result.Should().BeEquivalentTo([schema, user, grant], x => x.WithStrictOrdering());
    }

    [Fact]
    public async Task OrdersUserBeforeRoleAssignment()
    {
        var alter = new StringScript(
            "alter",
            @"alter role [somerole] add member [exampleuser];"
            );

        var user = new StringScript(
            "user",
            "create user [exampleuser];"
            );

        var testee = new ScriptOrderer();

        var result = await testee.OrderScripts([alter, user], CancellationToken.None);

        result.Should().BeEquivalentTo([user, alter], x => x.WithStrictOrdering());
    }
}
