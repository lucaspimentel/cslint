using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class TrailingWhitespaceRule : IRuleDefinition
{
    public string RuleId => "CSLINT001";

    public string Name => "TrailingWhitespace";

    public IReadOnlyList<string> ConfigKeys { get; } = ["trim_trailing_whitespace"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetBool("trim_trailing_whitespace");

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var diagnostics = new List<LintDiagnostic>();
        int lineNumber = 0;

        foreach (ReadOnlySpan<char> line in context.SourceString.AsSpan().EnumerateLines())
        {
            lineNumber++;

            if (line.Length > 0 && char.IsWhiteSpace(line[^1]))
            {
                int column = line.TrimEnd().Length + 1;

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Trailing whitespace detected",
                        Severity = LintSeverity.Warning,
                        FilePath = context.FilePath,
                        Line = lineNumber,
                        Column = column,
                    });
            }
        }

        return diagnostics;
    }
}
