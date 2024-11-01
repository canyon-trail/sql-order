namespace SqlOrder.Tests;

public sealed class SelectStatementParsingTests
{
    [Fact]
    public void SimpleSelect()
    {
        var sql = @"
            select * from Table1
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new Statement(
                Dependency.ArrayOf(
                    new Dependency(new ObjectName("dbo", "Table1"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void SimpleSelectWithSchemaAndAlias()
    {
        var sql = @"
            select * from someschema.Table1 t1
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new Statement(
                Dependency.ArrayOf(
                    new Dependency(new ObjectName("someschema", "Table1"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void TwoTableJoin()
    {
        var sql = @"
            select * from someschema.Table1 t1
            inner join table2 on t2.a = t1.b
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new Statement(
                Dependency.ArrayOf(
                    new Dependency(new ObjectName("someschema", "Table1"), DependencyKind.TableOrView),
                    new Dependency(ObjectName.NoSchema("table2"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void ThreeTableJoin()
    {
        var sql = @"
            select * from someschema.Table1 t1
            inner join table2 on t2.a = t1.b
            inner join table3 on t3.a = t1.b
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new Statement(
                Dependency.ArrayOf(
                    new Dependency(new ObjectName("someschema", "Table1"), DependencyKind.TableOrView),
                    new Dependency(ObjectName.NoSchema("table2"), DependencyKind.TableOrView),
                    new Dependency(ObjectName.NoSchema("table3"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void CteNameNotReported()
    {
        var sql = @"
            with somecte as (select * from table2)
            select * from someschema.Table1 t1
            inner join somecte t2 on t2.a = t1.b
            inner join table3 on t3.a = t1.b
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new Statement(
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("table2"), DependencyKind.TableOrView),
                    new Dependency(new ObjectName("someschema", "Table1"), DependencyKind.TableOrView),
                    new Dependency(ObjectName.NoSchema("table3"), DependencyKind.TableOrView)
                )
            ),
        });
    }

    [Fact]
    public void CteChainReferences()
    {
        var sql = @"
            with
                somecte1 as (select * from table2),
                somecte2 as (select * from somecte1)
            select * from someschema.Table1 t1
            inner join somecte2 t2 on t2.a = t1.b
            inner join table3 on t3.a = t1.b
        ";

        var parser = new SqlParser();

        var results = parser.Parse(sql);

        results.Should().BeEquivalentTo(new[] {
            new Statement(
                Dependency.ArrayOf(
                    new Dependency(ObjectName.NoSchema("table2"), DependencyKind.TableOrView),
                    new Dependency(new ObjectName("someschema", "Table1"), DependencyKind.TableOrView),
                    new Dependency(ObjectName.NoSchema("table3"), DependencyKind.TableOrView)
                )
            ),
        });
    }
}
