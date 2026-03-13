using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EmptyFinalizerRuleTests
{
    private readonly EmptyFinalizerRule _rule = new();

    [Theory]
    [InlineData("~Foo() { }")]
    [InlineData("~Foo() {\n}")]
    [InlineData("~Foo()\n    {\n    }")]
    public void Analyze_EmptyBody_ReturnsDiagnostic(string destructor)
    {
        string source = $$"""
            class Foo
            {
                {{destructor}}
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_empty_finalizers"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT237", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BodyWithStatement_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                ~Foo() { Dispose(); }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_empty_finalizers"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ExpressionBody_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                ~Foo() => Dispose();
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_empty_finalizers"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NoFinalizer_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                void M() { }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_empty_finalizers"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

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
                ~Foo() { }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_empty_finalizers"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
