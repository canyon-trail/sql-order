using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class SelectVisitor(ImmutableArray<ObjectName> cteNames) : StatementHarvestingVisitor
{
    public SelectVisitor(): this(ImmutableArray<ObjectName>.Empty)
    {
    }

    public override ImmutableArray<Statement> Visit(SqlSelectStatement codeObject, ImmutableArray<Statement> context)
    {
        var dependencies = new DependencyHarvester().Descend(codeObject);
        return context.Add(new Statement(dependencies));
    }

    public override ImmutableArray<Statement> Visit(SqlTableRefExpression codeObject, ImmutableArray<Statement> context)
    {
        var objectName = ObjectName.FromNullishSchema(codeObject.ObjectIdentifier.SchemaName.Value, codeObject.ObjectIdentifier.ObjectName.Value);

        // if this is a reference to a CTE,
        // it's not a dependency on a real table or view:
        // ignore it.
        if (cteNames.Contains(objectName))
        {
            return context;
        }
        var dependency = new Dependency(objectName, DependencyKind.TableOrView);

        return context.Add(new Statement(Dependency.ArrayOf(dependency)));
    }

    public override ImmutableArray<Statement> Visit(SqlQualifiedJoinTableExpression codeObject, ImmutableArray<Statement> context)
    {
        var dependencies = new DependencyHarvester(cteNames).Descend(codeObject.Children);

        var newStatement = new Statement(dependencies);

        return context.Add(newStatement);
    }

    public override ImmutableArray<Statement> Visit(SqlSelectSpecification codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlQueryWithClause codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlCommonTableExpression codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlBinaryQueryExpression codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlQuerySpecification codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }

    public override ImmutableArray<Statement> Visit(SqlFromClause codeObject, ImmutableArray<Statement> context)
    {
        return Descend(codeObject.Children, context);
    }
}
