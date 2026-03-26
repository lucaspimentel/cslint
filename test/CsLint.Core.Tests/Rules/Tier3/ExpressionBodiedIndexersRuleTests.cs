using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedIndexersRuleTests
{
    private readonly ExpressionBodiedIndexersRule _rule = new();

    [Fact]
    public void Analyze_ExpressionBodiedIndexer_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private int[] _values = new int[10];
                public int this[int i] => _values[i];
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_indexers"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_BlockBodyIndexer_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int[] _values = new int[10];
                public int this[int i] { get { return _values[i]; } }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_indexers"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0026", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_IndexerWithGetAndSet_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private int[] _values = new int[10];
                public int this[int i] { get { return _values[i]; } set { _values[i] = value; } }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_indexers"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigNotSet_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private int[] _values = new int[10];
                public int this[int i] { get { return _values[i]; } }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>());
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
