using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class UserDefinedTypeVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateUserDefinedDataTypeStatement codeObject, ImmutableArray<Statement> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Name);

        return context.Add(new UserDefinedTypeDefinition(
            name,
            Dependency.ArrayOf(
                ObjectName.Schema(name.SchemaName).ToSchemaDependency()
            )
        ));
    }

    public override ImmutableArray<Statement> Visit(SqlCreateUserDefinedTableTypeStatement codeObject, ImmutableArray<Statement> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Name);

        var dependencies = new TableDependencyVisitor().Descend(codeObject.Children)
            .Add(ObjectName.Schema(name.SchemaName).ToSchemaDependency());

        return context.Add(new UserDefinedTypeDefinition(name, dependencies));
    }
}
