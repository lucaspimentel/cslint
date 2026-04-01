using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class IdentifierContainsTypeNameRuleTests
{
    private readonly IdentifierContainsTypeNameRule _rule = new();

    private static readonly LintConfiguration EnabledConfig = new(new Dictionary<string, string>
    {
        ["dotnet_diagnostic.CA1720.severity"] = "warning",
    });

    [Theory]
    [InlineData("public void M(int int32) { }")]
    [InlineData("public void M(string boolean) { }")]
    [InlineData("public void M(object single) { }")]
    public void Analyze_ParamNamedAfterType_ReturnsDiagnostic(string method)
    {
        string source = $$"""
            namespace N;

            class C
            {
                {{method}}
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1720", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("public void M(int value) { }")]
    [InlineData("public void M(string name) { }")]
    public void Analyze_NormalParamName_NoDiagnostic(string method)
    {
        string source = $$"""
            namespace N;

            class C
            {
                {{method}}
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_PrivateMethod_NoDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                private void M(int int32) { }
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
                public void M(int int32) { }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1720.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
