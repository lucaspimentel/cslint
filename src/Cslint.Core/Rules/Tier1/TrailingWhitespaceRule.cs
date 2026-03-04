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
        string text = context.SourceText.ToString();
        string[] lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');

            if (line.Length > 0 && char.IsWhiteSpace(line[^1]))
            {
                string trimmed = line.TrimEnd();
                int column = trimmed.Length + 1;

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Trailing whitespace detected",
                        Severity = LintSeverity.Warning,
                        FilePath = context.FilePath,
                        Line = i + 1,
                        Column = column,
                    });
            }
        }

        return diagnostics;
    }
}
