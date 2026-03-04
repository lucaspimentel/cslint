using System.Text;
using System.Text.Json;
using Cslint.Core.Rules;

namespace Cslint.Core.Reporting;

public sealed class JsonFormatter : IOutputFormatter
{
    public string Format(IReadOnlyList<LintDiagnostic> diagnostics)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        writer.WriteStartArray();

        foreach (LintDiagnostic d in diagnostics)
        {
            writer.WriteStartObject();
            writer.WriteString("ruleId", d.RuleId);
            writer.WriteString("message", d.Message);
            writer.WriteString("severity", d.Severity.ToString().ToLowerInvariant());
            writer.WriteString("filePath", d.FilePath);
            writer.WriteNumber("line", d.Line);
            writer.WriteNumber("column", d.Column);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
