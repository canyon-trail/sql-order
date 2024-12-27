using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class GrantParsingTests()
{
    [Fact]
    public void GrantToSchema()
    {
        var sql = @"
GRANT CONTROL ON SCHEMA::derp TO some_role
 ";

        sql.AssertParsesTo(
            new Statement(
                DefaultSchema.GetDependency("derp")
                    .Add(ObjectName.NoSchema("some_role").ToUserOrRoleDependency()
                )
            )
        );
    }
}
