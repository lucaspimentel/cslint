using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PublicConstructorOnAbstractTypeRuleTests
{
    private readonly PublicConstructorOnAbstractTypeRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1012.severity"] = "warning",
    });

    [Fact]
    public void Analyze_PublicCtorOnAbstractClass_ReturnsDiagnostic()
    {
        const string source = """
            abstract class C
            {
                public C() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1012", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_ProtectedCtorOnAbstractClass_NoDiagnostic()
    {
        const string source = """
            abstract class C
            {
                protected C() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PublicCtorOnConcreteClass_NoDiagnostic()
    {
        const string source = """
            class C
            {
                public C() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleCtors_OnlyPublicFlagged()
    {
        const string source = """
            abstract class C
            {
                public C() { }
                protected C(int x) { }
                private C(string s) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "abstract class C { public C() { } }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1012.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
