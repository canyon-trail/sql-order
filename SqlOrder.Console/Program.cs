using CommandLine;
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

await using var container = new MsSqlBuilder()
    // per issue at https://github.com/testcontainers/testcontainers-dotnet/issues/1271
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .WithLogger(NullLogger.Instance)
    .Build();

Console.WriteLine("starting container...");
await container.StartAsync();
Console.WriteLine("\tstarted.");

foreach (var file in orderedFiles)
{
    Console.WriteLine($"Executing script at {file.Name}...");
    var result = await container.ExecScriptAsync(await file.GetScriptText(cts.Token), cts.Token);

    if (result.ExitCode != 0)
    {
        Console.WriteLine($"\tScript failed:");
        Console.WriteLine(result.Stdout);
        Console.WriteLine(result.Stderr);
        break;
    }
}
