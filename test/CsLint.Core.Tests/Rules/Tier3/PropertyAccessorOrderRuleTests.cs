using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PropertyAccessorOrderRuleTests
{
    private readonly PropertyAccessorOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string> { ["csharp_accessor_ordering"] = "true" });

    [Fact]
    public void PropertyGetBeforeSet_NoDiagnostics()
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
    public void PropertySetBeforeGet_Flagged()
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
        Assert.Equal("SA1212", diagnostics[0].RuleId);
        Assert.Contains("get", diagnostics[0].Message);
        Assert.Contains("set", diagnostics[0].Message);
    }

    [Fact]
    public void PropertyInitBeforeGet_Flagged()
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
        Assert.Equal("SA1212", diagnostics[0].RuleId);
        Assert.Contains("init", diagnostics[0].Message);
    }

    [Fact]
    public void PropertyGetOnly_NoDiagnostics()
    {
        string source = """
            class Foo
            {
                int X { get { return 1; } }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void AutoPropertyGetSet_NoDiagnostics()
    {
        string source = """
            class Foo
            {
                int X { get; set; }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void IndexerSetBeforeGet_Flagged()
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
        Assert.Equal("SA1212", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void RuleDisabled_NoDiagnostics(string? configValue)
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

        RuleContext context = TestHelper.CreateContext(source, new LintConfiguration(settings));

        Assert.Empty(_rule.Analyze(context));
    }
}
