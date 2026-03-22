using System.CommandLine;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Reporting;
using Cslint.Core.Rules;

var pathArgument = new Argument<string[]>("path")
{
    Description = "One or more paths to C# files or directories to lint",
    Arity = ArgumentArity.ZeroOrMore,
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

var versionOption = new Option<bool>("--version")
{
    Description = "Show version information and exit",
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
    versionOption,
};

rootCommand.SetAction(async (parseResult, cancellationToken) =>
{
    if (parseResult.GetValue(versionOption))
    {
        string version = Assembly.GetEntryAssembly()
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ?? "unknown";

        Console.WriteLine(version);
        return 0;
    }

    bool listRules = parseResult.GetValue(listRulesOption);

    if (listRules)
    {
        return PrintRuleList();
    }

    string[]? paths = parseResult.GetValue(pathArgument);

    if (paths is null or { Length: 0 })
    {
        paths = ["."];
    }

    bool showConfig = parseResult.GetValue(showConfigOption);

    if (showConfig)
    {
        if (paths.Length > 1)
        {
            Console.Error.WriteLine("--show-config accepts only a single path.");
            return 2;
        }

        string configPath = Path.GetFullPath(paths[0]);

        // editorconfig resolves by file extension, so use a dummy .cs file for directories
        if (Directory.Exists(configPath))
        {
            configPath = Path.Combine(configPath, "_.cs");
        }

        var editorConfigProvider = new EditorConfigProvider();
        LintConfiguration config = editorConfigProvider.GetConfiguration(configPath);
        return PrintConfig(config);
    }

    string format = parseResult.GetValue(formatOption)!;
    string severity = parseResult.GetValue(severityOption)!;
    string[]? excludePatterns = parseResult.GetValue(excludeOption);

    bool semantic = parseResult.GetValue(semanticOption);
#if !SEMANTIC
    if (semantic)
    {
        Console.Error.WriteLine("Warning: --semantic is not available in the native AOT build. Install the .NET tool version for semantic analysis.");
    }
#endif

    RuleRegistry registry = RuleRegistry.CreateDefault();
    var configProvider = new EditorConfigProvider();
    var fileLinter = new FileLinter(registry, configProvider)
    {
#if SEMANTIC
        EnableSemantic = semantic,
#endif
    };

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

    var allDiagnostics = new List<LintDiagnostic>();
    bool hasError = false;

    foreach (string path in paths)
    {
        string fullPath = Path.GetFullPath(path);

        if (File.Exists(fullPath))
        {
            allDiagnostics.AddRange(fileLinter.LintFile(fullPath));
        }
        else if (Directory.Exists(fullPath))
        {
            var directoryLinter = new DirectoryLinter(fileLinter);
            IReadOnlyList<LintDiagnostic> diagnostics = await directoryLinter.LintDirectoryAsync(fullPath, excludePatterns, cancellationToken);
            allDiagnostics.AddRange(diagnostics);
        }
        else
        {
            Console.Error.WriteLine($"Path not found: {fullPath}");
            hasError = true;
        }
    }

    if (hasError && allDiagnostics.Count == 0)
    {
        return 2;
    }

    // Filter by severity
    IReadOnlyList<LintDiagnostic> filteredDiagnostics = minSeverity > LintSeverity.Info
        ? allDiagnostics.Where(d => d.Severity >= minSeverity).ToList()
        : allDiagnostics;

    string output = formatter.Format(filteredDiagnostics);

    if (!string.IsNullOrEmpty(output))
    {
        Console.Write(output);
    }

    if (hasError)
    {
        return 2;
    }

    return filteredDiagnostics.Count > 0 ? 1 : 0;
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
