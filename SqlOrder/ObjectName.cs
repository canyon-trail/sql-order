using System.Collections.Immutable;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlOrder.AstTypes;

namespace SqlOrder;

/// <summary>
/// Represents the schema-qualified name of a schema object.
/// </summary>
/// <param name="SchemaName"></param>
/// <param name="Name"></param>
public sealed class ObjectName : IComparable, IEquatable<ObjectName?>
{
    public string SchemaName { get; }
    public string Name { get; }
    public static ImmutableArray<ObjectName> EmptyArray = ImmutableArray<ObjectName>.Empty;

    public static ImmutableArray<ObjectName> ArrayOf(params ObjectName[] names)
    {
        return ImmutableArray.Create(names);
    }

    public ObjectName(string schemaName, string name)
    {
        SchemaName = schemaName.ToLower();
        Name = name.ToLower();
    }

    public static ObjectName Schema(string name)
    {
        return new ObjectName(name, name);
    }

    public static ObjectName NoSchema(string name)
    {
        return new ObjectName(DefaultSchema.DefaultSchemaName, name);
    }

    public static ObjectName FromSchemaObjectName(SchemaObjectName typeName)
    {
        return FromNullishSchema(typeName.SchemaIdentifier?.Value, typeName.BaseIdentifier.Value);
    }

    public static ObjectName FromNullishSchema(string? schema, string name)
    {
        schema ??= DefaultSchema.DefaultSchemaName;
        return new ObjectName(schema, name);
    }

    public override string ToString()
    {
        return $"[{SchemaName}].[{Name}]";
    }

    public int CompareTo(object? obj)
    {
        if (obj is not ObjectName otherName)
        {
            return -1;
        }

        var schemaNameComparison = SchemaName.CompareTo(otherName.SchemaName);
        if (schemaNameComparison == 0)
        {
            return Name.CompareTo(otherName.Name);
        }

        return schemaNameComparison;
    }

    public bool Equals(ObjectName? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return SchemaName == other.SchemaName && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ObjectName other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SchemaName, Name);
    }

    public Dependency ToSchemaDependency()
    {
        return ToDependency(DependencyKind.Schema);
    }

    public Dependency ToTableDependency()
    {
        return ToDependency(DependencyKind.TableOrView);
    }

    public Dependency ToUserDefinedTypeDependency()
    {
        return ToDependency(DependencyKind.UserDefinedType);
    }

    public Dependency ToFunctionDependency()
    {
        return ToDependency(DependencyKind.Function);
    }

    private Dependency ToDependency(DependencyKind kind)
    {
        return new Dependency(this, kind);
    }
}
