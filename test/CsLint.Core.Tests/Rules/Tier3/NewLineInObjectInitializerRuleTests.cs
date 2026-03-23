using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NewLineInObjectInitializerRuleTests
{
    private readonly NewLineInObjectInitializerRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_new_line_before_members_in_object_initializers"] = "true",
        });

    [Fact]
    public void Analyze_MembersOnSameLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    var x = new C { A = 1, B = 2 };
                }
                int A { get; set; }
                int B { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
        Assert.Equal("CSLINT283", _rule.Analyze(context)[0].RuleId);
    }

    [Fact]
    public void Analyze_MembersOnSeparateLines_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    var x = new C
                    {
                        A = 1,
                        B = 2,
                    };
                }
                int A { get; set; }
                int B { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SingleMember_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M()
                {
                    var x = new C { A = 1 };
                }
                int A { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M() { var x = new C { A = 1, B = 2 }; }
                int A { get; set; }
                int B { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
