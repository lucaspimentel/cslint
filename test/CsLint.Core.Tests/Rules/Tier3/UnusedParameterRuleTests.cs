using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class UnusedParameterRuleTests
{
    private readonly UnusedParameterRule _rule = new();

    private static LintConfiguration NonVirtualConfig() =>
        new(new Dictionary<string, string> { ["dotnet_code_quality_unused_parameters"] = "non_virtual" });

    private static LintConfiguration AllConfig() =>
        new(new Dictionary<string, string> { ["dotnet_code_quality_unused_parameters"] = "all" });

    [Fact]
    public void UnusedParameter_Flagged()
    {
        string source = """
            class C
            {
                void M(int x) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0060", diagnostics[0].RuleId);
        Assert.Contains("x", diagnostics[0].Message);
    }

    [Fact]
    public void UsedParameter_NotFlagged()
    {
        string source = """
            class C
            {
                int M(int x) => x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void DiscardParameter_NotFlagged()
    {
        string source = """
            class C
            {
                void M(int _) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void OverrideMethod_NonVirtualMode_NotFlagged()
    {
        string source = """
            class C
            {
                public override string ToString() => "";
                public override bool Equals(object obj) => true;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void VirtualMethod_NonVirtualMode_NotFlagged()
    {
        string source = """
            class C
            {
                public virtual void M(int x) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void OverrideMethod_AllMode_Flagged()
    {
        string source = """
            class C
            {
                public override bool Equals(object obj) => true;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, AllConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("obj", diagnostics[0].Message);
    }

    [Fact]
    public void AbstractMethod_NotFlagged()
    {
        string source = """
            abstract class C
            {
                public abstract void M(int x);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, AllConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void InterfaceMethod_NotFlagged()
    {
        string source = """
            interface I
            {
                void M(int x);
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, AllConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Constructor_UnusedParam_Flagged()
    {
        string source = """
            class C
            {
                public C(int x) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Constructor_ParamUsedInInitializer_NotFlagged()
    {
        string source = """
            class C
            {
                public C(int x) : this(x, 0) { }
                public C(int x, int y) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        // First constructor: x is used in `: this(x, 0)` → not flagged
        // Second constructor: both x and y unused → flagged
        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }

    [Fact]
    public void LocalFunction_UnusedParam_Flagged()
    {
        string source = """
            class C
            {
                void M()
                {
                    void Local(int x) { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // M has no params, Local has unused x
        Assert.Single(diagnostics);
        Assert.Contains("x", diagnostics[0].Message);
    }

    [Fact]
    public void MainMethod_NotFlagged()
    {
        string source = """
            class Program
            {
                static void Main(string[] args) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ConfigAbsent_NotFlagged()
    {
        string source = """
            class C
            {
                void M(int x) { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void ConfigInvalidValue_NotFlagged()
    {
        string source = """
            class C
            {
                void M(int x) { }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_code_quality_unused_parameters"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("all")]
    [InlineData("non_virtual")]
    public void IsEnabled_WhenConfigPresent_ReturnsTrue(string value)
    {
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_code_quality_unused_parameters"] = value,
        });

        Assert.True(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_WhenConfigAbsent_ReturnsFalse()
    {
        Assert.False(_rule.IsEnabled(LintConfiguration.Empty));
    }

    [Fact]
    public void MultipleParams_OnlyUnusedFlagged()
    {
        string source = """
            class C
            {
                int M(int x, int y, int z) => x + z;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("y", diagnostics[0].Message);
    }

    [Fact]
    public void ExpressionBodiedMethod_ParamUsed_NotFlagged()
    {
        string source = """
            class C
            {
                string M(string s) => s.ToUpper();
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, NonVirtualConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
