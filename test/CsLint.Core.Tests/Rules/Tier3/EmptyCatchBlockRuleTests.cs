using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class EmptyCatchBlockRuleTests
{
    private readonly EmptyCatchBlockRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_no_empty_catch_blocks"] = "true",
        });

    [Theory]
    [InlineData("catch (Exception) { }")]
    [InlineData("catch (Exception ex) { }")]
    [InlineData("catch (Exception)\n            {\n            }")]
    public void Analyze_EmptyCatchWithType_ReturnsDiagnostic(string catchBlock)
    {
        string source = $$"""
            using System;
            class Foo
            {
                void M()
                {
                    try { }
                    {{catchBlock}}
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT305", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_BareEmptyCatch_ReturnsDiagnostic()
    {
        string source = """
            class Foo
            {
                void M()
                {
                    try { }
                    catch { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT305", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_CatchWithStatement_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class Foo
            {
                void M()
                {
                    try { }
                    catch (Exception) { Console.WriteLine("error"); }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_CatchWithRethrow_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            class Foo
            {
                void M()
                {
                    try { }
                    catch (Exception) { throw; }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleCatches_OnlyEmptyOneFlagged()
    {
        string source = """
            using System;
            class Foo
            {
                void M()
                {
                    try { }
                    catch (InvalidOperationException) { Console.WriteLine("handled"); }
                    catch (Exception) { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT305", diagnostics[0].RuleId);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            class Foo
            {
                void M()
                {
                    try { }
                    catch { }
                }
            }
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_no_empty_catch_blocks"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
