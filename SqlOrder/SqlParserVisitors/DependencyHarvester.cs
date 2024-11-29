using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;
using SqlOrder.AstTypes;

namespace SqlOrder.SqlParserVisitors;

internal sealed class DependencyHarvester(ImmutableArray<ObjectName> cteNames)
    : ImmutableArrayContextVisitor<Dependency>
{
    public DependencyHarvester() : this(ImmutableArray<ObjectName>.Empty)
    {
    }

    public override ImmutableArray<Dependency> Visit(SqlVariableDeclaration codeObject,
        ImmutableArray<Dependency> context)
    {
        var typeIdentifier = codeObject.Type.DataType.ObjectIdentifier;

        var type = ObjectName.FromSqlObjectIdentifier(typeIdentifier).ToUserDefinedTypeDependency();

        if (Builtins.All.Contains(type))
        {
            return context;
        }

        return context.Add(type);
    }

    public override ImmutableArray<Dependency> Visit(SqlInlineFunctionBodyDefinition codeObject,
        ImmutableArray<Dependency> context)
    {
        return HandleCtes(codeObject, context);
    }

    public override ImmutableArray<Dependency> Visit(SqlTableRefExpression codeObject,
        ImmutableArray<Dependency> context)
    {
        var objectName = ObjectName.FromSqlObjectIdentifier(codeObject.ObjectIdentifier);

        // if this is a reference to a CTE,
        // it's not a dependency on a real table or view:
        // ignore it.
        if (cteNames.Contains(objectName))
        {
            return context;
        }

        var dependency = new Dependency(objectName, DependencyKind.TableOrView);

        return context.Add(dependency);
    }

    public override ImmutableArray<Dependency> Visit(SqlParameterDeclaration codeObject,
        ImmutableArray<Dependency> context)
    {
        var name = ObjectName.FromSqlObjectIdentifier(codeObject.Type.DataType.ObjectIdentifier);

        var typeDependency = name.ToUserDefinedTypeDependency();
        if (Builtins.All.Contains(typeDependency))
        {
            return context;
        }

        return context.Add(typeDependency);
    }

    public override ImmutableArray<Dependency> Visit(SqlSelectStatement codeObject, ImmutableArray<Dependency> context)
    {
        return HandleCtes(codeObject, context);
    }

    public override ImmutableArray<Dependency> Visit(SqlInsertStatement codeObject, ImmutableArray<Dependency> context)
    {
        return HandleCtes(codeObject, context);
    }

    public override ImmutableArray<Dependency> Descend<TChild>(IEnumerable<TChild> children,
        ImmutableArray<Dependency> context)
    {
        var flattened = children.SelectMany(Flatten);

        return base.Descend(flattened, context);
    }

    private IEnumerable<SqlCodeObject> Flatten(SqlCodeObject codeObject)
    {
        if (AutoDescendTypes.Contains(codeObject.GetType()))
        {
            return codeObject.Children
                .SelectMany(Flatten);
        }

        return [codeObject];
    }

    /// <summary>
    /// Harvests CTE names and excludes them from the dependencies
    /// so that they aren't assumed to be actual tables or views.
    /// </summary>
    private ImmutableArray<Dependency> HandleCtes(SqlCodeObject codeObject, ImmutableArray<Dependency> context)
    {
        // visit CTE contents
        var cteVisitor = new CommonTableExpressionHarvester();
        var foundCteNames = cteVisitor.Descend(codeObject.Children);
        if (foundCteNames.Any())
        {
            var childHarvester = new DependencyHarvester(cteNames.AddRange(foundCteNames));

            var childResults = childHarvester.Descend(codeObject.Children);

            return context.AddRange(childResults);
        }

        return Descend(codeObject.Children, context);
    }

    /// <summary>
    /// Since this class does deep searches, many of the overrides would simply
    /// be calls to Descend(...). This helps facilitate eliding that to keep
    /// this class cleaner.
    /// </summary>
    private static readonly IReadOnlyCollection<Type> AutoDescendTypes = new[]
    {
        typeof(SqlBinaryQueryExpression),
        typeof(SqlCommonTableExpression),
        typeof(SqlCompoundStatement),
        typeof(SqlDerivedTableExpression),
        typeof(SqlExistsBooleanExpression),
        typeof(SqlFromClause),
        typeof(SqlInlineTableRelationalFunctionDefinition),
        typeof(SqlInsertSpecification),
        typeof(SqlMultistatementFunctionBodyDefinition),
        typeof(SqlQualifiedJoinTableExpression),
        typeof(SqlQuerySpecification),
        typeof(SqlQueryWithClause),
        typeof(SqlReturnStatement),
        typeof(SqlScalarRelationalFunctionDefinition),
        typeof(SqlScalarSubQueryExpression),
        typeof(SqlSelectSpecification),
        typeof(SqlSelectSpecificationInsertSource),
        typeof(SqlUnqualifiedJoinTableExpression),
        typeof(SqlVariableDeclareStatement),
        typeof(SqlWhereClause),
    }.ToImmutableHashSet();
}
