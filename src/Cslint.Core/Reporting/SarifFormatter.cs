using System.Text;
using System.Text.Json;
using Cslint.Core.Rules;

namespace Cslint.Core.Reporting;

public sealed class SarifFormatter : IOutputFormatter
{
    public string Format(IReadOnlyList<LintDiagnostic> diagnostics)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        writer.WriteStartObject();
        writer.WriteString("$schema", "https://raw.githubusercontent.com/oasis-tcs/sarif-spec/main/sarif-2.1/schema/sarif-schema-2.1.0.json");
        writer.WriteString("version", "2.1.0");

        writer.WriteStartArray("runs");
        writer.WriteStartObject();

        // Tool
        writer.WriteStartObject("tool");
        writer.WriteStartObject("driver");
        writer.WriteString("name", "Cslint");
        writer.WriteString("version", "0.0.1");
        writer.WriteString("informationUri", "https://github.com/lucaspimentel/Cslint");

        // Rules
        writer.WriteStartArray("rules");
        var seenRules = new HashSet<string>(StringComparer.Ordinal);

        foreach (LintDiagnostic d in diagnostics)
        {
            if (seenRules.Add(d.RuleId))
            {
                writer.WriteStartObject();
                writer.WriteString("id", d.RuleId);
                writer.WriteEndObject();
            }
        }

        writer.WriteEndArray(); // rules
        writer.WriteEndObject(); // driver
        writer.WriteEndObject(); // tool

        // Results
        writer.WriteStartArray("results");

        foreach (LintDiagnostic d in diagnostics)
        {
            writer.WriteStartObject();
            writer.WriteString("ruleId", d.RuleId);
            writer.WriteString("level", d.Severity switch
            {
                LintSeverity.Error => "error",
                LintSeverity.Warning => "warning",
                LintSeverity.Info => "note",
                _ => "none",
            });

            writer.WriteStartObject("message");
            writer.WriteString("text", d.Message);
            writer.WriteEndObject();

            writer.WriteStartArray("locations");
            writer.WriteStartObject();
            writer.WriteStartObject("physicalLocation");

            writer.WriteStartObject("artifactLocation");
            writer.WriteString("uri", d.FilePath.Replace('\\', '/'));
            writer.WriteEndObject();

            writer.WriteStartObject("region");
            writer.WriteNumber("startLine", d.Line);
            writer.WriteNumber("startColumn", d.Column);
            writer.WriteEndObject();

            writer.WriteEndObject(); // physicalLocation
            writer.WriteEndObject();
            writer.WriteEndArray(); // locations

            writer.WriteEndObject(); // result
        }

        writer.WriteEndArray(); // results
        writer.WriteEndObject(); // run
        writer.WriteEndArray(); // runs
        writer.WriteEndObject(); // root

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
