using Cslint.Core.Reporting;
using Cslint.Core.Rules;

namespace Cslint.Core.Tests.Reporting;

public class TextFormatterTests
{
    private readonly TextFormatter _formatter = new();

    [Fact]
    public void Format_Empty_ReturnsEmptyString()
    {
        string result = _formatter.Format([]);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Format_SingleDiagnostic_ReturnsFormattedLine()
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

        Assert.Contains("test.cs(5,10): warning CSLINT001: Trailing whitespace", result);
    }
}
