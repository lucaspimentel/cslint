using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class StaticUsingDirectiveAlphabeticalOrderRuleTests
{
    private readonly StaticUsingDirectiveAlphabeticalOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_using_directive_ordering"] = "true",
        });

    [Fact]
    public void Analyze_StaticAlphabetical_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using static System.Console;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_StaticNotAlphabetical_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using static System.Math;
            using static System.Console;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1217", diagnostics[0].RuleId);
        Assert.Contains("alphabetically", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_SingleStaticUsing_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }
}
