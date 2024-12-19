using System.Collections.Immutable;
using SharpDag.CSharp;
using SqlOrder.AstTypes;

namespace SqlOrder;

public sealed class SqlDependencyModel
{
    private ImmutableDictionary<string, Script> _scriptsByName;
    private Dag<string, Dependency> _dag;

    public SqlDependencyModel()
        : this(new Dictionary<string, Script>(), Dag<string, Dependency>.Empty)
    {
    }

    internal SqlDependencyModel(
        IReadOnlyDictionary<string, Script> scriptsByName,
        Dag<string, Dependency> dag)
    {
        _scriptsByName = scriptsByName.ToImmutableDictionary();
        _dag = dag;
    }

    public IEnumerable<Script> All =>
        _dag
            .TopologicalSort()
            .Select(x => _scriptsByName[x])
            .ToArray();

    /// <summary>
    /// Gets all relevant scripts (including the one passed in)
    /// in dependency order.
    /// </summary>
    /// <param name="scriptName"></param>
    /// <returns></returns>
    public IEnumerable<Script> DependenciesInOrder(string scriptName)
    {
        var subgraph = _dag;

        // the only sink (i.e. node with out-degree zero) should be
        // the one defined by scriptName. Other sinks need to be removed
        // until scriptName is the only one left.
        while (TryReduceSubgraph()){}

        return subgraph.TopologicalSort().Select(x => _scriptsByName[x]).ToArray();

        // We're "reducing" the subgraph by removing any sinks that aren't scriptName.
        // as soon as the only sink left is scriptName, it can't be reduced further.
        bool TryReduceSubgraph()
        {
            var sinks = subgraph
                .Sinks
                .Where(x => x != scriptName)
                .ToArray();

            if (sinks.Any())
            {
                subgraph = subgraph.RemoveNodes(sinks);
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Adds a dependency to the model so that <see cref="successor"/> depends
    /// on <see cref="predecessor"/>. This causes <see cref="predecessor"/>
    /// to always be ordered earlier than <see cref="successor"/>.
    /// </summary>
    public void AddDependency(Script predecessor, Script successor)
    {
        _scriptsByName = _scriptsByName
            .Add(predecessor.Name, predecessor)
            .Add(successor.Name, successor);

        var edge = new TypedEdge<string, Dependency>(
            src: predecessor.Name,
            dest: successor.Name,
            value: new Dependency(new ObjectName("custom", "custom"), DependencyKind.Custom));

        _dag = _dag.AddEdge(edge);
    }
}
