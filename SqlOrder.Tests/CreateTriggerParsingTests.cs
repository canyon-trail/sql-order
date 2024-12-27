using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public sealed class CreateTriggerParsingTests
{
    [Fact]
    public void CustomUdtVariable()
    {
        var sql = @"
create   trigger [myschema].[mytrigger]
on [myschema].[sometable]
after insert, update, delete
as
begin
	declare @somevar myschema.sometype

    insert into othertable (a,b) values (1, 2);
end
        ";

        sql.AssertParsesTo(
            new Statement(
                Dependency.ArrayOf(
                    new ObjectName("myschema", "sometable").ToTableDependency(),
                    new ObjectName("myschema", "sometype").ToUserDefinedTypeDependency(),
                    new ObjectName("dbo", "othertable").ToTableDependency()
                )
            )
        );
    }
}
