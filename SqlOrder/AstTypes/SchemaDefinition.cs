using System.Collections.Immutable;

namespace SqlOrder.AstTypes;

public record SchemaDefinition(ObjectName Name, ImmutableArray<Dependency> Dependencies)
    : Definition(Name, Dependencies)
{
    public SchemaDefinition(ObjectName name) : this(name, Dependency.EmptyArray)
    {
    }

    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.Schema);
    }
}