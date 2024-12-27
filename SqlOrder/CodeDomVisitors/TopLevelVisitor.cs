using System.Collections.Immutable;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlOrder.AstTypes;

namespace SqlOrder.CodeDomVisitors;

internal sealed class TopLevelVisitor(Action<Statement> onStatement) : TSqlConcreteFragmentVisitor
{
    public override void ExplicitVisit(AlterTableAddTableElementStatement node)
    {
        var deps = new DependencyHarvester().Harvest(node);

        var statement = new Statement(deps);

        AddStatement(statement);
    }

    public override void ExplicitVisit(CreateTriggerStatement node)
    {
        var harvester = new DependencyHarvester();
        var tableName = ObjectName.FromSchemaObjectName(node.TriggerObject.Name).ToTableDependency();
        var dependencies = harvester.Harvest(node);

        var statement = new Statement(dependencies.Insert(0, tableName));

        AddStatement(statement);
    }

    public override void ExplicitVisit(CreateFunctionStatement node)
    {
        var schema = GetSchemaName(node.Name.SchemaIdentifier);

        var functionName = ObjectName.FromSchemaObjectName(node.Name);

        var harvester = new DependencyHarvester();
        var childDependencies = harvester.Harvest(node);

        var declaration = new FunctionDefinition(
            functionName,
            DefaultSchema.GetDependency(schema).AddRange(childDependencies)
        );

        AddStatement(declaration);
    }

    public override void ExplicitVisit(CreateTableStatement node)
    {
        var schema = GetSchemaName(node.SchemaObjectName.SchemaIdentifier);

        var name = ObjectName.FromSchemaObjectName(node.SchemaObjectName);

        if (name.Name.StartsWith("#"))
        {
            // temp table, ignore
            return;
        }

        var declaration = new TableOrViewDefinition(
            name,
            DefaultSchema.GetDependency(schema)
        );

        AddStatement(declaration);
    }

    public override void ExplicitVisit(CreateProcedureStatement node)
    {
        var name = ObjectName.FromSchemaObjectName(node.ProcedureReference.Name);
        var harvester = new DependencyHarvester();

        var dependencies = harvester.Harvest(node)
            // skip temp tables
            .Where(x => !x.Name.Name.StartsWith("#"))
            .ToImmutableArray()
            ;
        var schemaDependency = ObjectName.Schema(name.SchemaName).ToSchemaDependency();

        if (!schemaDependency.Name.Equals(DefaultSchema.ObjectName))
        {
            dependencies = dependencies.Insert(0, schemaDependency);
        }

        var definition = new ProcedureDefinition(
            name,
            dependencies
        );

        AddStatement(definition);
    }

    public override void ExplicitVisit(CreateTypeTableStatement node)
    {
        var name = ObjectName.FromSchemaObjectName(node.Name);

        var harvester = new DependencyHarvester();
        var dependencies = harvester.Harvest(node);

        var definition = new UserDefinedTypeDefinition(
            name,
            DefaultSchema.GetDependency(name.SchemaName)
                .AddRange(dependencies)
        );

        AddStatement(definition);
    }

    public override void ExplicitVisit(CreateTypeUddtStatement node)
    {
        var name = ObjectName.FromSchemaObjectName(node.Name);

        var definition = new UserDefinedTypeDefinition(
            name,
            DefaultSchema.GetDependency(name.SchemaName)
        );

        AddStatement(definition);
    }

    public override void ExplicitVisit(CreateSchemaStatement node)
    {
        var schemaName = ObjectName.Schema(node.Name.Value);

        AddStatement(new SchemaDefinition(schemaName));
    }

    public override void ExplicitVisit(CreateUserStatement node)
    {
        var definition = new UserOrRoleDefinition(new ObjectName(DefaultSchema.DefaultSchemaName, node.Name.Value));

        AddStatement(definition);
    }

    public override void ExplicitVisit(CreateRoleStatement node)
    {
        var definition = new UserOrRoleDefinition(new ObjectName(DefaultSchema.DefaultSchemaName, node.Name.Value));

        AddStatement(definition);
    }

    public override void ExplicitVisit(CreateViewStatement node)
    {
        var name = ObjectName.FromSchemaObjectName(node.SchemaObjectName);

        var dependencies = new DependencyHarvester().Harvest(node);

        AddStatement(new TableOrViewDefinition(name, dependencies));
    }

    public override void ExplicitVisit(CreateSequenceStatement node)
    {
        var name = ObjectName.FromSchemaObjectName(node.Name);

        var definition = new SequenceDefinition(
            name,
            DefaultSchema.GetDependency(name.SchemaName)
        );

        AddStatement(definition);
    }

    public override void ExplicitVisit(SelectStatement node)
    {
        var dependencies = new DependencyHarvester().Harvest(node);

        AddStatement(new Statement(dependencies));
    }

    private void AddStatement(Statement statement) => AddStatements([statement]);

    private void AddStatements(IEnumerable<Statement> statements)
    {
        foreach (var statement in statements)
        {
            onStatement(statement);
        }
    }

    private string GetSchemaName(Identifier? identifier)
    {
        return identifier == null
            ? DefaultSchema.DefaultSchemaName
            : identifier.Value;
    }
}
