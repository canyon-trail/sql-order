using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal sealed class FunctionVisitor : StatementHarvestingVisitor
{
    public override ImmutableArray<Statement> Visit(SqlCreateFunctionStatement statement, ImmutableArray<Statement> context)
    {
        var schema = GetSchemaName(statement.Definition.Name);

        var functionName = new ObjectName(schema, statement.Definition.Name.ObjectName.Value);

        var childDependencies = Descend(statement.Children, ImmutableArray<Statement>.Empty)
            .SelectMany(x => x.Dependencies)
            .Distinct()
            ;

        var declaration = new FunctionDefinition(
            functionName,
            DefaultSchema.GetDependency(schema).AddRange(childDependencies)
        );

        return context.Add(declaration);
    }

    public override ImmutableArray<Statement> Visit(SqlInlineTableRelationalFunctionDefinition codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlInlineFunctionBodyDefinition codeObject, ImmutableArray<Statement> context)
    {
        var cteNames = new CommonTableExpressionHarvester().Descend(codeObject.Children);

        return DeferToSelectVisitor(codeObject, context, cteNames);
    }

    public override ImmutableArray<Statement> Visit(SqlScalarRelationalFunctionDefinition codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlMultistatementFunctionBodyDefinition codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlCompoundStatement codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlReturnStatement codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlScalarSubQueryExpression codeObject, ImmutableArray<Statement> context)
    {
        return DeferToSelectVisitor(codeObject, context, ImmutableArray<ObjectName>.Empty);
    }

    private static ImmutableArray<Statement> DeferToSelectVisitor(SqlCodeObject codeObject,
        ImmutableArray<Statement> context, ImmutableArray<ObjectName> cteNames)
    {
        var visitor = new SelectVisitor(cteNames);

        return visitor.Descend(codeObject.Children, context);
    }
}
