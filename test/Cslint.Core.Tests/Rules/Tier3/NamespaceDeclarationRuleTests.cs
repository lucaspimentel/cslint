using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NamespaceDeclarationRuleTests
{
    private readonly NamespaceDeclarationRule _rule = new();

    [Fact]
    public void Analyze_FileScopedWhenPreferred_ReturnsNoDiagnostics()
    {
        string source = "namespace Foo;";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_namespace_declarations"] = "file_scoped",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_BlockScopedWhenFileScopedPreferred_ReturnsDiagnostic()
    {
        string source = "namespace Foo { }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_namespace_declarations"] = "file_scoped",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT203", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_FileScopedWhenBlockPreferred_ReturnsDiagnostic()
    {
        string source = "namespace Foo;";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_namespace_declarations"] = "block_scoped",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }
}
