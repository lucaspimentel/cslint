using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PascalCasePlaceholderRuleTests
{
    private readonly PascalCasePlaceholderRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1727.severity"] = "warning",
    });

    [Theory]
    [InlineData("logger.LogInformation(\"{item} processed\", x);")]
    [InlineData("logger.LogWarning(\"{errorCode} failed\", x);")]
    public void Analyze_LowercasePlaceholder_ReturnsDiagnostic(string call)
    {
        string source = $$"""
            namespace N;

            class C
            {
                void M(object logger, object x)
                {
                    {{call}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1727", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("logger.LogInformation(\"{Item} processed\", x);")]
    [InlineData("logger.LogWarning(\"{ErrorCode} failed\", x);")]
    [InlineData("logger.LogDebug(\"No placeholders\");")]
    public void Analyze_PascalCaseOrNoPlaceholder_NoDiagnostic(string call)
    {
        string source = $$"""
            namespace N;

            class C
            {
                void M(object logger, object x)
                {
                    {{call}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NonLoggingMethod_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                void M()
                {
                    string.Format("{item}", "x");
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                void M(object logger, object x)
                {
                    logger.LogInformation("{item}", x);
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1727.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
