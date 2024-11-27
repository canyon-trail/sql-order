using CommandLine;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging.Abstractions;
using SqlOrder;
using SqlOrder.Console;
using Testcontainers.MsSql;

var argsResult = Parser.Default.ParseArguments<Options>(args);

if (argsResult.Tag != ParserResultType.Parsed)
{
    foreach (var err in argsResult.Errors)
    {
        Console.WriteLine(err);
    }
    return;
}

var options = argsResult.Value;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (o, e) => cts.Cancel();

var matcher = new Matcher();
matcher.AddInclude("**/*.sql");
matcher.AddExcludePatterns(options.IgnorePaths);

Console.WriteLine($"Ordering files at {options.Folder} excluding {string.Join(", ", options.IgnorePaths)}...");

var files = matcher.GetResultsInFullPath(options.Folder)
        .Select(x => new FileScript(x))
        .ToArray()
    ;

Console.WriteLine($"Found {files.Length} files at {options.Folder}...");

var orderedFiles = await new ScriptOrderer().OrderScripts(files, cts.Token);

await using var sqlContainer = new MsSqlBuilder()
    // using custom image that has full-text-search installed
    .WithImage("canyontrail/sqlorder-sqlserver")
    .WithLogger(NullLogger.Instance)
    .Build();

Console.WriteLine("starting container...");
await sqlContainer.StartAsync();
Console.WriteLine("\tstarted.");

await CreateSqlOrderDatabase(sqlContainer, cts.Token);

await RunScripts(sqlContainer, orderedFiles, cts.Token);

async Task CreateSqlOrderDatabase(MsSqlContainer container, CancellationToken cancellationToken)
{
    var connectionString = container.GetConnectionString();
    await using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    await using var sqlCommand = new SqlCommand("create database sqlorder;", connection);
    await sqlCommand.ExecuteNonQueryAsync(cancellationToken);
}

async Task RunScripts(MsSqlContainer container, IEnumerable<Script> scripts, CancellationToken token)
{
    foreach (var file in scripts)
    {
        Console.WriteLine($"Executing script at {file.Name}...");

        var sqlScript = $"use [sqlorder]; {Environment.NewLine}GO{Environment.NewLine}{await file.GetScriptText(token)}";

        var result = await container.ExecScriptAsync(sqlScript, token);

        if (result.ExitCode != 0)
        {
            Console.WriteLine($"\tError running {file.Name}:");
            Console.WriteLine(result.Stdout);
            Console.WriteLine(result.Stderr);
            return;
        }
    }
}
