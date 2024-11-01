using Snapshooter.Xunit;

namespace SqlOrder.Tests;

public sealed class AstSnapshots
{
    [Fact(Skip = "serialization issues")]
    public void SchemaWithGrant()
    {
        var script = @"
CREATE SCHEMA [Example]
AUTHORIZATION [dbo]
GO
GRANT CONTROL ON SCHEMA:: [Example] TO [ExampleUser]
GO
";
        var ast = SqlParser.ParseInternal(script);

        Snapshot.Match(ast);
    }
}
