using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;
using SqlOrder.Visitors;

namespace SqlOrder;
public sealed class SqlParser
{
    public IEnumerable<Statement> Parse(string sql)
    {
        var resultAst = ParseInternal(sql);

        var definitions = StatementHarvestingVisitor.All.Aggregate(
            ImmutableArray<Statement>.Empty,
            (defs, visitor) => resultAst.Accept(visitor, defs)
            );

        return definitions;
    }

    public static SqlScript ParseInternal(string sql)
    {
        var resultAst = Microsoft.SqlServer.Management.SqlParser.Parser.Parser.Parse(sql);

        if (resultAst.Errors.Any())
        {
            throw new InvalidOperationException($"parse error: {string.Join(", ", resultAst.Errors.Select(x => x.Message))}");
        }

        return resultAst.Script;
    }
}
