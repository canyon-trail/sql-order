using System.Collections.Immutable;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlOrder.Tests;

public static class TSqlFragmentExtensions
{
    public static SimplifiedSqlObject Simplify(this TSqlFragment node)
    {
        var children = new List<SimplifiedSqlObject>();
        var visitor = new SimplifyingVisitor(children.Add);
        node.AcceptChildren(visitor);

        return new SimplifiedSqlObject(node.GetType().Name, [..children]);
    }

    private sealed class SimplifyingVisitor(Action<SimplifiedSqlObject> onChild, HashSet<TSqlFragment> seen) : TSqlFragmentVisitor
    {
        public SimplifyingVisitor(Action<SimplifiedSqlObject> onChild) : this(onChild, new())
        {
        }

        public override void Visit(TSqlFragment node)
        {
            if (!seen.Add(node))
            {
                return;
            }

            var children = new List<SimplifiedSqlObject>();
            var childVisitor = new SimplifyingVisitor(children.Add, seen);
            node.AcceptChildren(childVisitor);

            onChild(new SimplifiedSqlObject(
                node.GetType().Name,
                [..children]
            ));
        }
    }
}
