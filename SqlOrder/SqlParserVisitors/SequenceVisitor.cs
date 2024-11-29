using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.Parser;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class SequenceVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlStatement codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlBatch codeObject, ImmutableArray<Statement> context)
    {
        // janky parsing code because there isn't an AST object for create sequence statements

        if (codeObject.TokenManager.Count < 8)
        {
            return context;
        }

        var relevantTokenTypes = new [] {
            TokenTypes.Create,
            TokenTypes.Sequence,
            TokenTypes.Id,
            TokenTypes.Dot,
        };

        var relevantTokens = codeObject.Tokens
            .Where(x => relevantTokenTypes.Contains(x.Type))
            .Take(5)
            .ToArray();

        if (relevantTokens.Length < 5)
        {
            return context;
        }

        var expectedSequence = new[]
        {
            TokenTypes.Create,
            TokenTypes.Sequence,
            TokenTypes.Id,
            TokenTypes.Dot,
            TokenTypes.Id,
        };

        if (!relevantTokens.Select(x => x.Type).SequenceEqual(expectedSequence))
        {
            return context;
        }

        var schemaName = relevantTokens[2].Text.Trim('[', ']', '"');
        var name = relevantTokens[4].Text.Trim('[', ']', '"');

        var dependencies =
            schemaName.Equals(DefaultSchema.DefaultSchemaName, StringComparison.InvariantCultureIgnoreCase)
                ? Dependency.EmptyArray
                : Dependency.ArrayOf(
                    ObjectName.Schema(schemaName).ToSchemaDependency()
                );

        var definition = new SequenceDefinition(
            new ObjectName(schemaName, name),
            dependencies
        );

        return context.Add(definition);
    }

    private static class TokenTypes
    {
        public const string Create = nameof(Tokens.TOKEN_CREATE);
        public const string Sequence = nameof(Tokens.TOKEN_s_CDA_SEQUENCE);
        public const string Id =  nameof(Tokens.TOKEN_ID);
        public const string Dot = ".";
    }
}
