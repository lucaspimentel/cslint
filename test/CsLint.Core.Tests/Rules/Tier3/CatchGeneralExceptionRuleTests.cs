using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class CatchGeneralExceptionRuleTests
{
    private readonly CatchGeneralExceptionRule _rule = new();

    [Theory]
    [InlineData("catch (Exception) { }")]
    [InlineData("catch (Exception ex) { }")]
    [InlineData("catch (System.Exception) { }")]
    public void Analyze_CatchGeneralException_ReturnsDiagnostic(
        string catchBlock)
    {
        string source = $$"""
            using System;
            namespace N;

            class C
            {
                void M()
                {
                    try { }
                    {{catchBlock}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1031", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BareCatch_ReturnsDiagnostic()
    {
        const string source = """
            namespace N;

            class C
            {
                void M()
                {
                    try { }
                    catch { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Single(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("catch (InvalidOperationException) { }")]
    [InlineData("catch (ArgumentNullException ex) { }")]
    public void Analyze_CatchSpecificException_NoDiagnostic(
        string catchBlock)
    {
        string source = $$"""
            using System;
            namespace N;

            class C
            {
                void M()
                {
                    try { }
                    {{catchBlock}}
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
            using System;
            namespace N;

            class C
            {
                void M()
                {
                    try { }
                    catch (Exception) { }
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA1031.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }
}
