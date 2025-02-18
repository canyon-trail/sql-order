﻿using System.Collections.Immutable;

namespace SqlOrder.AstTypes;

public record TableOrViewDefinition(ObjectName Name, ImmutableArray<Dependency> Dependencies)
    : Definition(Name, Dependencies)
{
    public TableOrViewDefinition(ObjectName name) : this(name, Dependency.EmptyArray)
    {
    }

    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.TableOrView);
    }
}