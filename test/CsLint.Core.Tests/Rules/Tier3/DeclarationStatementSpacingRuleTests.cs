using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class DeclarationStatementSpacingRuleTests
{
    private readonly DeclarationStatementSpacingRule _rule = new();

    [Fact]
    public void Analyze_ExtraSpaces_ReturnsDiagnostic()
    {
        string source = "class C { void M() { int  x = 1; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_around_declaration_statements"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT291", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_NormalSpacing_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { int x = 1; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_around_declaration_statements"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_IgnoreValue_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { int  x = 1; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_space_around_declaration_statements"] = "ignore",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { int  x = 1; } }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
