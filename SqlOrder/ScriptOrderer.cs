using SharpDag.CSharp;
using SqlOrder.AstTypes;

namespace SqlOrder;

public sealed class ScriptOrderer
{
    public async Task<IEnumerable<Script>> OrderScripts(ICollection<Script> scripts, CancellationToken ct)
    {
        var scriptsByName = scripts.ToDictionary(x => x.Name);

        var parser = new SqlParser();

        var objectNameToDefinedInScript = new Dictionary<Dependency, string>();
        var allStatements = new List<DefinitionDependenciesContext>();

        var dag = Dag<string, ObjectName>.Empty;

        foreach (var script in scripts)
        {
            dag = dag.AddNode(script.Name);
            var statements = await ParseScript(script, parser, ct);

            foreach (var definition in statements.OfType<Definition>())
            {
                objectNameToDefinedInScript[definition.ToDependency()] = script.Name;
            }

            allStatements.AddRange(statements.Select(x => new DefinitionDependenciesContext(script.Name, x.Dependencies, objectNameToDefinedInScript)));
        }

        foreach (var definition in allStatements)
        {
            dag = definition.AddEdgesToDag(dag);
        }

        var ordering = dag.TopologicalSort();

        return ordering.Select(x => scriptsByName[x]).ToArray();
    }

    private async Task<ICollection<Statement>> ParseScript(Script script, SqlParser parser, CancellationToken ct)
    {
        try
        {
            var definitions = parser.Parse(await script.GetScriptText(ct));

            return definitions.ToArray();
        }
        catch (Exception ex)
        {
            throw new Exception($"Problem parsing {script.Name}", ex);
        }
    }

    private sealed class DefinitionDependenciesContext(string scriptName, IEnumerable<Dependency> dependencies, Dictionary<Dependency, string> objectToDefinedInScript)
    {
        public Dag<string, ObjectName> AddEdgesToDag(Dag<string, ObjectName> dag)
        {
            foreach (var dependency in dependencies)
            {
                if (Builtins.All.Contains(dependency))
                {
                    continue;
                }

                var definedInScript = FindScriptName(dependency);

                // notice that the direction of the edge points
                // from definition to usage.
                var edge = new TypedEdge<string, ObjectName>(
                    dest: scriptName,
                    src: definedInScript,
                    value: dependency.Name
                    );
                try
                {
                    dag = dag.AddEdge(edge);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Problem adding edge from {scriptName} to {edge.Dest} for {dependency.Kind} {dependency.Name}", ex);
                }
            }

            return dag;
        }

        private string FindScriptName(Dependency dependency)
        {
            if (!objectToDefinedInScript.TryGetValue(dependency, out var definedInScript))
            {
                throw new Exception($"Unable to find definition of\n{dependency.Kind} {dependency.Name}\n\treferenced in {scriptName}");
            }

            return definedInScript;
        }
    }
}
