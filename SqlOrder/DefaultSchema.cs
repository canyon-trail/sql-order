using System.Collections.Immutable;

namespace SqlOrder;

public sealed class DefaultSchema
{
    public const string DefaultSchemaName = "dbo";
    public static ObjectName ObjectName { get; } = ObjectName.Schema(DefaultSchemaName);
    /// <summary>
    /// Returns the object name wrapped in an array if it is NOT the default schema
    /// (for now, dbo).
    /// </summary>
    public static ImmutableArray<Dependency> GetDependency(string schemaName)
    {
        if (schemaName == DefaultSchemaName)
        {
            return Dependency.EmptyArray;
        }

        return ImmutableArray.Create(
            new Dependency(ObjectName.Schema(schemaName),
                DependencyKind.Schema
            ));
    }
}
