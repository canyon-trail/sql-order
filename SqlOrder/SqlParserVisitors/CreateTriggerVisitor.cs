using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class CreateTriggerVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateTriggerStatement codeObject, ImmutableArray<Statement> context)
    {
        var harvester = new DependencyHarvester();

        var tableName = codeObject.Children
            .OfType<SqlDmlTriggerDefinition>()
            .Select(x => x.TargetName)
            .Select(ObjectName.FromSqlObjectIdentifier)
            .SingleOrDefault();

        var dependencies = harvester.Harvest(codeObject);
        if (tableName != null)
        {
            dependencies = dependencies.Add(tableName.ToTableDependency());
        }

        return context.Add(new Statement(dependencies));
    }
}
