using System.Collections.Immutable;

namespace SqlOrder.AstTypes;

public sealed record UserDefinedTypeDefinition(ObjectName Name, ImmutableArray<Dependency> Dependencies)
    : Definition(Name, Dependencies)
{
    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.UserDefinedType);
    }
}