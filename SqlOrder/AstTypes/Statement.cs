using System.Collections.Immutable;

namespace SqlOrder.AstTypes;

public record Statement(ImmutableArray<Dependency> Dependencies)
{
    public Statement(params Dependency[] dependencies): this(dependencies.ToImmutableArray())
    {

    }

}
