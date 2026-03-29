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
        Assert.Equal("CA1821", diagnostics[0].RuleId);
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

    [Fact]
    public void Analyze_EnabledByDefault_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                ~Foo() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_DisabledViaCustomKey_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                ~Foo() { }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_no_empty_finalizers"] = "false",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_DisabledViaDiagnosticKey_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                ~Foo() { }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1821.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
