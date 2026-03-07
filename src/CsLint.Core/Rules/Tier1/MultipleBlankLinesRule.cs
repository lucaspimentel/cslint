using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class MultipleBlankLinesRule : IRuleDefinition
{
    public string RuleId => "CSLINT008";

    public string Name => "MultipleBlankLines";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_no_multiple_blank_lines"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetBool("csharp_no_multiple_blank_lines");

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
