﻿using System.Collections.Immutable;

namespace SqlOrder.AstTypes;

public record FunctionDefinition(ObjectName Name, ImmutableArray<Dependency> Dependencies)
    : Definition(Name, Dependencies)
{
    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.Function);
    }
}