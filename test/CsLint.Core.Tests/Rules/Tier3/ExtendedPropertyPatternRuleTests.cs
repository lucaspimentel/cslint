using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExtendedPropertyPatternRuleTests
{
    private readonly ExtendedPropertyPatternRule _rule = new();

    [Fact]
    public void Analyze_NestedPropertyPattern_ReturnsDiagnostic()
    {
        string source = """
            class A { public B B { get; set; } }
            class B { public int C { get; set; } }
            class Test
            {
                bool M(A x) => x is { B: { C: 1 } };
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_extended_property_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT236", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_FlatPropertyPattern_ReturnsNoDiagnostics()
    {
        string source = """
            class A { public int B { get; set; } }
            class Test
            {
                bool M(A x) => x is { B: 1 };
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_extended_property_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NestedWithType_ReturnsNoDiagnostics()
    {
        string source = """
            class A { public object B { get; set; } }
            class SomeType { public int C { get; set; } }
            class Test
            {
                bool M(A x) => x is { B: SomeType { C: 1 } };
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_extended_property_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NestedWithDesignation_ReturnsNoDiagnostics()
    {
        string source = """
            class A { public B B { get; set; } }
            class B { public int C { get; set; } }
            class Test
            {
                bool M(A x) => x is { B: { C: 1 } y };
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_extended_property_pattern"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class A { public B B { get; set; } }
            class B { public int C { get; set; } }
            class Test
            {
                bool M(A x) => x is { B: { C: 1 } };
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_extended_property_pattern"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class A { public B B { get; set; } }
            class B { public int C { get; set; } }
            class Test
            {
                bool M(A x) => x is { B: { C: 1 } };
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
