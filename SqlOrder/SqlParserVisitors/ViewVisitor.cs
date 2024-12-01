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

        var cteNames = new CommonTableExpressionHarvester().Descend(codeObject.Children);

        var dependencies = new DependencyHarvester(cteNames).Harvest(codeObject);

        return context.Add(new TableOrViewDefinition(name, dependencies));
    }
}
