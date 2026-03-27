using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class TypeParameterNamingRuleTests
{
    private readonly TypeParameterNamingRule _rule = new();

    private static LintConfiguration DefaultConfig() =>
        new(new Dictionary<string, string>());

    [Theory]
    [InlineData("class C<K> { }")]
    [InlineData("class C<Value> { }")]
    [InlineData("class C<a> { }")]
    public void Analyze_TypeParameterNotStartingWithT_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, DefaultConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1314", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("class C<T> { }")]
    [InlineData("class C<TKey> { }")]
    [InlineData("class C<TValue> { }")]
    [InlineData("class C<T, TKey, TValue> { }")]
    [InlineData("class C<T0> { }")]
    [InlineData("class C<T0, T1, T2> { }")]
    public void Analyze_TypeParameterStartingWithT_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source, DefaultConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MethodTypeParameter_ReturnsDiagnostic()
    {
        string source = "class C { void M<K>() { } }";
        RuleContext context = TestHelper.CreateContext(source, DefaultConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleInvalid_ReturnsMultipleDiagnostics()
    {
        string source = "class C<K, V> { }";
        RuleContext context = TestHelper.CreateContext(source, DefaultConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }
}
