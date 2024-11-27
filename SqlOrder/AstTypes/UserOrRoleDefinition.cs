namespace SqlOrder.AstTypes;

public record UserOrRoleDefinition(ObjectName Name)
    : Definition(Name, Dependency.EmptyArray)
{
    public override Dependency ToDependency()
    {
        return new Dependency(Name, DependencyKind.UserOrRole);
    }
}
