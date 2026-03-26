using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ExpressionBodiedAccessorsRuleTests
{
    private readonly ExpressionBodiedAccessorsRule _rule = new();

    [Fact]
    public void Analyze_ExpressionBodiedAccessors_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private int _age;
                public int Age { get => _age; set => _age = value; }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_BlockBodyGet_WhenExpressionPreferred_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int _age;
                public int Age { get { return _age; } set { _age = value; } }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, d => Assert.Equal("IDE0027", d.RuleId));
    }

    [Fact]
    public void Analyze_MultiStatementAccessor_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private int _age;
                public int Age { get { var x = _age; return x; } }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_style_expression_bodied_accessors"] = "true",
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
                private int _age;
                public int Age { get { return _age; } }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>());
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IndexerAccessors_ReturnsDiagnostic()
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
            ["csharp_style_expression_bodied_accessors"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, d => Assert.Equal("IDE0027", d.RuleId));
    }
}
