using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Tests;

public static class SqlCodeObjectExtensions
{
    public static SimplifiedSqlObject Simplify(this SqlCodeObject astNode)
    {
        return new SimplifiedSqlObject(
            astNode.GetType().Name,
            astNode.Children.Select(x => x.Simplify()).ToImmutableArray()
        );
    }
}

public sealed record SimplifiedSqlObject(string Name, ImmutableArray<SimplifiedSqlObject> Children);
