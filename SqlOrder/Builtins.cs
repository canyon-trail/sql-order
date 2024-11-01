namespace SqlOrder;

public sealed class Builtins
{
    private static readonly Lazy<HashSet<Dependency>> lzDependencies = new (CreateDependencies);
    public static readonly IEnumerable<ObjectName> Tables = [
        new ObjectName("dbo", "spt_values"),
        new ObjectName("sys", "extended_properties"),
        new ObjectName("sys", "key_constraints"),
        new ObjectName("sys", "objects"),
        new ObjectName("sys", "indexes"),
        new ObjectName("sys", "schemas"),
        new ObjectName("sys", "types"),
    ];

    public static IReadOnlyCollection<Dependency> All => lzDependencies.Value;

    private static HashSet<Dependency> CreateDependencies()
    {
        var tables = Tables.Select(x => new Dependency(x, DependencyKind.TableOrView));

        return [..tables];
    }
}
