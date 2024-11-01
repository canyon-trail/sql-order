using System.Collections.Immutable;
using Microsoft.SqlServer.Management.SqlParser.SqlCodeDom;

namespace SqlOrder.Visitors;

internal abstract class ImmutableArrayContextVisitor<TContext> : SqlCodeObjectContextVisitor<ImmutableArray<TContext>, ImmutableArray<TContext>>
{
    public ImmutableArray<TContext> Descend<TChild>(IEnumerable<TChild> children) where TChild : SqlCodeObject
    {
        return Descend(children, ImmutableArray<TContext>.Empty);
    }

    public ImmutableArray<TContext> Descend<TChild>(
        IEnumerable<TChild> children,
        ImmutableArray<TContext> context)
        where TChild : SqlCodeObject
    {
        var results = children
                .Aggregate(context, (ctx, child) => Accept(child, ctx))
            ;

        return results;
    }

    private ImmutableArray<TContext> Accept<TChild>(TChild child, ImmutableArray<TContext> context)
        where TChild : SqlCodeObject
    {
        var result = child.Accept(this, context);

        if (result.IsDefault)
        {
            return context;
        }

        return result;
    }
}
