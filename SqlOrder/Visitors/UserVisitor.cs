using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal sealed class UserVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateUserStatement codeObject, ImmutableArray<Statement> context)
    {
        var definition = new UserDefinition(new ObjectName(DefaultSchema.DefaultSchemaName, codeObject.Name.Value));

        return context.Add(definition);
    }

    public override ImmutableArray<Statement> Visit(SqlCreateUserWithImplicitAuthenticationStatement codeObject, ImmutableArray<Statement> context)
    {
        var definition = new UserDefinition(new ObjectName(DefaultSchema.DefaultSchemaName, codeObject.Name.Value.Trim('\'')));

        return context.Add(definition);
    }
}
