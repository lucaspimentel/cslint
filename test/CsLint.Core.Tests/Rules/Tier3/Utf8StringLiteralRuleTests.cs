using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class Utf8StringLiteralRuleTests
{
    private readonly Utf8StringLiteralRule _rule = new();

    [Fact]
    public void Analyze_EncodingUtf8GetBytes_ReturnsDiagnostic()
    {
        string source = """
            using System.Text;
            class C
            {
                void M()
                {
                    var bytes = Encoding.UTF8.GetBytes("hello");
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_utf8_string_literals"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT224", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_GetBytesWithVariable_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Text;
            class C
            {
                void M(string s)
                {
                    var bytes = Encoding.UTF8.GetBytes(s);
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_utf8_string_literals"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Text;
            class C
            {
                void M()
                {
                    var bytes = Encoding.UTF8.GetBytes("hello");
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_prefer_utf8_string_literals"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
