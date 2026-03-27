using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ReadonlyFieldOrderRuleTests
{
    private readonly ReadonlyFieldOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_readonly_before_mutable"] = "true",
        });

    [Fact]
    public void Analyze_ReadonlyBeforeMutable_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private readonly int _x;
                private int _y;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ReadonlyAfterMutable_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                private int _y;
                private readonly int _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1214", diagnostics[0].RuleId);
        Assert.Contains("Readonly", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ConstFieldsIgnored_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private const int C = 1;
                private int _y;
                private readonly int _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // readonly after mutable is still flagged, but const doesn't affect ordering
        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_StaticFieldsIgnored_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private static int _s;
                private static readonly int _sr = 1;
                private readonly int _x;
                private int _y;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Static fields are skipped; readonly instance before mutable instance is fine
        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleTypes_ChecksEachSeparately()
    {
        string source = """
            class Foo
            {
                private readonly int _x;
                private int _y;
            }
            class Bar
            {
                private int _a;
                private readonly int _b;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1214", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            class Foo
            {
                private int _y;
                private readonly int _x;
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_readonly_before_mutable"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
