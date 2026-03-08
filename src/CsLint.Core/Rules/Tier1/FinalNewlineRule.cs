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
        string text = context.SourceString;

        if (text.Length == 0)
        {
            return [];
        }

        bool hasFinalNewline = text[^1] is '\n' or '\r';

        if (expectFinalNewline && !hasFinalNewline)
        {
            int lineCount = context.SourceText.Lines.Count;

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
            int lineCount = context.SourceText.Lines.Count;

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
