using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class MaxLineLengthRule : IRuleDefinition
{
    public string RuleId => "CSLINT005";

    public string Name => "MaxLineLength";

    public IReadOnlyList<string> ConfigKeys { get; } = ["max_line_length"];

    public bool IsEnabled(LintConfiguration configuration)
    {
        string? value = configuration.GetValue("max_line_length");
        return value is not null && !string.Equals(value, "off", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        int? maxLength = context.Configuration.GetInt("max_line_length");

        if (maxLength is null or <= 0)
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();
        string text = context.SourceText.ToString();
        string[] lines = text.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].TrimEnd('\r');

            if (line.Length > maxLength.Value)
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Line length {line.Length} exceeds maximum of {maxLength.Value}",
                        Severity = LintSeverity.Warning,
                        FilePath = context.FilePath,
                        Line = i + 1,
                        Column = maxLength.Value + 1,
                    });
            }
        }

        return diagnostics;
    }
}
