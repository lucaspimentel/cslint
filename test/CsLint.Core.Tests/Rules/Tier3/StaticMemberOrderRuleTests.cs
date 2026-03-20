using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class StaticMemberOrderRuleTests
{
    private readonly StaticMemberOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_static_before_instance"] = "true",
        });

    [Fact]
    public void Analyze_StaticFieldBeforeInstanceField_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private static int _s;
                private int _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_InstanceFieldBeforeStaticField_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                private int _x;
                private static int _s;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT266", diagnostics[0].RuleId);
        Assert.Contains("Static", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_StaticMethodAfterInstanceField_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private int _x;
                public static void DoSomething() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Different kind groups — no violation
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_InstanceMethodBeforeStaticMethod_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                public void InstanceMethod() { }
                public static void StaticMethod() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT266", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ConstFieldsIgnored_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private int _x;
                private const int C = 1;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // const is implicitly static, but should be skipped by this rule
        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            class Foo
            {
                private int _x;
                private static int _s;
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_static_before_instance"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
