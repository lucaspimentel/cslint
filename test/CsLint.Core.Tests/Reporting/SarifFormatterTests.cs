using System.Text.Json;
using Cslint.Core.Reporting;
using Cslint.Core.Rules;

namespace Cslint.Core.Tests.Reporting;

public class SarifFormatterTests
{
    private readonly SarifFormatter _formatter = new();

    [Fact]
    public void Format_ReturnsValidSarifStructure()
    {
        var diagnostics = new List<LintDiagnostic>
        {
            new()
            {
                RuleId = "CSLINT001",
                Message = "Trailing whitespace",
                Severity = LintSeverity.Warning,
                FilePath = "test.cs",
                Line = 5,
                Column = 10,
            },
        };

        string result = _formatter.Format(diagnostics);

        using JsonDocument doc = JsonDocument.Parse(result);
        JsonElement root = doc.RootElement;
        Assert.Equal("2.1.0", root.GetProperty("version").GetString());
        Assert.True(root.TryGetProperty("runs", out JsonElement runs));
        Assert.Equal(1, runs.GetArrayLength());

        JsonElement results = runs[0].GetProperty("results");
        Assert.Equal(1, results.GetArrayLength());
        Assert.Equal("CSLINT001", results[0].GetProperty("ruleId").GetString());
        Assert.Equal("warning", results[0].GetProperty("level").GetString());
    }
}
