using System.Text.Json;
using Cslint.Core.Reporting;
using Cslint.Core.Rules;

namespace Cslint.Core.Tests.Reporting;

public class JsonFormatterTests
{
    private readonly JsonFormatter _formatter = new();

    [Fact]
    public void Format_Empty_ReturnsEmptyArray()
    {
        string result = _formatter.Format([]);

        Assert.Equal("[]", result);
    }

    [Fact]
    public void Format_SingleDiagnostic_ReturnsValidJson()
    {
        var diagnostics = new List<LintDiagnostic>
        {
            new()
            {
                RuleId = "SA1028",
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
        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.Equal(1, root.GetArrayLength());
        Assert.Equal("SA1028", root[0].GetProperty("ruleId").GetString());
    }
}
