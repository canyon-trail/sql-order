using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;
using SqlOrder.SqlParserVisitors;

namespace SqlOrder;

/// <summary>
/// Depth-first searches through an AST for dependencies. Does not use built-in
/// visitor pattern since it doesn't support a depth-first search and we'd have
/// to override basically EVERY method.
/// </summary>
/// <param name="cteNames">CTE names that should be ignored</param>
internal sealed class DependencyHarvester(ImmutableArray<ObjectName> cteNames)
{
    /// <summary>
    /// Indicates whether the algorithm should keep descending to child nodes
    /// or stop. In some cases we use a child harvester, so descending after
    /// doing so would produce incorrect results.
    /// </summary>
    private enum DescendOption
    {
        Descend,
        Stop
    }
    public DependencyHarvester() : this(ImmutableArray<ObjectName>.Empty)
    {
    }

    public ImmutableArray<Dependency> Harvest(SqlCodeObject codeObject)
    {
        return Harvest([codeObject], Dependency.EmptyArray).Distinct().ToImmutableArray();
    }

    private ImmutableArray<Dependency> Harvest(IEnumerable<SqlCodeObject> children, ImmutableArray<Dependency> context)
    {
        foreach (var child in children)
        {
            // uses dynamic dispatch to select the correct overload of HarvestSingle(...).
            (context, var descend) = ((ImmutableArray<Dependency>, DescendOption))HarvestSingle((dynamic) child, context);

            if (descend == DescendOption.Descend)
            {
                context = Harvest(child.Children, context);
            }
        }

        return context;
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlCodeObject codeObject, ImmutableArray<Dependency> context)
    {
        return (context, DescendOption.Descend);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlVariableDeclaration codeObject, ImmutableArray<Dependency> context)
    {
        var typeIdentifier = codeObject.Type.DataType.ObjectIdentifier;

        var type = ObjectName.FromSqlObjectIdentifier(typeIdentifier).ToUserDefinedTypeDependency();

        if (Builtins.All.Contains(type))
        {
            return (context, DescendOption.Descend);
        }

        return (context.Add(type), DescendOption.Descend);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlInlineFunctionBodyDefinition codeObject, ImmutableArray<Dependency> context)
    {
        return HandleCtes(codeObject, context);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlTableRefExpression codeObject, ImmutableArray<Dependency> context)
    {
        var objectName = ObjectName.FromSqlObjectIdentifier(codeObject.ObjectIdentifier);

        // if this is a reference to a CTE,
        // it's not a dependency on a real table or view:
        // ignore it.
        if (cteNames.Contains(objectName))
        {
            return (context, DescendOption.Descend);
        }

        var dependency = new Dependency(objectName, DependencyKind.TableOrView);

        return (context.Add(dependency), DescendOption.Descend);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlTableValuedFunctionRefExpression codeObject, ImmutableArray<Dependency> context)
    {
        var objectName = ObjectName.FromSqlObjectIdentifier(codeObject.ObjectIdentifier);

        var dependency = new Dependency(objectName, DependencyKind.Function);

        if (Builtins.All.Contains(dependency))
        {
            return (context, DescendOption.Descend);
        }

        return (context.Add(dependency), DescendOption.Descend);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlParameterDeclaration codeObject, ImmutableArray<Dependency> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Type.DataType.ObjectIdentifier);

        var typeDependency = name.ToUserDefinedTypeDependency();
        if (Builtins.All.Contains(typeDependency))
        {
            return (context, DescendOption.Descend);
        }

        return (context.Add(typeDependency), DescendOption.Descend);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlMergeStatement codeObject, ImmutableArray<Dependency> context)
    {
        return HandleCtes(codeObject, context);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlSelectStatement codeObject, ImmutableArray<Dependency> context)
    {
        return HandleCtes(codeObject, context);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlInsertStatement codeObject, ImmutableArray<Dependency> context)
    {
        return HandleCtes(codeObject, context);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlDeleteStatement codeObject, ImmutableArray<Dependency> context)
    {
        return HandleUpdateAndDelete(codeObject, codeObject.DeleteSpecification.FromClause, context);
    }

    private (ImmutableArray<Dependency>, DescendOption) HarvestSingle(SqlUpdateStatement codeObject, ImmutableArray<Dependency> context)
    {
        return HandleUpdateAndDelete(codeObject, codeObject.UpdateSpecification.FromClause, context);
    }

    /// <summary>
    /// Harvests CTE names and excludes them from the dependencies
    /// so that they aren't assumed to be actual tables or views.
    /// </summary>
    private (ImmutableArray<Dependency>, DescendOption) HandleCtes(SqlCodeObject codeObject, ImmutableArray<Dependency> context)
    {
        // visit CTE contents
        var cteVisitor = new CommonTableExpressionHarvester();
        var foundCteNames = cteVisitor.Descend(codeObject.Children);
        if (foundCteNames.Any())
        {
            var childHarvester = new DependencyHarvester(cteNames.AddRange(foundCteNames));

            // note that we're calling Harvest on Children here; otherwise we'd get infinite recursion.
            var childResults = childHarvester.Harvest(codeObject.Children, context);

            // we need to stop descending since we use a child harvester to descend from here
            return (context.AddRange(childResults), DescendOption.Stop);
        }

        return (context, DescendOption.Descend);
    }

    private IEnumerable<SqlCodeObject> DepthFirstTraverse(SqlCodeObject codeObject)
    {
        foreach (var child in codeObject.Children)
        {
            yield return child;
            foreach (var descendant in DepthFirstTraverse(child))
            {
                yield return descendant;
            }
        }
    }

    private (ImmutableArray<Dependency>, DescendOption) HandleUpdateAndDelete(
        SqlCodeObject codeObject,
        SqlFromClause? fromClause,
        ImmutableArray<Dependency> context
    )
    {
        var otherCteNames = new CommonTableExpressionHarvester().Descend(codeObject.Children);

        var aliases = GetTableAliases(fromClause);

        var childHarvester = new DependencyHarvester(cteNames.Concat(otherCteNames).ToImmutableArray());
        var childDependencies = childHarvester
            .Harvest(codeObject.Children, Dependency.EmptyArray)
            .Except(aliases)
            ;

        return (context.AddRange(childDependencies), DescendOption.Stop);
    }

    private ImmutableArray<Dependency> GetTableAliases(SqlFromClause? fromClause)
    {
        if (fromClause == null)
        {
            return Dependency.EmptyArray;
        }

        var tableRefAliases = DepthFirstTraverse(fromClause)
            .OfType<SqlTableRefExpression>()
            .Where(x => x.Alias != null)
            .Select(x => x.Alias.Value)
            .ToImmutableArray();

        var tableVarAliases = DepthFirstTraverse(fromClause)
            .OfType<SqlTableVariableRefExpression>()
            .Where(x => x.Alias != null)
            .Select(x => x.Alias.Value)
            .ToImmutableArray();

        return tableRefAliases
            .Concat(tableVarAliases)
            .Select(x => new ObjectName("dbo", x).ToTableDependency())
            .ToImmutableArray();
    }
}
