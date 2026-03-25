using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class VarPreferenceRuleTests
{
    private readonly VarPreferenceRule _rule = new();

    [Fact]
    public void Analyze_VarWithApparentType_WhenPreferred_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { var x = new List<int>(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExplicitTypeWhenApparent_WhenVarPreferred_ReturnsDiagnostic()
    {
        string source = "class C { void M() { List<int> x = new List<int>(); } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0007", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("SpanContext? parent = null;")]
    [InlineData("TraceId traceId = default;")]
    public void Analyze_NullOrDefaultLiteral_NeverSuggestsVar(string declaration)
    {
        string source = $"class C {{ void M() {{ {declaration} }} }}";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_when_type_is_apparent"] = "true",
            ["csharp_style_var_for_built_in_types"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("ulong x = 0;")]
    [InlineData("long x = 0;")]
    [InlineData("short x = 0;")]
    [InlineData("double x = 0;")]
    public void Analyze_NumericLiteralTypeMismatch_DoesNotSuggestVar(string declaration)
    {
        string source = $"class C {{ void M() {{ {declaration} }} }}";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_for_built_in_types"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("int x = 42;")]
    [InlineData("string s = \"hello\";")]
    [InlineData("bool b = true;")]
    [InlineData("char c = 'a';")]
    [InlineData("long x = 42L;")]
    public void Analyze_LiteralMatchesDeclaredType_SuggestsVar(string declaration)
    {
        string source = $"class C {{ void M() {{ {declaration} }} }}";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_var_for_built_in_types"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("built-in", diagnostics[0].Message);
    }
}
