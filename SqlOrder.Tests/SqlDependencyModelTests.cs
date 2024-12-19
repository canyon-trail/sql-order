namespace SqlOrder.Tests;

public sealed class SqlDependencyModelTests
{
    [Fact]
    public void CustomEdges()
    {
        var testee = new SqlDependencyModel();

        var script1 = new StringScript("script1.sql", "");
        var script2 = new StringScript("script2.sql", "");
        var script3 = new StringScript("script3.sql", "");
        testee.AddDependency(script1, script2);
        testee.AddDependency(script1, script3);
        testee.AddDependency(script2, script3);

        testee.All
            .Should()
            .BeEquivalentTo([
                script1,
                script2,
                script3,
            ],
            x => x.WithStrictOrdering());
    }
}
