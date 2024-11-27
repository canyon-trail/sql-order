using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.Visitors;

/// <summary>
/// Harvests dependencies from table definitions
/// </summary>
internal sealed class TableDependencyVisitor : ImmutableArrayContextVisitor<Dependency>
{
    public override ImmutableArray<Dependency> Visit(SqlTableDefinition codeObject, ImmutableArray<Dependency> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Dependency> Visit(SqlColumnDefinition codeObject, ImmutableArray<Dependency> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.DataType.DataType.ObjectIdentifier);

        if (Builtins.Types.Contains(name))
        {
            return context;
        }

        return context.Add(new Dependency(name, DependencyKind.UserDefinedType));
    }
}