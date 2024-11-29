using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class ViewVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateViewStatement codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlViewDefinition codeObject, ImmutableArray<Statement> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Name);

        var ctes = new CommonTableExpressionHarvester().Descend(codeObject.Children);

        var selectStatementResults = new SelectVisitor(ctes).Descend(codeObject.Children);

        var dependencies = selectStatementResults
            .SelectMany(x => x.Dependencies)
            .ToImmutableArray();

        return context.Add(new TableOrViewDefinition(name, dependencies));
    }
}
