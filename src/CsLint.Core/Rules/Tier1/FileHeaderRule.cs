using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class FileHeaderRule : IRuleDefinition
{
    public string RuleId => "CSLINT007";

    public string Name => "FileHeader";

    public IReadOnlyList<string> ConfigKeys { get; } = ["file_header_template"];

    public bool IsEnabled(LintConfiguration configuration)
    {
        string? template = configuration.GetValue("file_header_template");
        return template is not null && !string.Equals(template, "unset", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        string? template = context.Configuration.GetValue("file_header_template");

        if (template is null || string.Equals(template, "unset", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        string[] templateLines = template.Split('\n');
        string source = context.SourceString;
        int lineIndex = 0;

        foreach (ReadOnlySpan<char> line in source.AsSpan().EnumerateLines())
        {
            if (lineIndex >= templateLines.Length)
            {
                break;
            }

            string expectedComment = $"// {templateLines[lineIndex].Trim()}";
            string actualLine = line.ToString().TrimEnd();

            if (!string.Equals(actualLine, expectedComment, StringComparison.Ordinal))
            {
                return
                [
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"File header does not match template. Expected: '{expectedComment}'",
                        Severity = LintSeverity.Warning,
                        FilePath = context.FilePath,
                        Line = lineIndex + 1,
                        Column = 1,
                    },
                ];
            }

            lineIndex++;
        }

        // If we didn't get through all template lines, the file is too short
        if (lineIndex < templateLines.Length)
        {
            return
            [
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "File header is missing or incomplete",
                    Severity = LintSeverity.Warning,
                    FilePath = context.FilePath,
                    Line = 1,
                    Column = 1,
                },
            ];
        }

        return [];
    }
}
