using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class RemoveUnnecessaryParenthesesRuleTests
{
    private readonly RemoveUnnecessaryParenthesesRule _rule = new();

    private static LintConfiguration Config(string key, string value) =>
        new(new Dictionary<string, string> { [key] = value });

    private static LintConfiguration ArithmeticConfig(string value) =>
        Config("dotnet_style_parentheses_in_arithmetic_binary_operators", value);

    private static LintConfiguration OtherBinaryConfig(string value) =>
        Config("dotnet_style_parentheses_in_other_binary_operators", value);

    [Fact]
    public void HigherPrecedenceInLowerContext_Flagged()
    {
        // (a * b) + c — parens around * are unnecessary since * > +
        string source = """
            class C
            {
                int M(int a, int b, int c) => (a * b) + c;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0047", diagnostics[0].RuleId);
    }

    [Fact]
    public void HigherPrecedenceOnRight_Flagged()
    {
        // a + (b * c) — parens around * are unnecessary since * > +
        string source = """
            class C
            {
                int M(int a, int b, int c) => a + (b * c);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void LowerPrecedenceInHigherContext_NotFlagged()
    {
        // (a + b) * c — removing parens changes meaning
        string source = """
            class C
            {
                int M(int a, int b, int c) => (a + b) * c;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void SamePrecedenceLeftOperand_Flagged()
    {
        // (a + b) + c — same precedence, left-associative, left operand doesn't need parens
        string source = """
            class C
            {
                int M(int a, int b, int c) => (a + b) + c;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void SamePrecedenceRightOperand_NotFlagged()
    {
        // a + (b + c) — same precedence, left-associative, right operand needs parens to preserve semantics
        // (integer arithmetic is not truly associative due to overflow)
        string source = """
            class C
            {
                int M(int a, int b, int c) => a + (b + c);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void LogicalAndInLogicalOr_Flagged()
    {
        // (a && b) || c — && has higher precedence than ||
        string source = """
            class C
            {
                bool M(bool a, bool b, bool c) => (a && b) || c;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, OtherBinaryConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void ParensInReturnStatement_Flagged()
    {
        // return (a + b) — unnecessary in non-binary parent context
        string source = """
            class C
            {
                int M(int a, int b)
                {
                    return (a + b);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void ParensInAssignment_Flagged()
    {
        // x = (a * b) — unnecessary in assignment context
        string source = """
            class C
            {
                void M(int a, int b)
                {
                    int x = (a * b);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void ConfigAbsent_NotFlagged()
    {
        string source = """
            class C
            {
                int M(int a, int b, int c) => (a * b) + c;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ConfigAlwaysForClarity_NotFlagged()
    {
        string source = """
            class C
            {
                int M(int a, int b, int c) => (a * b) + c;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("always_for_clarity"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void NonBinaryInnerExpression_NotFlagged()
    {
        // (x) + y — inner is not a binary expression
        string source = """
            class C
            {
                int M(int x, int y) => (x) + y;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, ArithmeticConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("never_if_unnecessary")]
    [InlineData("always_for_clarity")]
    public void IsEnabled_WhenConfigPresent_ReturnsTrue(string value)
    {
        LintConfiguration config = ArithmeticConfig(value);

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_WhenConfigAbsent_ReturnsFalse()
    {
        Assert.False(_rule.IsEnabled(LintConfiguration.Empty));
    }

    [Fact]
    public void CoalesceRightAssociative_RightOperandFlagged()
    {
        // a ?? (b ?? c) — ?? is right-associative, right operand doesn't need parens
        string source = """
            class C
            {
                string M(string? a, string? b, string c) => a ?? (b ?? c);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, OtherBinaryConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void CoalesceRightAssociative_LeftOperandNotFlagged()
    {
        // (a ?? b) ?? c — ?? is right-associative, left operand might change grouping
        string source = """
            class C
            {
                string M(string? a, string? b, string c) => (a ?? b) ?? c;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, OtherBinaryConfig("never_if_unnecessary"));

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
