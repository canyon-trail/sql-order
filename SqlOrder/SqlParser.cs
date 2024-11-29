using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlOrder.AstTypes;
using SqlOrder.CodeDomVisitors;
using SqlOrder.SqlParserVisitors;

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

        var codeDomParser = new TSql160Parser(true, SqlEngineType.All);
        var result = codeDomParser.Parse(new StringReader(sql), out var errors);

        if (errors != null && errors.Any())
        {
            var errorMessages = errors.Select(x => $"at {x.Line} ({x.Column}): {x.Message}");
            throw new Exception($"Found errors: \n{string.Join("\n\t", errorMessages)}");
        }

        var visitor = new AlterTableVisitor();
        result.Accept(visitor);
        definitions = definitions.AddRange(visitor.Statements);

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
