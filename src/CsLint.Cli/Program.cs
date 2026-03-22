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

var summaryOption = new Option<bool>("--summary")
{
    Description = "Show a summary of diagnostics grouped by rule ID",
};

var rulesOption = new Option<string>("--rules")
{
    Description = "Comma-separated rule IDs to run (e.g., CSLINT266,CSLINT268), or 'all' to enable every rule. Ignores .editorconfig when set.",
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
    summaryOption,
    rulesOption,
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
    bool summary = parseResult.GetValue(summaryOption);
#if !SEMANTIC
    if (semantic)
    {
        Console.Error.WriteLine("Warning: --semantic is not available in the native AOT build. Install the .NET tool version for semantic analysis.");
    }
#endif

    RuleRegistry registry = RuleRegistry.CreateDefault();
    string? rulesValue = parseResult.GetValue(rulesOption);

    HashSet<string>? ruleFilter = null;
    bool skipEnabledCheck = false;

    if (rulesValue is not null)
    {
        if (string.Equals(rulesValue, "all", StringComparison.OrdinalIgnoreCase))
        {
            skipEnabledCheck = true;
        }
        else
        {
            var knownIds = new HashSet<string>(registry.Rules.Select(r => r.RuleId), StringComparer.OrdinalIgnoreCase);
            string[] requested = rulesValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (requested.Length == 0)
            {
                Console.Error.WriteLine("--rules requires at least one rule ID or 'all'.");
                return 2;
            }

            ruleFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string id in requested)
            {
                if (!knownIds.Contains(id))
                {
                    Console.Error.WriteLine($"Unknown rule ID: {id}");
                    return 2;
                }

                ruleFilter.Add(id);
            }

            skipEnabledCheck = true;
        }
    }

    var configProvider = new EditorConfigProvider();
    var fileLinter = new FileLinter(registry, configProvider)
    {
        RuleFilter = ruleFilter,
        SkipEnabledCheck = skipEnabledCheck,
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

    if (summary)
    {
        PrintSummary(filteredDiagnostics, registry);
    }
    else
    {
        string output = formatter.Format(filteredDiagnostics);

        if (!string.IsNullOrEmpty(output))
        {
            Console.Write(output);
        }
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

static void PrintSummary(IReadOnlyList<LintDiagnostic> diagnostics, RuleRegistry registry)
{
    if (diagnostics.Count == 0)
    {
        return;
    }

    Dictionary<string, string> ruleNames = registry.Rules.ToDictionary(r => r.RuleId, r => r.Name);

    List<(string RuleId, string Name, int Count)> groups = diagnostics
        .GroupBy(d => d.RuleId)
        .Select(g => (RuleId: g.Key, Name: ruleNames.GetValueOrDefault(g.Key, ""), Count: g.Count()))
        .OrderByDescending(g => g.Count)
        .ThenBy(g => g.RuleId, StringComparer.Ordinal)
        .ToList();

    int maxIdLen = Math.Max("Rule".Length, groups.Max(g => g.RuleId.Length));
    int maxNameLen = Math.Max("Name".Length, groups.Max(g => g.Name.Length));
    int maxCountLen = Math.Max("Count".Length, diagnostics.Count.ToString().Length);
    int lineWidth = maxIdLen + 2 + maxNameLen + 2 + maxCountLen;

    Console.WriteLine($"{"Rule".PadRight(maxIdLen)}  {"Name".PadRight(maxNameLen)}  {"Count".PadLeft(maxCountLen)}");
    Console.WriteLine(new string('\u2500', lineWidth));

    foreach ((string ruleId, string name, int count) in groups)
    {
        Console.WriteLine($"{ruleId.PadRight(maxIdLen)}  {name.PadRight(maxNameLen)}  {count.ToString().PadLeft(maxCountLen)}");
    }

    Console.WriteLine(new string('\u2500', lineWidth));
    Console.WriteLine($"{"Total".PadRight(maxIdLen)}  {"".PadRight(maxNameLen)}  {diagnostics.Count.ToString().PadLeft(maxCountLen)}");
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
