using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class FunctionVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateFunctionStatement statement, ImmutableArray<Statement> context)
    {
        var schema = GetSchemaName(statement.Definition.Name);

        var functionName = new ObjectName(schema, statement.Definition.Name.ObjectName.Value);

        var harvester = new DependencyHarvester();
        var childDependencies = harvester.Harvest(statement);

        var declaration = new FunctionDefinition(
            functionName,
            DefaultSchema.GetDependency(schema).AddRange(childDependencies)
        );

        return context.Add(declaration);
    }
}
