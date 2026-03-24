using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class UnboundGenericInNameofRuleTests
{
    private readonly UnboundGenericInNameofRule _rule = new();

    [Fact]
    public void Analyze_ClosedGenericInNameof_ReturnsDiagnostic()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                string M() => nameof(List<int>);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_unbound_generic_type_in_nameof"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0340", diagnostics[0].RuleId);
        Assert.Contains("List<>", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_MultipleTypeArgs_ReturnsDiagnostic()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                string M() => nameof(Dictionary<string, int>);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_unbound_generic_type_in_nameof"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("Dictionary<>", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_NonGenericInNameof_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M() => nameof(C);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_unbound_generic_type_in_nameof"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_QualifiedGenericInNameof_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                string M() => nameof(System.Collections.Generic.List<int>);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_unbound_generic_type_in_nameof"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("List<>", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                string M() => nameof(List<int>);
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_unbound_generic_type_in_nameof"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                string M() => nameof(List<int>);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NotNameofInvocation_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Collections.Generic;
            class C
            {
                List<int> M() => new List<int>();
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_unbound_generic_type_in_nameof"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
