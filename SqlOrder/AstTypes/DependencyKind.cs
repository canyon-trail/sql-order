namespace SqlOrder.AstTypes;

public enum DependencyKind
{
    TableOrView,
    Function,
    Schema,
    Procedure,
    UserOrRole,
    UserDefinedType,
    Sequence,
    /// <summary>
    /// Represents a custom-added dependency to force ordering to happen
    /// in a particular way.
    /// </summary>
    Custom,
}
