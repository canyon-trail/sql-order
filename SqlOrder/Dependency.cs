using System.Collections.Immutable;

namespace SqlOrder;

public sealed record Dependency(ObjectName Name, DependencyKind Kind)
{
    public static ImmutableArray<Dependency> EmptyArray => ImmutableArray<Dependency>.Empty;

    public static ImmutableArray<Dependency> ArrayOf(params Dependency[] dependencies)
    {
        return ImmutableArray.Create(dependencies);
    }
}