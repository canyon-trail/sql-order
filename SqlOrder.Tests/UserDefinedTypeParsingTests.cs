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

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new UserDefinedTypeDefinition(
                new ObjectName("dbo", "derp"),
                Dependency.ArrayOf(
                    ObjectName.Schema("dbo").ToSchemaDependency()
                )
            ),
        });
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

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new UserDefinedTypeDefinition(
                new ObjectName("dbo", "derp"),
                Dependency.ArrayOf(
                    ObjectName.Schema("dbo").ToSchemaDependency()
                )
            ),
        });
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

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new UserDefinedTypeDefinition(
                new ObjectName("dbo", "derp"),
                Dependency.ArrayOf(
                    ObjectName.Schema("dbo").ToSchemaDependency(),
                    new Dependency(new ObjectName("otherschema", "deptype"), DependencyKind.UserDefinedType)
                )
            ),
        });
    }
}
