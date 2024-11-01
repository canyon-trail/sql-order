using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal sealed class GrantVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlGrantStatement codeObject, ImmutableArray<Statement> context)
    {
        // this is janky because the SqlGrantStatement class doesn't represent which token is the user id
        var userToken = codeObject.Tokens.Where(x => x.Type == "TOKEN_ID").Last();

        var objectName = new ObjectName(
                DefaultSchema.DefaultSchemaName,
                userToken.Text.Trim('[', ']'));

        var statement = new Statement(
            Dependency.ArrayOf(
                new Dependency(objectName, DependencyKind.User)));

        return context.Add(statement);
    }
}
