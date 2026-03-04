using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class AccessibilityModifierRuleTests
{
    private readonly AccessibilityModifierRule _rule = new();

    private static LintConfiguration AlwaysConfig =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_require_accessibility_modifiers"] = "always",
        });

    [Theory]
    [InlineData("public class C { }")]
    [InlineData("internal class C { }")]
    [InlineData("public class C { private int _x; }")]
    public void Analyze_WithAccessibility_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, AlwaysConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class C { }", "class", "C")]
    [InlineData("public class C { void M() { } }", "method", "M")]
    public void Analyze_MissingAccessibility_ReturnsDiagnostic(string source, string kind, string name)
    {
        RuleContext context = TestHelper.CreateContext(source, AlwaysConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.RuleId == "CSLINT206" && d.Message.Contains(kind) && d.Message.Contains(name));
    }
}
