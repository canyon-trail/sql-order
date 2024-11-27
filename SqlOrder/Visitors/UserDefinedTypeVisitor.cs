using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.Visitors;

internal sealed class UserDefinedTypeVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateUserDefinedDataTypeStatement codeObject, ImmutableArray<Statement> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Name);

        return context.Add(new UserDefinedTypeDefinition(name, Dependency.EmptyArray));
    }

    public override ImmutableArray<Statement> Visit(SqlCreateUserDefinedTableTypeStatement codeObject, ImmutableArray<Statement> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Name);

        var dependencies = new TableDependencyVisitor().Descend(codeObject.Children);

        return context.Add(new UserDefinedTypeDefinition(name, dependencies));
    }
}
