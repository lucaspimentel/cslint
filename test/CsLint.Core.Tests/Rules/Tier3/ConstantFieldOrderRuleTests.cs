using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ConstantFieldOrderRuleTests
{
    private readonly ConstantFieldOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_constants_before_fields"] = "true",
        });

    [Fact]
    public void Analyze_ConstBeforeField_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private const int C = 1;
                private int _x;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConstAfterField_ReturnsDiagnostic()
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

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT265", diagnostics[0].RuleId);
        Assert.Contains("Constant", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_MixedWithStaticAndReadonly_FlagsConstAfterField()
    {
        string source = """
            class Foo
            {
                private const int A = 1;
                private static int _s;
                private readonly int _r;
                private const int B = 2;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT265", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_OnlyConsts_ReturnsNoDiagnostics()
    {
        string source = """
            class Foo
            {
                private const int A = 1;
                private const int B = 2;
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
                private int _x;
                private const int C = 1;
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_constants_before_fields"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
