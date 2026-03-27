using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NoYodaConditionsRuleTests
{
    private readonly NoYodaConditionsRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_yoda_conditions"] = "true",
        });

    [Theory]
    [InlineData("0 == x")]
    [InlineData("null == obj")]
    [InlineData("\"abc\" == s")]
    [InlineData("true == flag")]
    [InlineData("5 != count")]
    [InlineData("0 < x")]
    [InlineData("10 >= y")]
    public void Analyze_YodaCondition_ReturnsDiagnostic(string condition)
    {
        string source = $$"""
            class C
            {
                void M(int x, object obj, string s, bool flag, int count, int y)
                {
                    if ({{condition}}) { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1131", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("x == 0")]
    [InlineData("obj == null")]
    [InlineData("s == \"abc\"")]
    [InlineData("flag == true")]
    [InlineData("x == y")]
    [InlineData("1 == 1")]
    public void Analyze_NormalCondition_ReturnsNoDiagnostics(string condition)
    {
        string source = $$"""
            class C
            {
                void M(int x, object obj, string s, bool flag, int y)
                {
                    if ({{condition}}) { }
                }
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
            class C
            {
                void M(int x)
                {
                    if (0 == x) { }
                }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_yoda_conditions"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
