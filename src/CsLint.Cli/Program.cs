using System.CommandLine;
using System.Text.Json;
using System.Text;
using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Reporting;
using Cslint.Core.Rules;

var pathArgument = new Argument<string>("path")
{
    Description = "Path to a C# file or directory to lint",
    DefaultValueFactory = _ => ".",
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

var listRulesOption = new Option<bool>("--list-rules")
{
    Description = "List all available rules and exit",
};

var showConfigOption = new Option<bool>("--show-config")
{
    Description = "Show resolved .editorconfig settings for the given path and exit",
};

var semanticOption = new Option<bool>("--semantic")
{
    Description = "Enable semantic analysis for advanced rules",
};

var rootCommand = new RootCommand("Cslint - Fast C# linter respecting .editorconfig")
{
    pathArgument,
    formatOption,
    severityOption,
    excludeOption,
    listRulesOption,
    showConfigOption,
    semanticOption,
};

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    bool listRules = parseResult.GetValue(listRulesOption);

    if (listRules)
    {
        return PrintRuleList();
    }

    bool showConfig = parseResult.GetValue(showConfigOption);

    if (showConfig)
    {
        string configPath = Path.GetFullPath(parseResult.GetValue(pathArgument)!);

        // editorconfig resolves by file extension, so use a dummy .cs file for directories
        if (Directory.Exists(configPath))
        {
            configPath = Path.Combine(configPath, "_.cs");
        }

        var editorConfigProvider = new EditorConfigProvider();
        LintConfiguration config = editorConfigProvider.GetConfiguration(configPath);
        return PrintConfig(config);
    }

    string path = parseResult.GetValue(pathArgument)!;
    string format = parseResult.GetValue(formatOption)!;
    string severity = parseResult.GetValue(severityOption)!;
    string[]? excludePatterns = parseResult.GetValue(excludeOption);

    bool semantic = parseResult.GetValue(semanticOption);
    RuleRegistry registry = RuleRegistry.CreateDefault();
    var configProvider = new EditorConfigProvider();
    var fileLinter = new FileLinter(registry, configProvider) { EnableSemantic = semantic };

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

static int PrintRuleList()
{
    RuleRegistry registry = RuleRegistry.CreateDefault();
    IReadOnlyDictionary<string, List<string>> aliases = RuleRegistry.GetAliases();

    using var stream = new MemoryStream();
    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

    writer.WriteStartArray();

    foreach (IRuleDefinition rule in registry.Rules)
    {
        writer.WriteStartObject();
        writer.WriteString("id", rule.RuleId);
        writer.WriteString("name", rule.Name);
        writer.WriteString("defaultSeverity", rule.DefaultSeverity.ToString().ToLowerInvariant());

        writer.WriteStartArray("configKeys");

        foreach (string key in rule.ConfigKeys)
        {
            writer.WriteStringValue(key);
        }

        writer.WriteEndArray();

        writer.WriteStartArray("aliases");

        if (aliases.TryGetValue(rule.RuleId, out List<string>? aliasList))
        {
            foreach (string alias in aliasList)
            {
                writer.WriteStringValue(alias);
            }
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    writer.WriteEndArray();
    writer.Flush();

    Console.WriteLine(Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Position));
    return 0;
}

static int PrintConfig(LintConfiguration config)
{
    using var stream = new MemoryStream();
    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

    writer.WriteStartObject();

    foreach (KeyValuePair<string, string> kvp in config.Properties.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
    {
        writer.WriteString(kvp.Key, kvp.Value);
    }

    writer.WriteEndObject();
    writer.Flush();

    Console.WriteLine(Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Position));
    return 0;
}
