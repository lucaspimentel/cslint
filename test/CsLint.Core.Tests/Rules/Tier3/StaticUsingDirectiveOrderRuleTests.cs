using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class StaticUsingDirectiveOrderRuleTests
{
    private readonly StaticUsingDirectiveOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_using_directive_ordering"] = "true",
        });

    [Fact]
    public void Analyze_StaticAfterRegular_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_StaticBeforeRegular_ReturnsDiagnostic()
    {
        string source = """
            using static System.Math;
            using System;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1216", diagnostics[0].RuleId);
        Assert.Contains("Static", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_OnlyStaticUsings_ReturnsNoDiagnostics()
    {
        string source = """
            using static System.Console;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }
}
