using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class SequenceParsingTests()
{
    [Fact]
    public void CreateSequence()
    {
        var sql = @"
create sequence [myschema].[myseq]
AS int
START WITH 1
INCREMENT BY 1
MINVALUE -2147483648
MAXVALUE 2147483647
NO CYCLE
CACHE 
GO
";

        sql.AssertParsesTo(
            new SequenceDefinition(
                new ObjectName("myschema", "myseq"),
                Dependency.ArrayOf(
                    ObjectName.Schema("myschema").ToSchemaDependency()
                )
            )
        );
    }

    [Fact]
    public void NoDependencyOnDboSchema()
    {
        var sql = @"
create sequence [dbo].[myseq]
AS int
START WITH 1
INCREMENT BY 1
MINVALUE -2147483648
MAXVALUE 2147483647
NO CYCLE
CACHE 
GO
";

        sql.AssertParsesTo(
            new SequenceDefinition(
                new ObjectName("dbo", "myseq"),
                Dependency.EmptyArray
            )
        );
    }
}
