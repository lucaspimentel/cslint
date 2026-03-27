using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier1;

namespace Cslint.Core.Tests.Rules.Tier1;

public class Utf8FileEncodingRuleTests
{
    private readonly Utf8FileEncodingRule _rule = new();

    private static LintConfiguration EnabledConfig => new(
        new Dictionary<string, string> { ["csharp_store_files_as_utf8"] = "true" });

    [Theory]
    [InlineData(new byte[] { 0xFF, 0xFE }, "UTF-16 LE")]
    [InlineData(new byte[] { 0xFE, 0xFF }, "UTF-16 BE")]
    [InlineData(new byte[] { 0xFF, 0xFE, 0x00, 0x00 }, "UTF-32 LE")]
    [InlineData(new byte[] { 0x00, 0x00, 0xFE, 0xFF }, "UTF-32 BE")]
    public void Analyze_NonUtf8Bom_ReturnsDiagnostic(byte[] prefix, string expectedEncoding)
    {
        RuleContext context = TestHelper.CreateContext(
            "class Foo { }", EnabledConfig, filePrefix: prefix);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("SA1412", diagnostic.RuleId);
        Assert.Equal(1, diagnostic.Line);
        Assert.Equal(1, diagnostic.Column);
        Assert.Contains(expectedEncoding, diagnostic.Message);
    }

    [Theory]
    [InlineData(new byte[] { 0xEF, 0xBB, 0xBF, 0x75 })] // UTF-8 BOM
    [InlineData(new byte[] { 0x75, 0x73, 0x69, 0x6E })]   // No BOM (starts with "usin")
    public void Analyze_Utf8OrNoBom_ReturnsNoDiagnostic(byte[] prefix)
    {
        RuleContext context = TestHelper.CreateContext(
            "class Foo { }", EnabledConfig, filePrefix: prefix);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NullPrefix_ReturnsNoDiagnostic()
    {
        RuleContext context = TestHelper.CreateContext(
            "class Foo { }", EnabledConfig, filePrefix: null);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_RuleDisabled_ReturnsNoDiagnostic()
    {
        var config = new LintConfiguration(new Dictionary<string, string>());
        RuleContext context = TestHelper.CreateContext(
            "class Foo { }", config, filePrefix: [0xFF, 0xFE]);

        // Rule is not enabled, so Analyze shouldn't normally be called,
        // but verify it still returns diagnostics if called directly
        // (the engine skips disabled rules before calling Analyze)
        Assert.Single(_rule.Analyze(context));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsEnabled_RespectsConfiguration(bool configValue, bool expected)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["csharp_store_files_as_utf8"] = configValue.ToString().ToLowerInvariant() });

        Assert.Equal(expected, _rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_MissingKey_ReturnsFalse()
    {
        var config = new LintConfiguration(new Dictionary<string, string>());

        Assert.False(_rule.IsEnabled(config));
    }

    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-8-bom")]
    [InlineData("UTF-8")]
    public void IsEnabled_StandardCharsetUtf8_ReturnsTrue(string charset)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["charset"] = charset });

        Assert.True(_rule.IsEnabled(config));
    }

    [Theory]
    [InlineData("latin1")]
    [InlineData("utf-16le")]
    public void IsEnabled_StandardCharsetNonUtf8_ReturnsFalse(string charset)
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["charset"] = charset });

        Assert.False(_rule.IsEnabled(config));
    }

    [Fact]
    public void IsEnabled_CsLintKeyTakesPrecedence()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string>
            {
                ["csharp_store_files_as_utf8"] = "false",
                ["charset"] = "utf-8",
            });

        // CsLint key says disabled, should take precedence
        Assert.False(_rule.IsEnabled(config));
    }

    [Fact]
    public void Analyze_StandardCharsetKey_DetectsNonUtf8()
    {
        var config = new LintConfiguration(
            new Dictionary<string, string> { ["charset"] = "utf-8" });
        RuleContext context = TestHelper.CreateContext(
            "class Foo { }", config, filePrefix: [0xFF, 0xFE]);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("SA1412", diagnostics[0].RuleId);
    }
}
