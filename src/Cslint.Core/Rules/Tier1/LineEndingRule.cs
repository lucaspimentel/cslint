using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class LineEndingRule : IRuleDefinition
{
    public string RuleId => "CSLINT003";

    public string Name => "LineEnding";

    public IReadOnlyList<string> ConfigKeys { get; } = ["end_of_line"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("end_of_line") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var diagnostics = new List<LintDiagnostic>();
        string? eol = context.Configuration.GetValue("end_of_line");

        if (eol is null)
        {
            return diagnostics;
        }

        string text = context.SourceText.ToString();
        bool expectCrlf = string.Equals(eol, "crlf", StringComparison.OrdinalIgnoreCase);
        bool expectCr = string.Equals(eol, "cr", StringComparison.OrdinalIgnoreCase);
        // Default: lf

        int line = 1;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r')
            {
                if (i + 1 < text.Length && text[i + 1] == '\n')
                {
                    // CRLF
                    if (!expectCrlf)
                    {
                        string expected = expectCr ? "cr" : "lf";
                        diagnostics.Add(CreateDiagnostic(line, $"Expected {expected} line ending, found crlf", context.FilePath));
                    }

                    i++; // skip the \n
                }
                else
                {
                    // CR only
                    if (!expectCr)
                    {
                        string expected = expectCrlf ? "crlf" : "lf";
                        diagnostics.Add(CreateDiagnostic(line, $"Expected {expected} line ending, found cr", context.FilePath));
                    }
                }

                line++;
            }
            else if (text[i] == '\n')
            {
                // LF only
                if (expectCrlf || expectCr)
                {
                    diagnostics.Add(CreateDiagnostic(line, $"Expected {eol} line ending, found lf", context.FilePath));
                }

                line++;
            }
        }

        return diagnostics;
    }

    private LintDiagnostic CreateDiagnostic(int line, string message, string filePath) =>
        new()
        {
            RuleId = RuleId,
            Message = message,
            Severity = LintSeverity.Warning,
            FilePath = filePath,
            Line = line,
            Column = 1,
        };
}
