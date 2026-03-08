using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class InferredMemberNameRuleTests
{
    private readonly InferredMemberNameRule _rule = new();

    [Fact]
    public void Analyze_AnonymousType_RedundantName_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    var obj = new { x = x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT234", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_Tuple_RedundantName_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    int name = 1;
                    int age = 2;
                    var t = (name: name, age: age);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, d => Assert.Equal("CSLINT234", d.RuleId));
    }

    [Fact]
    public void Analyze_AnonymousType_DifferentName_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int y = 1;
                    var obj = new { x = y };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AnonymousType_MemberAccess_ReturnsNoDiagnostics()
    {
        string source = """
            class Obj { public int x; }
            class C
            {
                void M()
                {
                    var obj = new Obj();
                    var anon = new { x = obj.x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AnonymousType_CaseDiffers_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    var obj = new { X = x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "true",
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
                    int x = 1;
                    var obj = new { x = x };
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_style_prefer_inferred_anonymous_type_member_names"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    int x = 1;
                    var obj = new { x = x };
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
