using System.Collections.Immutable;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlOrder.AstTypes;

namespace SqlOrder.CodeDomVisitors;

internal sealed class AlterTableVisitor : TSqlFragmentVisitor
{
    public ImmutableArray<Statement> Statements { get; private set; } = ImmutableArray<Statement>.Empty;

    public override void Visit(ForeignKeyConstraintDefinition node)
    {
        var tableRef = node.ReferenceTableName;
        var tableName = ObjectName.FromNullishSchema(
            tableRef.SchemaIdentifier?.Value,
            tableRef.BaseIdentifier.Value);

        var statement = new Statement(tableName.ToTableDependency());

        Statements = Statements.Add(statement);
    }
}
