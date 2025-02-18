﻿namespace SqlOrder.Tests;

public sealed class ScriptOrdererTests
{
    [Fact]
    public async Task OrdersSchemaBeforeFunction()
    {
        var func = new StringScript(
            "func",
            @"create function custom_schema.foo() returns int begin return 0 end;"
            );

        var schema = new StringScript("schema", "create schema custom_schema authorization dbo;");

        var testee = new ScriptOrderer();

        var result = await testee.OrderScripts([func, schema], CancellationToken.None);

        result.Should().BeEquivalentTo([schema, func], x => x.WithStrictOrdering());
    }

    [Fact]
    public async Task OrdersTableDataTypeBeforeProc()
    {
        var proc = new StringScript(
            "proc",
            @"create procedure dothething(@args as tabletype readonly)
                as
                begin
                    select * from @args;
                end;
                "
            );

        var dataType = new StringScript("datatype", "create type tabletype as table (id int not null)");

        var testee = new ScriptOrderer();

        var result = await testee.OrderScripts([proc, dataType], CancellationToken.None);

        result.Should().BeEquivalentTo([dataType, proc], x => x.WithStrictOrdering());
    }

    [Fact]
    public async Task RemovesIrrelevantScripts()
    {
        var procA = new StringScript(
            "procA",
            @"create procedure procA
                as
                begin
                    select * from tableA;
                end;
                "
            );
        var procB = new StringScript(
            "procB",
            @"create procedure procB
                as
                begin
                    select * from tableB;
                end;
                "
            );

        var tableA = new StringScript("tableA", "create table tableA (id int not null)");
        var tableB = new StringScript("tableB", "create table tableB (id int not null)");

        var testee = new ScriptOrderer();

        var model = await testee.BuildModel([procA, procB, tableA, tableB], CancellationToken.None);

        model.DependenciesInOrder("procA").Should().BeEquivalentTo([tableA, procA]);
    }
}
