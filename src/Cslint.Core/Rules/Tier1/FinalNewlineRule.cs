using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class FinalNewlineRule : IRuleDefinition
{
    public string RuleId => "CSLINT004";

    public string Name => "FinalNewline";

    public IReadOnlyList<string> ConfigKeys { get; } = ["insert_final_newline"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("insert_final_newline") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        bool expectFinalNewline = context.Configuration.GetBool("insert_final_newline");
        string text = context.SourceText.ToString();

        if (text.Length == 0)
        {
            return [];
        }

        bool hasFinalNewline = text[^1] == '\n' || text[^1] == '\r';

        if (expectFinalNewline && !hasFinalNewline)
        {
            // Count lines to report correct position
            int lineCount = 1;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    lineCount++;
                }
                else if (text[i] == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n'))
                {
                    lineCount++;
                }
            }

            return
            [
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "File should end with a newline",
                    Severity = LintSeverity.Warning,
                    FilePath = context.FilePath,
                    Line = lineCount,
                    Column = text.Length - text.LastIndexOfAny(['\n', '\r']),
                },
            ];
        }

        if (!expectFinalNewline && hasFinalNewline)
        {
            int lineCount = text.Count(c => c == '\n') + 1;

            return
            [
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "File should not end with a newline",
                    Severity = LintSeverity.Warning,
                    FilePath = context.FilePath,
                    Line = lineCount,
                    Column = 1,
                },
            ];
        }

        return [];
    }
}
