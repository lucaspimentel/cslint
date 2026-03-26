using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConstantArrayAsArgumentRuleTests
{
    private readonly ConstantArrayAsArgumentRule _rule = new();

    [Theory]
    [InlineData("M(new[] { 1, 2, 3 });")]
    [InlineData("M(new int[] { 1, 2, 3 });")]
    [InlineData("M(new[] { \"a\", \"b\" });")]
    public void Analyze_ConstantArrayArg_ReturnsDiagnostic(string call)
    {
        string source = $$"""
            namespace N;

            class C
            {
                void M(object o) { }
                void Test()
                {
                    {{call}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1861", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_NonConstantArrayArg_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                int _x;
                void M(int[] a) { }
                void Test()
                {
                    M(new[] { _x, _x + 1 });
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ArrayInLocalVariable_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                void M()
                {
                    var a = new[] { 1, 2, 3 };
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EmptyArrayArg_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                void M(int[] a) { }
                void Test()
                {
                    M(new int[] { });
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                void M(int[] a) { }
                void Test() { M(new[] { 1, 2 }); }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1861.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
