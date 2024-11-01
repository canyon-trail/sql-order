using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal abstract class StatementHarvestingVisitor : ImmutableArrayContextVisitor<Statement>
{
    public static readonly ImmutableArray<StatementHarvestingVisitor> All =
        typeof(SqlParser).Assembly
            .GetTypes()
            .Where(x => !x.IsAbstract)
            .Where(x => x.IsAssignableTo(typeof(StatementHarvestingVisitor)))
            .Select(Activator.CreateInstance)
            .Cast<StatementHarvestingVisitor>()
            .ToImmutableArray();
    public override ImmutableArray<Statement> Visit(SqlScript codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlBatch codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    protected string GetSchemaName(SqlObjectIdentifier identifier)
    {
        return identifier.IsMultiPartName
            ? identifier.SchemaName.Value
            : DefaultSchema.DefaultSchemaName;
    }
}
