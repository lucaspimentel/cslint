using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class FileHeaderRuleTests
{
    private readonly FileHeaderRule _rule = new();

    [Fact]
    public void Analyze_MissingHeader_ReturnsDiagnostic()
    {
        string source = "class C { }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["file_header_template"] = "Copyright (c) Contoso",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT007", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_WrongHeader_ReturnsDiagnostic()
    {
        string source = """
            // Copyright (c) Wrong Company
            class C { }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["file_header_template"] = "Copyright (c) Contoso",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
    }

    [Fact]
    public void Analyze_CorrectHeader_ReturnsNoDiagnostics()
    {
        string source = """
            // Copyright (c) Contoso
            class C { }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["file_header_template"] = "Copyright (c) Contoso",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_TemplateUnset_ReturnsNoDiagnostics()
    {
        string source = "class C { }";
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["file_header_template"] = "unset",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = "class C { }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultiLineTemplate_ReturnsNoDiagnostics()
    {
        string source = """
            // Copyright (c) Contoso
            // Licensed under MIT
            class C { }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["file_header_template"] = "Copyright (c) Contoso\nLicensed under MIT",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultiLineTemplate_SecondLineMismatch_ReturnsDiagnostic()
    {
        string source = """
            // Copyright (c) Contoso
            // Licensed under Apache
            class C { }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["file_header_template"] = "Copyright (c) Contoso\nLicensed under MIT",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal(2, diagnostics[0].Line);
    }
}
