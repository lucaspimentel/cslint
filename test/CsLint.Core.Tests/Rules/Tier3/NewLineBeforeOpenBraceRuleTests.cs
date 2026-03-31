using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NewLineBeforeOpenBraceRuleTests
{
    private readonly NewLineBeforeOpenBraceRule _rule = new();

    [Fact]
    public void Analyze_AllValue_BraceOnSameLine_ReturnsDiagnostic()
    {
        string source = "class C {" + "\n" + "}";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "all",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT279", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_AllValue_BraceOnNewLine_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "all",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NoneValue_BraceOnNewLine_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.NotEmpty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_SpecificContext_MatchingBrace_ReturnsDiagnostic()
    {
        string source = "class C { void M() { if (true) {\n    int x = 1;\n} } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "control_blocks",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // control_blocks context should flag the if's brace
        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);
        Assert.Contains(diagnostics, d => d.Message.Contains("control_blocks"));
    }

    [Fact]
    public void Analyze_SpecificContext_NonMatchingBrace_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "control_blocks",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // methods context not in the list, should not flag
        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);
        Assert.DoesNotContain(diagnostics, d => d.Message.Contains("methods"));
    }

    [Fact]
    public void Analyze_CommaSeparatedContexts_FlagsBoth()
    {
        string source = "class C { void M() {\n    if (true) {\n        int x = 1;\n    }\n} }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "methods, control_blocks",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Contains(diagnostics, d => d.Message.Contains("methods"));
        Assert.Contains(diagnostics, d => d.Message.Contains("control_blocks"));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "class C { }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AllValue_SingleLineInitializer_ReturnsNoDiagnostics()
    {
        string source = """
            class C { void M() { string[] foo = new[] { "1", "2" }; } }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "all",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AllValue_MultiLineInitializerBraceInline_ReturnsNoDiagnostics()
    {
        string source = "class C { void M() { string[] foo = new[] { \"a\",\n\"b\" }; } }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "all",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // No newline between previous token and the brace — brace is inline, no diagnostic
        Assert.DoesNotContain(_rule.Analyze(context), d => d.Message.Contains("object_collection_array_initializers"));
    }

    [Fact]
    public void Analyze_AllValue_BraceOnNewLineWithContentOnNextLine_ReturnsNoDiagnostics()
    {
        string source = "class C\n{ }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "all",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // Newline is before the brace (correct Allman style) — no diagnostic
        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_AllValue_BraceOnNewLineWithMultiLineBody_ReturnsNoDiagnostics()
    {
        string source = "class C\n{\n}";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_new_line_before_open_brace"] = "all",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // Newline is before the brace (correct Allman style) — no diagnostic
        Assert.Empty(_rule.Analyze(context));
    }
}
