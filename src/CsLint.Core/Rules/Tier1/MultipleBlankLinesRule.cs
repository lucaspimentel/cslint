using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class MultipleBlankLinesRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_no_multiple_blank_lines";

    private const string StandardKey = "dotnet_style_allow_multiple_blank_lines_experimental";

    public string RuleId => "SA1507";

    public string Name => "MultipleBlankLines";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, StandardKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration)
    {
        // CsLint key: true = disallow multiple blank lines
        if (configuration.GetValue(ConfigKey) is not null)
        {
            return configuration.GetBool(ConfigKey);
        }

        // Standard key: false = disallow multiple blank lines (inverted semantics)
        (string? value, string? _) = configuration.GetValueWithSeverity(StandardKey);
        return string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var diagnostics = new List<LintDiagnostic>();
        int lineNumber = 0;
        bool previousLineBlank = false;

        foreach (ReadOnlySpan<char> line in context.SourceString.AsSpan().EnumerateLines())
        {
            lineNumber++;

            bool currentLineBlank = line.IsWhiteSpace();

            if (currentLineBlank && previousLineBlank)
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Avoid multiple consecutive blank lines",
                        Severity = LintSeverity.Warning,
                        FilePath = context.FilePath,
                        Line = lineNumber,
                        Column = 1,
                    });
            }

            previousLineBlank = currentLineBlank;
        }

        return diagnostics;
    }
}
