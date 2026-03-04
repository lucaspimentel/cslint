using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class InterfacePrefixRuleTests
{
    private readonly InterfacePrefixRule _rule = new();

    [Theory]
    [InlineData("interface IFoo { }")]
    [InlineData("interface IMyInterface { }")]
    public void Analyze_IPrefixed_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("interface Foo { }", "Foo")]
    [InlineData("interface myInterface { }", "myInterface")]
    public void Analyze_NotIPrefixed_ReturnsDiagnostic(string source, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT101", diagnostics[0].RuleId);
        Assert.Contains(name, diagnostics[0].Message);
    }
}
