using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NullPropagationRuleTests
{
    private readonly NullPropagationRule _rule = new();

    private static LintConfiguration Enabled =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_null_propagation"] = "true",
        });

    [Fact]
    public void Analyze_NotEqualsNull_MemberAccess_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                string M(string s) => s != null ? s.ToString() : null;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0031", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_EqualsNull_MemberAccess_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                string M(string s) => s == null ? null : s.ToString();
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0031", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NullOnLeft_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int? M(string s) => null != s ? s.Length : null;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_AlreadyUsingNullPropagation_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M(string s) => s?.ToString();
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_DifferentIdentifiers_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M(string s, string t) => s != null ? t.ToString() : null;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultiStatementTernary_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M(string s) => s != null ? s.ToString() : "default";
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigNotSet_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                string M(string s) => s != null ? s.ToString() : null;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>());
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ChainedMemberAccess_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int? M(string s) => s != null ? s.ToString().Length : null;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_PropertyAccess_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                int? M(string s) => s != null ? s.Length : null;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_BareIdentifierInTrue_ReturnsNoDiagnostics()
    {
        // x != null ? x : null is null coalescing, not null propagation
        string source = """
            class C
            {
                string M(string s) => s != null ? s : null;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enabled);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
