using System.Collections.Immutable;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlOrder;

internal sealed class SelectStatementHarvester
{
    public ImmutableArray<string> Harvest(string sqlText)
    {
        var parser = new TSql160Parser(true, SqlEngineType.All);
        var domResult = parser.Parse(new StringReader(sqlText), out var errors);

        if (errors?.Any() == true)
        {
            throw new Exception($"parsing error parsing {sqlText}:\n{string.Join("\n", errors.Select(x => x.Message))} ");
        }

        var visitor = new SelectStatementVisitor(sqlText);

        domResult.Accept(visitor);

        return visitor.SelectStatements;
    }

    private class SelectStatementVisitor(string sqlText) : TSqlFragmentVisitor
    {
        public ImmutableArray<string> SelectStatements { get; private set; } = ImmutableArray<string>.Empty;

        public override void Visit(SelectStatement node)
        {
            var sql = sqlText.Substring(node.StartOffset, node.FragmentLength);
            SelectStatements = SelectStatements.Add(sql);

            node.AcceptChildren(this);
        }

        public override void Visit(QuerySpecification node)
        {
            var sql = sqlText.Substring(node.StartOffset, node.FragmentLength);
            SelectStatements = SelectStatements.Add(sql);

            node.AcceptChildren(this);
        }

        public override void Visit(TSqlFragment node)
        {
            node.AcceptChildren(this);
        }

        public override void Visit(QueryExpression node)
        {
            node.AcceptChildren(this);
        }

        public override void Visit(ScalarExpression node)
        {
            node.AcceptChildren(this);
        }
    }
}
