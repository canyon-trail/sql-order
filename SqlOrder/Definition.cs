using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;

namespace SqlOrder;

public enum DependencyKind
{
    TableOrView,
    Function,
    Schema,
    Procedure,
    User,
}

public record Statement(ImmutableArray<Dependency> Dependencies);

/// <summary>
/// Represents the definition of some schema object, like a schema, table, function, or stored procedure.
/// </summary>
/// <param name="Name"></param>
/// <param name="Dependencies"></param>
public abstract record Definition(ObjectName Name, ImmutableArray<Dependency> Dependencies) : Statement(Dependencies)
{
    public static ImmutableArray<Definition> EmptyArray = ImmutableArray<Definition>.Empty;

    public static ImmutableArray<Definition> ArrayOf(params Definition[] definitions)
    {
        return ImmutableArray.Create(definitions);
    }

    public abstract Dependency ToDependency();
}

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

public record FunctionDefinition(ObjectName Name, ImmutableArray<Dependency> Dependencies)
    : Definition(Name, Dependencies)
{
    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.Function);
    }
}

public record ProcedureDefinition(ObjectName Name, ImmutableArray<Dependency> Dependencies)
    : Definition(Name, Dependencies)
{
    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.Procedure);
    }
}

public record UserDefinition(ObjectName Name)
    : Definition(Name, Dependency.EmptyArray)
{
    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.User);
    }
}
