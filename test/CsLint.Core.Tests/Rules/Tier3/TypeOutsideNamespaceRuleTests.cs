using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class TypeOutsideNamespaceRuleTests
{
    private readonly TypeOutsideNamespaceRule _rule = new();

    [Theory]
    [InlineData("class C { }")]
    [InlineData("struct S { }")]
    [InlineData("enum E { A }")]
    [InlineData("interface I { }")]
    public void Analyze_TypeOutsideNamespace_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1050", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_TypeInsideNamespace_NoDiagnostic()
    {
        const string source = """
            namespace Foo;

            class C { }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TypeInsideBlockNamespace_NoDiagnostic()
    {
        const string source = """
            namespace Foo
            {
                class C { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "class C { }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1050.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
