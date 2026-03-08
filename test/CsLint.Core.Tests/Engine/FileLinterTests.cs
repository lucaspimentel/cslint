using Cslint.Core.Config;
using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Moq;

namespace Cslint.Core.Tests.Engine;

public class FileLinterTests
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
}
