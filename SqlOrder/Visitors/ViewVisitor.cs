using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal sealed class ViewVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateViewStatement codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlViewDefinition codeObject, ImmutableArray<Statement> context)
    {
        var name = ObjectName.FromNullishSchema(codeObject.Name.SchemaName?.Value, codeObject.Name.ObjectName.Value);

        var ctes = new CommonTableExpressionHarvester().Descend(codeObject.Children);

        var selectStatementResults = new SelectVisitor(ctes).Descend(codeObject.Children);

        var dependencies = selectStatementResults
            .SelectMany(x => x.Dependencies)
            .ToImmutableArray();

        return context.Add(new TableOrViewDefinition(name, dependencies));
    }
}
