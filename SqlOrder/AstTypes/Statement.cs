using System.Collections.Immutable;

namespace SqlOrder.AstTypes;

public record Statement(ImmutableArray<Dependency> Dependencies);