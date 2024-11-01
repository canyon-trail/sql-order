using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal class AlterRoleVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlBatch codeObject, ImmutableArray<Statement> context)
    {
        var firstThreeTokens = codeObject.Tokens.Take(3)
            .Select(x => x.Text.ToLower());

        // this visitor is full of jank
        if (string.Join("", firstThreeTokens) == "alter role")
        {
            var username = codeObject.Tokens
                .Last(x => x.Type == "TOKEN_ID")
                .Text.Trim('[', ']');

            return context.Add(new Statement(
                Dependency.ArrayOf(
                    new Dependency(
                        ObjectName.FromNullishSchema(null, username),
                        DependencyKind.User
                        ))));
        }

        return base.Visit(codeObject, context);
    }
}
