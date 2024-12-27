using System.Collections.Immutable;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlOrder.AstTypes;

namespace SqlOrder.CodeDomVisitors;

internal static class StatementHarvester
{
    public static ImmutableArray<Statement> Harvest(TSqlFragment node)
    {
        var statements = new List<Statement>();

        var visitor = new TopLevelVisitor(statements.Add);
        node.Accept(visitor);

        return [..statements];
    }

}
