using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ProtectedMemberInSealedTypeRuleTests
{
    private readonly ProtectedMemberInSealedTypeRule _rule = new();

    [Theory]
    [InlineData("sealed class C { protected void M() { } }")]
    [InlineData("sealed class C { protected int Field; }")]
    [InlineData("sealed class C { protected int Prop { get; set; } }")]
    [InlineData("sealed class C { protected event System.EventHandler E; }")]
    public void Analyze_ProtectedMemberInSealedType_ReturnsDiagnostic(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1047", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_ProtectedOverrideInSealedType_NoDiagnostic()
    {
        const string source = """
            sealed class C
            {
                protected override string ToString() => "C";
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ProtectedInUnsealedType_NoDiagnostic()
    {
        const string source = """
            class C
            {
                protected void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PublicInSealedType_NoDiagnostic()
    {
        const string source = """
            sealed class C
            {
                public void M() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "sealed class C { protected void M() { } }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1047.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
