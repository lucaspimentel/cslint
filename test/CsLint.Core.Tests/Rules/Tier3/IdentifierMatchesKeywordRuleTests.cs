using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class IdentifierMatchesKeywordRuleTests
{
    private readonly IdentifierMatchesKeywordRule _rule = new();

    [Theory]
    [InlineData("namespace N; public class Event { }")]
    [InlineData("namespace N; class C { public void Delegate() { } }")]
    [InlineData("namespace N; class C { public int Object { get; } }")]
    public void Analyze_IdentifierMatchesKeyword_ReturnsDiagnostic(
        string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1716", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("namespace N; public class MyClass { }")]
    [InlineData("namespace N; class C { public void DoWork() { } }")]
    public void Analyze_NormalIdentifier_NoDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_PrivateMember_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                private void Event() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "namespace N; public class Event { }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1716.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
