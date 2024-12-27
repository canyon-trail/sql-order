using SqlOrder.AstTypes;

namespace SqlOrder.Tests;

public static class StringExtensions
{
    public static void AssertParsesTo(this string sql, params Statement[] statements)
    {
        var parser = new SqlParser();

        var results = parser.Parse(sql);

        var reason = results.Ast.Simplify().Stringify();

        results.Statements.Should().BeEquivalentTo(statements, reason);
    }
}
