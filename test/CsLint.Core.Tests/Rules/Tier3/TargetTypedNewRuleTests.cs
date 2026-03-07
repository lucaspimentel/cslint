using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class TargetTypedNewRuleTests
{
    private readonly TargetTypedNewRule _rule = new();

    [Fact]
    public void Analyze_ExplicitTypeWithMatchingNew_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_implicit_object_creation_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT212", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_VarDeclaration_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    var list = new System.Collections.Generic.List<int>();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_implicit_object_creation_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AlreadyTargetTyped_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    System.Collections.Generic.List<int> list = new();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_implicit_object_creation_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_implicit_object_creation_when_type_is_apparent"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_FieldDeclaration_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private System.Collections.Generic.List<int> _list = new System.Collections.Generic.List<int>();
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_implicit_object_creation_when_type_is_apparent"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT212", diagnostics[0].RuleId);
    }
}
