using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class UsingDirectiveOrderRuleTests
{
    private readonly UsingDirectiveOrderRule _rule = new();

    private static LintConfiguration EnabledConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_using_directive_ordering"] = "true",
        });

    [Fact]
    public void Analyze_CorrectlyOrdered_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Collections.Generic;
            using Microsoft.Extensions;
            using Newtonsoft.Json;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_SystemAfterNonSystem_ReturnsDiagnostic()
    {
        string source = """
            using Newtonsoft.Json;
            using System;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT269", diagnostics[0].RuleId);
        Assert.Contains("System", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_AlphabeticalWithinSystemGroup_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using System.Collections.Generic;
            using System.Linq;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NotAlphabeticalWithinSystemGroup_ReturnsDiagnostic()
    {
        string source = """
            using System.Linq;
            using System.Collections.Generic;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT269", diagnostics[0].RuleId);
        Assert.Contains("alphabetically", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_StaticAfterRegular_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_StaticBeforeRegular_ReturnsDiagnostic()
    {
        string source = """
            using static System.Math;
            using System;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.NotEmpty(diagnostics);
        Assert.Equal("CSLINT269", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_AliasUsingsLast_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Newtonsoft.Json;
            using static System.Math;
            using Foo = System.String;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AliasBeforeRegular_ReturnsDiagnostic()
    {
        string source = """
            using Foo = System.String;
            using System;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.NotEmpty(diagnostics);
        Assert.Equal("CSLINT269", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_StaticUsingsAlphabetical_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using static System.Console;
            using static System.Math;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_StaticUsingsNotAlphabetical_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using static System.Math;
            using static System.Console;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT269", diagnostics[0].RuleId);
        Assert.Contains("alphabetically", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_NonSystemAlphabetical_ReturnsNoDiagnostics()
    {
        string source = """
            using System;
            using Alpha.Beta;
            using Gamma.Delta;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NonSystemNotAlphabetical_ReturnsDiagnostic()
    {
        string source = """
            using System;
            using Gamma.Delta;
            using Alpha.Beta;
            """;
        RuleContext context = TestHelper.CreateContext(source, EnabledConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("alphabetically", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("false")]
    [InlineData(null)]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostics(string? configValue)
    {
        string source = """
            using Newtonsoft.Json;
            using System;
            """;
        var settings = new Dictionary<string, string>();

        if (configValue is not null)
        {
            settings["csharp_using_directive_ordering"] = configValue;
        }

        var config = new LintConfiguration(settings);
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
