using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ZeroLengthArrayRuleTests
{
    private readonly ZeroLengthArrayRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1825.severity"] = "warning",
    });

    [Theory]
    [InlineData("var a = new int[0];")]
    [InlineData("var a = new string[0];")]
    [InlineData("var a = new object[0];")]
    public void Analyze_ZeroLengthArray_ReturnsDiagnostic(string statement)
    {
        string source = $$"""
            namespace N;

            class C
            {
                void M()
                {
                    {{statement}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1825", diagnostic.RuleId);
        Assert.Contains("Array.Empty", diagnostic.Message);
    }

    [Theory]
    [InlineData("var a = new int[1];")]
    [InlineData("var a = new int[10];")]
    [InlineData("var a = new int[] { 1, 2, 3 };")]
    [InlineData("var a = new[] { 1, 2, 3 };")]
    public void Analyze_NonZeroArray_NoDiagnostic(string statement)
    {
        string source = $$"""
            namespace N;

            class C
            {
                void M()
                {
                    {{statement}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

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
                void M() { var a = new int[0]; }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1825.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
