using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NumericPlaceholderRuleTests
{
    private readonly NumericPlaceholderRule _rule = new();

    [Theory]
    [InlineData("logger.LogInformation(\"{0} processed\", item);")]
    [InlineData("logger.LogWarning(\"{0} failed with {1}\", a, b);")]
    [InlineData("logger.LogError(\"Error {0:D}\", code);")]
    public void Analyze_NumericPlaceholder_ReturnsDiagnostic(string call)
    {
        string source = $$"""
            namespace N;

            class C
            {
                void M(object logger, object item, object a,
                    object b, object code)
                {
                    {{call}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2253", diagnostic.RuleId);
    }

    [Theory]
    [InlineData("logger.LogInformation(\"{Item} processed\", item);")]
    [InlineData("logger.LogWarning(\"No placeholders here\");")]
    [InlineData("logger.LogDebug(\"Escaped {{0}} brace\");")]
    public void Analyze_NamedOrNoPlaceholder_NoDiagnostic(string call)
    {
        string source = $$"""
            namespace N;

            class C
            {
                void M(object logger, object item)
                {
                    {{call}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
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
                    string.Format("{0} items", 5);
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                void M(object logger, object item)
                {
                    logger.LogInformation("{0}", item);
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2253.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
