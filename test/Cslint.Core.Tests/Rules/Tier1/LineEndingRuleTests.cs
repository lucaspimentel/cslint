using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class LineEndingRuleTests
{
    private readonly LineEndingRule _rule = new();

    [Theory]
    [InlineData("a\nb\n", "lf")]
    [InlineData("a\r\nb\r\n", "crlf")]
    [InlineData("a\rb\r", "cr")]
    public void Analyze_CorrectLineEndings_ReturnsNoDiagnostics(string source, string eol)
    {
        RuleContext context = CreateContext(source, eol);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_CrlfWhenLfExpected_ReturnsDiagnostics()
    {
        RuleContext context = CreateContext("a\r\nb\r\n", "lf");

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, d => Assert.Equal("CSLINT003", d.RuleId));
    }

    [Fact]
    public void Analyze_LfWhenCrlfExpected_ReturnsDiagnostics()
    {
        RuleContext context = CreateContext("a\nb\n", "crlf");

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    private static RuleContext CreateContext(string source, string eol)
    {
        var config = new LintConfiguration(new Dictionary<string, string> { ["end_of_line"] = eol });
        return TestHelper.CreateContext(source, config);
    }
}
