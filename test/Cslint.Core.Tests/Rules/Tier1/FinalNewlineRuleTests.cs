using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class FinalNewlineRuleTests
{
    private readonly FinalNewlineRule _rule = new();

    [Theory]
    [InlineData("class Foo { }\n", true)]
    [InlineData("class Foo { }", false)]
    public void Analyze_CorrectFinalNewline_ReturnsNoDiagnostics(string source, bool expectNewline)
    {
        RuleContext context = CreateContext(source, expectNewline);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MissingFinalNewline_ReturnsDiagnostic()
    {
        RuleContext context = CreateContext("class Foo { }", true);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT004", diagnostics[0].RuleId);
        Assert.Contains("should end with a newline", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_UnwantedFinalNewline_ReturnsDiagnostic()
    {
        RuleContext context = CreateContext("class Foo { }\n", false);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("should not end with a newline", diagnostics[0].Message);
    }

    private static RuleContext CreateContext(string source, bool expectNewline)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["insert_final_newline"] = expectNewline.ToString().ToLowerInvariant() });
        return TestHelper.CreateContext(source, config);
    }
}
