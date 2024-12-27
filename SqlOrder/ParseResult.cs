using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlOrder.AstTypes;

namespace SqlOrder;

public sealed record ParseResult(
    IEnumerable<Statement> Statements,
    TSqlFragment Ast
    );
