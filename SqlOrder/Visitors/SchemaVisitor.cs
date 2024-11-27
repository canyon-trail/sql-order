using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.Visitors;

internal sealed class SchemaVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateSchemaStatement createSchemaStatement, ImmutableArray<Statement> context)
    {
        var schemaName = ObjectName.Schema(createSchemaStatement.Name.Value);

        return context.Add(new SchemaDefinition(schemaName));
    }
}
