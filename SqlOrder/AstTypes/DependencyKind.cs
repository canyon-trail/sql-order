namespace SqlOrder.AstTypes;

public enum DependencyKind
{
    TableOrView,
    Function,
    Schema,
    Procedure,
    UserOrRole,
    UserDefinedType,
    Sequence
}