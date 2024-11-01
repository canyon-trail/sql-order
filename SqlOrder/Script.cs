namespace SqlOrder;

public abstract class Script
{
    internal Script()
    {
    }
    public abstract Task<string> GetScriptText(CancellationToken ct);

    public abstract string Name { get; }
}

public sealed class FileScript : Script
{
    public FileScript(string path)
    {
        Path = path;
    }

    public string Path { get; init; }

    public override Task<string> GetScriptText(CancellationToken ct)
    {
        return File.ReadAllTextAsync(Path, ct);
    }

    public override string Name => Path;
}

public sealed class StreamScript : Script
{
    private readonly Stream _stream;

    public StreamScript(string name, Stream stream)
    {
        Name = name;
        _stream = stream;
    }

    public override async Task<string> GetScriptText(CancellationToken ct)
    {
        using var reader = new StreamReader(_stream);

        return await reader.ReadToEndAsync(ct);
    }

    public override string Name { get; }
}

public sealed class StringScript : Script
{
    private readonly string _script;

    public StringScript(string name, string script)
    {
        _script = script;
        Name = name;
    }
    public override Task<string> GetScriptText(CancellationToken ct)
    {
        return Task.FromResult(_script);
    }

    public override string Name { get; }
}
