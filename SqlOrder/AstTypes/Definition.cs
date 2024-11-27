using System.Collections.Immutable;

namespace SqlOrder.AstTypes;

/// <summary>
/// Represents the definition of some schema object, like a schema, table, function, or stored procedure.
/// </summary>
/// <param name="Name"></param>
/// <param name="Dependencies"></param>
public abstract record Definition(ObjectName Name, ImmutableArray<Dependency> Dependencies) : Statement(Dependencies)
{
    public static ImmutableArray<Definition> EmptyArray = ImmutableArray<Definition>.Empty;

    public static ImmutableArray<Definition> ArrayOf(params Definition[] definitions)
    {
        return ImmutableArray.Create(definitions);
    }

    public abstract Dependency ToDependency();
}