using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class NoBlankLinesAtStartOfFileRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_no_blank_lines_at_start_of_file";

    public string RuleId => "CSLINT009";

    public string Name => "NoBlankLinesAtStartOfFile";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetBool(ConfigKey);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var diagnostics = new List<LintDiagnostic>();
        int lineNumber = 0;

        foreach (ReadOnlySpan<char> line in context.SourceString.AsSpan().EnumerateLines())
        {
            lineNumber++;

            if (!line.IsWhiteSpace())
            {
                break;
            }

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "File should not start with blank lines",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = lineNumber,
                    Column = 1,
                });
        }

        return diagnostics;
    }
}
