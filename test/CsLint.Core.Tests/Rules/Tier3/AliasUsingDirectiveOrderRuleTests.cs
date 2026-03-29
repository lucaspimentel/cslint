using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class AliasUsingDirectiveOrderRuleTests
{
    private readonly AliasUsingDirectiveOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_using_directive_ordering"] = "true",
        });

    [Fact]
    public void Analyze_AliasAfterAll_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Newtonsoft.Json;
            using static System.Math;
            using Foo = System.String;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AliasBeforeRegular_ReturnsDiagnostic()
    {
        string source = """
            using Foo = System.String;
            using System;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1209", diagnostics[0].RuleId);
        Assert.Contains("Alias", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_AliasBeforeStatic_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using Foo = System.String;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1209", diagnostics[0].RuleId);
    }
}
