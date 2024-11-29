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

var folderTxt = string.Join(", ", options.Folders);
var excludeTxt = string.Join(", ", options.IgnorePaths);

Console.WriteLine($"Ordering files at {folderTxt} excluding {excludeTxt}...");

var files = options.Folders
    .SelectMany(
        x => matcher.GetResultsInFullPath(x)
            .Select(Path.GetFullPath)
    )
    // this excludes items where the exclude pattern includes parent folders. The
    // Match function doesn't work on rooted paths, so we have to strip the root
    .Where(x => matcher.Match(Path.GetRelativePath(Path.GetPathRoot(x), x)).HasMatches)
    .Distinct()
    .OrderBy(x => x)
    .Select(x => new FileScript(x))
    .ToArray()
    ;

Console.WriteLine($"Found {files.Length} files...");

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
    var leadingLines = new[]
    {
        "use [sqlorder]",
        "GO",
        "SET QUOTED_IDENTIFIER ON",
        "GO",
    };
    var header = string.Join(Environment.NewLine, leadingLines);

    foreach (var file in scripts)
    {
        Console.WriteLine($"Executing script at {file.Name}...");

        var sqlScript = $"{header}{Environment.NewLine}{await file.GetScriptText(token)}";

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
