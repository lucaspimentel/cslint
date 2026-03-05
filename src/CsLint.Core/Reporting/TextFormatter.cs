using System.Text;
using Cslint.Core.Rules;

namespace Cslint.Core.Reporting;

public sealed class TextFormatter : IOutputFormatter
{
    public string Format(IReadOnlyList<LintDiagnostic> diagnostics)
    {
        if (diagnostics.Count == 0)
        {
            return string.Empty;
        }

        var sb = new StringBuilder();

        foreach (LintDiagnostic d in diagnostics)
        {
            string severity = d.Severity.ToString().ToLowerInvariant();
            sb.AppendLine($"{d.FilePath}({d.Line},{d.Column}): {severity} {d.RuleId}: {d.Message}");
        }

        return sb.ToString();
    }
}
