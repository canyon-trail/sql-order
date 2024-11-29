using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class UserOrRoleVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateUserStatement codeObject, ImmutableArray<Statement> context)
    {
        var definition = new UserOrRoleDefinition(new ObjectName(DefaultSchema.DefaultSchemaName, codeObject.Name.Value));

        return context.Add(definition);
    }

    public override ImmutableArray<Statement> Visit(SqlCreateUserWithImplicitAuthenticationStatement codeObject, ImmutableArray<Statement> context)
    {
        var definition = new UserOrRoleDefinition(new ObjectName(DefaultSchema.DefaultSchemaName, codeObject.Name.Value.Trim('\'')));

        return context.Add(definition);
    }

    public override ImmutableArray<Statement> Visit(SqlCreateRoleStatement codeObject, ImmutableArray<Statement> context)
    {
        var definition = new UserOrRoleDefinition(new ObjectName(DefaultSchema.DefaultSchemaName, codeObject.Name.Value));

        return context.Add(definition);
    }
}
