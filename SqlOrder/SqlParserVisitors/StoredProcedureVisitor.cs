using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class StoredProcedureVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateProcedureStatement codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlProcedureDefinition codeObject, ImmutableArray<Statement> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Name);
        var harvester = new DependencyHarvester();

        // we have to harvest twice here since codeObject.Statement is apparently
        // not reachable from SqlProcedureDefinition.Children
        var argDependencies = harvester.Harvest(codeObject);
        var dependencies = harvester
            .Harvest(codeObject.Statement)
            .Where(x => !x.Name.Name.StartsWith("#")) // exclude temp tables
            .ToImmutableArray()
            ;

        var definition = new ProcedureDefinition(
            name,
            new[]
                {
                    ObjectName.Schema(name.SchemaName).ToSchemaDependency()
                }
                .Concat(dependencies)
                .Concat(argDependencies)
                .Where(x => !Builtins.All.Contains(x))
                .Distinct()
                .ToImmutableArray()
        );

        return context.Add(definition);
    }
}
