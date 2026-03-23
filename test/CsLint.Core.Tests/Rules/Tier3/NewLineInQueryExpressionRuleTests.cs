using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NewLineInQueryExpressionRuleTests
{
    private readonly NewLineInQueryExpressionRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_new_line_between_query_expression_clauses"] = "true",
        });

    [Fact]
    public void Analyze_ClausesOnSameLine_ReturnsDiagnostic()
    {
        string source = """
            using System.Linq;
            class C
            {
                void M()
                {
                    var q = from x in new[] { 1, 2 } where x > 0 select x;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.NotEmpty(diagnostics);
        Assert.Equal("CSLINT285", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ClausesOnSeparateLines_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Linq;
            class C
            {
                void M()
                {
                    var q = from x in new[] { 1, 2 }
                            where x > 0
                            select x;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            using System.Linq;
            class C
            {
                void M()
                {
                    var q = from x in new[] { 1 } where x > 0 select x;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
