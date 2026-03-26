using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EmptyInterfaceRuleTests
{
    private readonly EmptyInterfaceRule _rule = new();

    [Fact]
    public void Analyze_EmptyInterface_ReturnsDiagnostic()
    {
        const string source = "interface IMarker { }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1040", diagnostic.RuleId);
        Assert.Contains("IMarker", diagnostic.Message);
    }

    [Fact]
    public void Analyze_InterfaceWithMembers_NoDiagnostic()
    {
        const string source = """
            interface IFoo
            {
                void DoWork();
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "interface IMarker { }";
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1040.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
