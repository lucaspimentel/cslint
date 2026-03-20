using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class AccessorOrderingRuleTests
{
    private readonly AccessorOrderingRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_accessor_ordering"] = "true",
        });

    [Fact]
    public void Analyze_PropertyGetBeforeSet_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                int X { get { return 1; } set { } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_PropertySetBeforeGet_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                int X { set { } get { return 1; } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT263", diagnostics[0].RuleId);
        Assert.Contains("get", diagnostics[0].Message);
        Assert.Contains("set", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_PropertyInitBeforeGet_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                int X { init { } get { return 1; } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT263", diagnostics[0].RuleId);
        Assert.Contains("init", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_PropertyGetOnly_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                int X { get { return 1; } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AutoPropertyGetSet_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                int X { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_IndexerSetBeforeGet_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                int this[int i] { set { } get { return 1; } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT263", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_EventAddBeforeRemove_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class Foo
            {
                event EventHandler E { add { } remove { } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_EventRemoveBeforeAdd_ReturnsDiagnostic()
    {
        string source = """
            using System;
            class Foo
            {
                event EventHandler E { remove { } add { } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT263", diagnostics[0].RuleId);
        Assert.Contains("add", diagnostics[0].Message);
        Assert.Contains("remove", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            class Foo
            {
                int X { set { } get { return 1; } }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_accessor_ordering"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
