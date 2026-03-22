using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Moq;

namespace Cslint.Core.Tests.Engine;

public sealed class FileLinterTests
{
    [Fact]
    public void LintSource_WithEnabledRule_ReturnsLintDiagnostics()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        mockProvider.Setup(p => p.GetConfiguration(It.IsAny<string>())).Returns(config);

        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.NotEmpty(diagnostics);
    }

    [Fact]
    public void LintSource_WithDisabledRule_ReturnsNoLintDiagnostics()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "false" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        mockProvider.Setup(p => p.GetConfiguration(It.IsAny<string>())).Returns(config);

        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void LintSource_CleanFile_ReturnsNoLintDiagnostics()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }\n", config);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void LintSource_PragmaDisable_SuppressesDiagnosticInRange()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);

        const string Source = "#pragma warning disable CSLINT001\nclass Foo { }   \n";
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", Source, config);

        Assert.DoesNotContain(diagnostics, d => d.RuleId == "CSLINT001");
    }

    [Fact]
    public void LintSource_PragmaDisableRestore_DiagnosticOutsideRangeStillReported()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);

        const string Source = "#pragma warning disable CSLINT001\n#pragma warning restore CSLINT001\nclass Foo { }   \n";
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", Source, config);

        Assert.Contains(diagnostics, d => d.RuleId == "CSLINT001");
    }

    [Fact]
    public void LintSource_SeverityOverrideError_ProducesErrorDiagnostics()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true:error" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, d => Assert.Equal(LintSeverity.Error, d.Severity));
    }

    [Fact]
    public void LintSource_SeverityNone_SuppressesDiagnostics()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true:none" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.DoesNotContain(diagnostics, d => d.RuleId == "CSLINT001");
    }

    [Fact]
    public void LintSource_SeveritySilent_SuppressesDiagnostics()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true:silent" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.DoesNotContain(diagnostics, d => d.RuleId == "CSLINT001");
    }

    [Fact]
    public void LintSource_NoSeveritySuffix_UsesDefaultSeverity()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.NotEmpty(diagnostics);
        Assert.All(diagnostics, d => Assert.Equal(LintSeverity.Warning, d.Severity));
    }

    [Fact]
    public void LintSource_FalseWithSeveritySuffix_RuleIsDisabled()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "false:error" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.DoesNotContain(diagnostics, d => d.RuleId == "CSLINT001");
    }

    [Fact]
    public void LintSource_TrueWithSeveritySuffix_RuleIsEnabled()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["trim_trailing_whitespace"] = "true:warning" });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object);
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.NotEmpty(diagnostics);
        Assert.Contains(diagnostics, d => d.RuleId == "CSLINT001");
    }

    [Fact]
    public void LintSource_RuleFilter_OnlyRunsSpecifiedRules()
    {
        // Enable two rules via config, but filter to only one
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["trim_trailing_whitespace"] = "true",
                ["insert_final_newline"] = "true",
            });

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object)
        {
            RuleFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CSLINT001" },
            SkipEnabledCheck = true,
        };

        // Source has trailing whitespace (CSLINT001) and no final newline (CSLINT002)
        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   ", config);

        Assert.Contains(diagnostics, d => d.RuleId == "CSLINT001");
        Assert.DoesNotContain(diagnostics, d => d.RuleId == "CSLINT002");
    }

    [Fact]
    public void LintSource_SkipEnabledCheck_RunsRuleEvenWhenConfigDisabled()
    {
        // Config does not enable the rule
        var config = new LintConfiguration(new Dictionary<string, string>());

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object)
        {
            RuleFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "CSLINT001" },
            SkipEnabledCheck = true,
        };

        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        Assert.Contains(diagnostics, d => d.RuleId == "CSLINT001");
    }

    [Fact]
    public void LintSource_SkipEnabledCheckWithoutFilter_RunsAllRules()
    {
        // Empty config — normally no Tier1 rules would be enabled
        var config = new LintConfiguration(new Dictionary<string, string>());

        RuleRegistry registry = RuleRegistry.CreateDefault();
        var mockProvider = new Mock<IConfigProvider>();
        var linter = new FileLinter(registry, mockProvider.Object)
        {
            SkipEnabledCheck = true,
        };

        IReadOnlyList<LintDiagnostic> diagnostics = linter.LintSource("test.cs", "class Foo { }   \n", config);

        // With all rules force-enabled, we should get trailing whitespace at minimum
        Assert.Contains(diagnostics, d => d.RuleId == "CSLINT001");
    }
}
