using SharpDag.CSharp;

namespace SqlOrder;

public sealed class SqlDependencyModel
{
    private readonly IReadOnlyDictionary<string, Script> _scriptsByName;
    private readonly Dag<string, Dependency> _dag;

    internal SqlDependencyModel(
        IReadOnlyDictionary<string, Script> scriptsByName,
        Dag<string, Dependency> dag)
    {
        _scriptsByName = scriptsByName;
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

        var sinks = GetNonMatchingSinks();
        while (sinks.Any())
        {
            subgraph = subgraph.RemoveNodes(sinks);
            sinks = GetNonMatchingSinks();
        }

        return subgraph.TopologicalSort().Select(x => _scriptsByName[x]).ToArray();

        string[] GetNonMatchingSinks()
        {
            return subgraph
                .Sinks
                .Where(x => x != scriptName)
                .ToArray();
        }
    }
}
