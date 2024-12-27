using System.Collections.Immutable;

namespace SqlOrder.Tests;

public sealed record SimplifiedSqlObject(string Name, ImmutableArray<SimplifiedSqlObject> Children)
{
    public string Stringify(int depth = 0)
    {
        var children = Children.Select(x => x.Stringify(depth + 1));
        var pad = "".PadLeft(depth, ' ');
        var name = $"{pad}{Name}";
        return string.Join("\n", new[] { name }.Concat(children));
    }
}