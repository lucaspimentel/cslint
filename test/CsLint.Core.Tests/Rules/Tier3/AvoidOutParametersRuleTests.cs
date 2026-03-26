using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class AvoidOutParametersRuleTests
{
    private readonly AvoidOutParametersRule _rule = new();

    [Fact]
    public void Analyze_PublicMethodWithOut_ReturnsDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public void M(out int result) { result = 0; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1021", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_TryPattern_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public bool TryParse(string s, out int result)
                {
                    result = 0;
                    return true;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_PrivateMethod_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                private void M(out int result) { result = 0; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_RefParameter_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public void M(ref int value) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public void M(out int result) { result = 0; }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1021.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
