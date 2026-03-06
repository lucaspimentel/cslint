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
}
