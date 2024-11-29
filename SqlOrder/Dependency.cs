using System.Collections.Immutable;
using SqlOrder.AstTypes;

namespace SqlOrder;

public sealed record Dependency(ObjectName Name, DependencyKind Kind) : IComparable
{
    public static ImmutableArray<Dependency> EmptyArray => ImmutableArray<Dependency>.Empty;

    public static ImmutableArray<Dependency> ArrayOf(params Dependency[] dependencies)
    {
        return ImmutableArray.Create(dependencies);
    }

    public int CompareTo(object? obj)
    {
        if (obj is not Dependency other)
        {
            return -1;
        }

        var nameCmp = Name.CompareTo(other.Name);

        if (nameCmp != 0)
        {
            return nameCmp;
        }

        return Kind.CompareTo(other.Kind);
    }
}
