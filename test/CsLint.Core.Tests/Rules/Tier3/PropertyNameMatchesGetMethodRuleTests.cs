using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class PropertyNameMatchesGetMethodRuleTests
{
    private readonly PropertyNameMatchesGetMethodRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1721.severity"] = "warning",
    });

    [Fact]
    public void Analyze_GetMethodMatchesProperty_ReturnsDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public int Value { get; }
                public int GetValue() => 0;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA1721", diagnostic.RuleId);
        Assert.Contains("GetValue", diagnostic.Message);
        Assert.Contains("Value", diagnostic.Message);
    }

    [Fact]
    public void Analyze_GetMethodNoMatchingProperty_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                public int Value { get; }
                public int GetOther() => 0;
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_PrivateMembers_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                private int Value { get; }
                private int GetValue() => 0;
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
                public int Value { get; }
                public int GetValue() => 0;
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1721.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
