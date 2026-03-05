using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class UsingDirectivePlacementRuleTests
{
    private readonly UsingDirectivePlacementRule _rule = new();

    [Fact]
    public void Analyze_OutsideNamespace_WhenPreferred_ReturnsNoDiagnostics()
    {
        string source = "using System;\nnamespace Foo { }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_using_directive_placement"] = "outside_namespace",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_InsideNamespace_WhenOutsidePreferred_ReturnsDiagnostic()
    {
        string source = "namespace Foo { using System; }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_using_directive_placement"] = "outside_namespace",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT207", diagnostics[0].RuleId);
    }
}
