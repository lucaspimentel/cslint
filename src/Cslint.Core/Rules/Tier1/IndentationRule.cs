using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class IndentationRule : IRuleDefinition
{
    public string RuleId => "CSLINT002";

    public string Name => "Indentation";

    public IReadOnlyList<string> ConfigKeys { get; } = ["indent_style", "indent_size"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("indent_style") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var diagnostics = new List<LintDiagnostic>();
        string? style = context.Configuration.GetValue("indent_style");
        int? size = context.Configuration.GetInt("indent_size");

        if (style is null)
        {
            return diagnostics;
        }

        bool expectTabs = string.Equals(style, "tab", StringComparison.OrdinalIgnoreCase);
        string text = context.SourceText.ToString();
        string[] lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');

            if (line.Length == 0 || !char.IsWhiteSpace(line[0]))
            {
                continue;
            }

            // Extract leading whitespace
            int wsEnd = 0;

            while (wsEnd < line.Length && (line[wsEnd] == ' ' || line[wsEnd] == '\t'))
            {
                wsEnd++;
            }

            string leading = line[..wsEnd];

            if (expectTabs)
            {
                if (leading.Contains(' '))
                {
                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Expected tab indentation, found spaces",
                            Severity = LintSeverity.Warning,
                            FilePath = context.FilePath,
                            Line = i + 1,
                            Column = 1,
                        });
                }
            }
            else
            {
                if (leading.Contains('\t'))
                {
                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Expected space indentation, found tabs",
                            Severity = LintSeverity.Warning,
                            FilePath = context.FilePath,
                            Line = i + 1,
                            Column = 1,
                        });
                }
            }
        }

        return diagnostics;
    }
}
