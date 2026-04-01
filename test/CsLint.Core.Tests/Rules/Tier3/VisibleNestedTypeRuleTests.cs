using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class VisibleNestedTypeRuleTests
{
    private readonly VisibleNestedTypeRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1034.severity"] = "warning",
    });

    [Fact]
    public void Analyze_PublicNestedClass_ReturnsDiagnostic()
    {
        const string source = """
            class Outer
            {
                public class Inner { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1034", diagnostic.RuleId);
        Assert.Contains("Inner", diagnostic.Message);
    }

    [Fact]
    public void Analyze_PrivateNestedClass_NoDiagnostic()
    {
        const string source = """
            class Outer
            {
                private class Inner { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TopLevelClass_NoDiagnostic()
    {
        const string source = "public class C { }";
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_InternalNestedClass_NoDiagnostic()
    {
        const string source = """
            class Outer
            {
                internal class Inner { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            class Outer
            {
                public class Inner { }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1034.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
