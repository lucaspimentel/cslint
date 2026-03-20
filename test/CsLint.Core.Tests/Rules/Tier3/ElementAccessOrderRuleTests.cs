using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ElementAccessOrderRuleTests
{
    private readonly ElementAccessOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_element_access_ordering"] = "true",
        });

    [Fact]
    public void Analyze_PublicThenPrivate_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                public void A() { }
                private void B() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PrivateThenPublic_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                private void B() { }
                public void A() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT267", diagnostics[0].RuleId);
        Assert.Contains("public", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_DifferentKindsDontInterfere_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private int _x;
                public void A() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Fields and methods are different kinds
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ProtectedInternalOrdering_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                protected void A() { }
                protected internal void B() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // protected internal is more visible than protected, so B should come before A
        Assert.Single(diagnostics);
        Assert.Equal("CSLINT267", diagnostics[0].RuleId);
        Assert.Contains("protected internal", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_FullAccessOrderCorrect_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                public void A() { }
                internal void B() { }
                protected internal void C() { }
                protected void D() { }
                private void E() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ImplicitPrivateAfterPrivate_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private void A() { }
                void B() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

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
                private void B() { }
                public void A() { }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_element_access_ordering"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
