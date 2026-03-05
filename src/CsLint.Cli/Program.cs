using System.CommandLine;
using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Reporting;
using Cslint.Core.Rules;

var pathArgument = new Argument<string>("path")
{
    Description = "Path to a C# file or directory to lint",
};

var formatOption = new Option<string>("--format")
{
    Description = "Output format: text, json, or sarif",
    DefaultValueFactory = _ => "text",
};

var severityOption = new Option<string>("--severity")
{
    Description = "Minimum severity to report: info, warning, or error",
    DefaultValueFactory = _ => "info",
};

var excludeOption = new Option<string[]>("--exclude")
{
    Description = "Glob patterns to exclude (e.g., **/Generated/*.cs)",
};

var rootCommand = new RootCommand("Cslint - Fast C# linter respecting .editorconfig")
{
    pathArgument,
    formatOption,
    severityOption,
    excludeOption,
};

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    string path = parseResult.GetValue(pathArgument)!;
    string format = parseResult.GetValue(formatOption)!;
    string severity = parseResult.GetValue(severityOption)!;
    string[]? excludePatterns = parseResult.GetValue(excludeOption);

    RuleRegistry registry = RuleRegistry.CreateDefault();
    var configProvider = new EditorConfigProvider();
    var fileLinter = new FileLinter(registry, configProvider);

    IOutputFormatter formatter = format.ToLowerInvariant() switch
    {
        "json" => new JsonFormatter(),
        "sarif" => new SarifFormatter(),
        _ => new TextFormatter(),
    };

    LintSeverity minSeverity = severity.ToLowerInvariant() switch
    {
        "warning" => LintSeverity.Warning,
        "error" => LintSeverity.Error,
        _ => LintSeverity.Info,
    };

    string fullPath = Path.GetFullPath(path);
    IReadOnlyList<LintDiagnostic> diagnostics;

    if (File.Exists(fullPath))
    {
        diagnostics = fileLinter.LintFile(fullPath);
    }
    else if (Directory.Exists(fullPath))
    {
        var directoryLinter = new DirectoryLinter(fileLinter);
        diagnostics = await directoryLinter.LintDirectoryAsync(fullPath, excludePatterns, cancellationToken);
    }
    else
    {
        Console.Error.WriteLine($"Path not found: {fullPath}");
        return 2;
    }

    // Filter by severity
    if (minSeverity > LintSeverity.Info)
    {
        diagnostics = diagnostics.Where(d => d.Severity >= minSeverity).ToList();
    }

    string output = formatter.Format(diagnostics);

    if (!string.IsNullOrEmpty(output))
    {
        Console.Write(output);
    }

    return diagnostics.Count > 0 ? 1 : 0;
});

ParseResult result = rootCommand.Parse(args);
return await result.InvokeAsync();
