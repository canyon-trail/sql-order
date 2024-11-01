using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal sealed class CommonTableExpressionHarvester : ImmutableArrayContextVisitor<ObjectName>
{
    public override ImmutableArray<ObjectName> Visit(SqlQueryWithClause codeObject, ImmutableArray<ObjectName> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<ObjectName> Visit(SqlCommonTableExpression codeObject, ImmutableArray<ObjectName> context)
    {
        return context.Add(ObjectName.FromNullishSchema(null, codeObject.Name.Value));
    }
}
