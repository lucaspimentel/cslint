using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ModifierOrderRuleTests
{
    private readonly ModifierOrderRule _rule = new();

    private static LintConfiguration OrderConfig =>
        new(new Dictionary<string, string>
        {
            ["csharp_preferred_modifier_order"] = "public,private,protected,internal,static,extern,new,virtual,abstract,sealed,override,readonly,unsafe,volatile,async",
        });

    [Fact]
    public void Analyze_CorrectOrder_ReturnsNoDiagnostics()
    {
        string source = "public static class C { }";
        RuleContext context = TestHelper.CreateContext(source, OrderConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_WrongOrder_ReturnsDiagnostic()
    {
        string source = "static public class C { }";
        RuleContext context = TestHelper.CreateContext(source, OrderConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT205", diagnostics[0].RuleId);
    }
}
