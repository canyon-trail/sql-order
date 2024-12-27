using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlOrder.CodeDomVisitors;

namespace SqlOrder;
public sealed class SqlParser
{
    public ParseResult Parse(string sql)
    {
        var ast = ParseInternal2(sql);

        var definitions = StatementHarvester.Harvest(ast);

        return new ParseResult(definitions, ast);
    }

    public static TSqlFragment ParseInternal2(string sql)
    {
        var parser = new TSql160Parser(true, SqlEngineType.All);
        var result = parser.Parse(new StringReader(sql), out var errors);

        if (errors != null && errors.Any())
        {
            var errorMessages = errors.Select(x => $"at {x.Line} ({x.Column}): {x.Message}");
            throw new Exception($"Found errors: \n{string.Join("\n\t", errorMessages)}");
        }

        return result;
    }
}
