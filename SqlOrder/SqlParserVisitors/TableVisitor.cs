using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class TableVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateTableStatement statement, ImmutableArray<Statement> context)
    {
        var schema = GetSchemaName(statement.Name);

        var declaration = new TableOrViewDefinition(
            new ObjectName(schema, statement.Name.ObjectName.Value),
            DefaultSchema.GetDependency(schema)
        );

        return context.Add(declaration);
    }
}
