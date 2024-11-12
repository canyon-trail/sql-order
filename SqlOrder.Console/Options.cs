﻿using CommandLine;

namespace SqlOrder.Console;

public sealed class Options
{
    [Option('f', nameof(Folder), Required = true,
        HelpText = "Path to the folder(s) containing sql files to put into order.")]
    public string Folder { get; set; } = null!;

    [Option('i', nameof(IgnorePaths), Separator = ',', HelpText = "Paths to ignore")]
    public IEnumerable<string> IgnorePaths { get; set; } = null!;
}