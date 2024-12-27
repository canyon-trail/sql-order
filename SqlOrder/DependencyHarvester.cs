using System.Collections.Immutable;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlOrder;

public sealed class DependencyHarvester(ImmutableArray<string> cteNames)
{
    public DependencyHarvester() : this([])
    {

    }

    public ImmutableArray<Dependency> Harvest(TSqlFragment node)
    {
        var dependencies = new HashSet<Dependency>();
        var visitor = new HarvestingVisitor(x => dependencies.Add(x), cteNames);

        node.Accept(visitor);

        return [..dependencies];
    }

    public ImmutableArray<Dependency> HarvestChildren(TSqlFragment node)
    {
        var dependencies = new HashSet<Dependency>();
        var visitor = new HarvestingVisitor(x => dependencies.Add(x), cteNames);

        node.AcceptChildren(visitor);

        return [..dependencies];
    }

    private sealed class HarvestingVisitor(Action<Dependency> onDependency, ImmutableArray<string> cteNames) : TSqlConcreteFragmentVisitor
    {
        public override void Visit(AlterTableAddTableElementStatement node)
        {
            var tableName = ObjectName.FromSchemaObjectName(node.SchemaObjectName);

            onDependency(tableName.ToTableDependency());
        }

        public override void Visit(ForeignKeyConstraintDefinition node)
        {
            var tableName = ObjectName.FromSchemaObjectName(node.ReferenceTableName);

            onDependency(tableName.ToTableDependency());
        }

        public override void Visit(DeclareVariableElement node)
        {
            var dataType = ObjectName.FromSchemaObjectName(node.DataType.Name).ToUserDefinedTypeDependency();

            if (Builtins.Types.Contains(dataType.Name))
            {
                return;
            }

            onDependency(dataType);
        }

        public override void Visit(NamedTableReference node)
        {
            if (node.SchemaObject.SchemaIdentifier == null)
            {
                var baseTableName = node.SchemaObject.BaseIdentifier.Value;
                if (cteNames.Contains(baseTableName))
                {
                    return;
                }

                // temp table
                if (baseTableName.StartsWith("#"))
                {
                    return;
                }
            }

            var tableDependency = ObjectName.FromSchemaObjectName(node.SchemaObject).ToTableDependency();

            onDependency(tableDependency);
        }

        public override void Visit(SchemaObjectFunctionTableReference node)
        {
            var name = ObjectName.FromSchemaObjectName(node.SchemaObject);

            onDependency(name.ToFunctionDependency());
        }

        public override void ExplicitVisit(SelectStatement node)
        {
            HandleCtesAndAliases(node, node.WithCtesAndXmlNamespaces);
        }

        public override void ExplicitVisit(InsertStatement node)
        {
            HandleCtesAndAliases(node, node.WithCtesAndXmlNamespaces);
        }

        public override void ExplicitVisit(UpdateStatement node)
        {
            HandleCtesAndAliases(node, node.WithCtesAndXmlNamespaces);
        }

        public override void ExplicitVisit(MergeStatement node)
        {
            HandleCtesAndAliases(node, node.WithCtesAndXmlNamespaces);
        }

        public override void ExplicitVisit(DeleteStatement node)
        {
            HandleCtesAndAliases(node, node.WithCtesAndXmlNamespaces);
        }

        public override void Visit(UserDataTypeReference node)
        {
            var name = ObjectName.FromSchemaObjectName(node.Name);

            onDependency(name.ToUserDefinedTypeDependency());
        }

        public override void ExplicitVisit(SecurityPrincipal node)
        {
            var name = ObjectName.NoSchema(node.Identifier.Value);

            onDependency(name.ToUserOrRoleDependency());
        }

        public override void ExplicitVisit(SecurityTargetObject node)
        {
            var identifiers = node.ObjectName.MultiPartIdentifier.Identifiers;
            var baseName = identifiers.Last();
            var schemaName = identifiers.Count > 1 ? identifiers[-2] : null;

            var name = ObjectName.FromNullishSchema(schemaName?.Value, baseName.Value);

            var dependency = node.ObjectKind switch
            {
                SecurityObjectKind.Schema => ObjectName.Schema(baseName.Value).ToSchemaDependency(),
                _ => null,
            };

            if (dependency != null)
            {
                onDependency(dependency);
            }
        }

        private void HandleCtesAndAliases(TSqlFragment node, WithCtesAndXmlNamespaces withClause)
        {
            var aliases = HarvestAliases(node)
                .Select(x => ObjectName.FromNullishSchema(null, x).ToTableDependency());
            var newCtes = HarvestCtes(withClause);

            var allCtes = cteNames.AddRange(newCtes);
            var childHarvester = new DependencyHarvester(allCtes);
            var dependencies = childHarvester.HarvestChildren(node)
                .Except(aliases);

            foreach (var d in dependencies)
            {
                onDependency(d);
            }
        }

        private static HashSet<string> HarvestCtes(TSqlFragment? node)
        {
            var ctes = new HashSet<string>();
            var cteHarvester = new CteHarvester(x => ctes.Add(x));
            node?.Accept(cteHarvester);
            return ctes;
        }

        private static HashSet<string> HarvestAliases(TSqlFragment? node)
        {
            var aliases = new HashSet<string>();
            var harvester = new AliasHarvester(x => aliases.Add(x));
            node?.Accept(harvester);
            return aliases;
        }
    }

    private sealed class CteHarvester(Action<string> onCte) : TSqlConcreteFragmentVisitor
    {
        public override void Visit(CommonTableExpression node)
        {
            onCte(node.ExpressionName.Value);
        }
    }

    private sealed class AliasHarvester(Action<string> onAlias) : TSqlFragmentVisitor
    {
        public override void Visit(TableReferenceWithAlias node)
        {
            if (node.Alias != null)
            {
                onAlias(node.Alias.Value);
            }
        }
    }
}
