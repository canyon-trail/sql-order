using Snapshooter.Xunit;
using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class UserDefinedTypeParsingTests
{
    [Fact]
    public void SimpleScalarType()
    {
        var sql = @"
            create type derp from varchar(5) not null;
        ";

        sql.AssertParsesTo(
            new UserDefinedTypeDefinition(
                new ObjectName("dbo", "derp"),
                []
            )
        );
    }

    [Fact]
    public void SimpleTableType()
    {
        var sql = @"
            create type derp as table (
                thingId int not null,
                name text not null
            );
        ";

        sql.AssertParsesTo(
            new UserDefinedTypeDefinition(
                new ObjectName("dbo", "derp"),
                []
            )
        );
    }

    [Fact]
    public void TableWithCustomTypeColumn()
    {
        var sql = @"
            create type derp as table (
                thingId int not null,
                name text not null,
                otherthing varchar(3) null,
                dep otherschema.deptype null
            );
        ";

        sql.AssertParsesTo(
            new UserDefinedTypeDefinition(
                new ObjectName("dbo", "derp"),
                [
                    new Dependency(new ObjectName("otherschema", "deptype"), DependencyKind.UserDefinedType)
                ]
            )
        );
    }
}
