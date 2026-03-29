using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class AliasUsingDirectiveAlphabeticalOrderRuleTests
{
    private readonly AliasUsingDirectiveAlphabeticalOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_using_directive_ordering"] = "true",
        });

    [Fact]
    public void Analyze_AliasAlphabetical_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Bar = System.Int32;
            using Foo = System.String;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AliasNotAlphabetical_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using Foo = System.String;
            using Bar = System.Int32;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1211", diagnostics[0].RuleId);
        Assert.Contains("alias name", diagnostics[0].Message);
        Assert.Contains("Bar", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_SingleAlias_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Foo = System.String;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }
}
