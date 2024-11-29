using System.Collections.Immutable;
using SqlOrder.AstTypes;

namespace SqlOrder;

public sealed class Builtins
{
    private static readonly Lazy<HashSet<Dependency>> lzDependencies = new (CreateDependencies);
    public static readonly IEnumerable<ObjectName> Tables = [
        new ObjectName("dbo", "spt_values"),
        new ObjectName("dbo", "inserted"),
        new ObjectName("dbo", "updated"),
        new ObjectName("dbo", "deleted"),
        new ObjectName("sys", "extended_properties"),
        new ObjectName("sys", "key_constraints"),
        new ObjectName("sys", "objects"),
        new ObjectName("sys", "indexes"),
        new ObjectName("sys", "schemas"),
        new ObjectName("sys", "types"),
        new ObjectName("sys", "columns"),
    ];

    public static readonly IEnumerable<ObjectName> Users = [
        new ObjectName("dbo", "guest"),
        new ObjectName("dbo", "db_ddlviewer"),
    ];

    public static readonly ImmutableHashSet<ObjectName> Types = new[]
    {
        new ObjectName("dbo", "int"),
        new ObjectName("dbo", "bigint"),
        new ObjectName("dbo", "smallint"),
        new ObjectName("dbo", "tinyint"),
        new ObjectName("dbo", "float"),
        new ObjectName("dbo", "decimal"),
        new ObjectName("dbo", "numeric"),
        new ObjectName("dbo", "uniqueidentifier"),
        new ObjectName("dbo", "text"),
        new ObjectName("dbo", "ntext"),
        new ObjectName("dbo", "char"),
        new ObjectName("dbo", "nchar"),
        new ObjectName("dbo", "varchar"),
        new ObjectName("dbo", "varbinary"),
        new ObjectName("dbo", "nvarchar"),
        new ObjectName("dbo", "bit"),
        new ObjectName("dbo", "date"),
        new ObjectName("dbo", "time"),
        new ObjectName("dbo", "datetime"),
        new ObjectName("dbo", "datetime2"),
        new ObjectName("dbo", "datetimeoffset"),
        new ObjectName("dbo", "sysname"),
        new ObjectName("dbo", "xml"),
        new ObjectName("dbo", "sql_variant"),
    }.ToImmutableHashSet();

    public static IReadOnlyCollection<Dependency> All => lzDependencies.Value;

    private static HashSet<Dependency> CreateDependencies()
    {
        var tables = Tables.Select(x => new Dependency(x, DependencyKind.TableOrView));
        var users = Users.Select(x => new Dependency(x, DependencyKind.UserOrRole));
        var types = Types.Select(x => new Dependency(x, DependencyKind.UserDefinedType));

        return [
            ..tables,
            ..users,
            ..types,
            ObjectName.Schema(DefaultSchema.DefaultSchemaName).ToSchemaDependency(),
        ];
    }
}
