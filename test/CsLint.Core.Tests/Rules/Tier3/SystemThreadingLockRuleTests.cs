using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SystemThreadingLockRuleTests
{
    private readonly SystemThreadingLockRule _rule = new();

    [Fact]
    public void Analyze_LockOnObjectField_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private readonly object _sync = new object();
                void M()
                {
                    lock (_sync) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0330", diagnostics[0].RuleId);
        Assert.Contains("_sync", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_LockOnThis_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    lock (this) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("this", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_LockOnTypeof_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                void M()
                {
                    lock (typeof(C)) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("Type object", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_LockOnThisDotObjectField_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private readonly object _sync = new object();
                void M()
                {
                    lock (this._sync) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Contains("_sync", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_LockOnNonObjectField_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private readonly string _sync = "";
                void M()
                {
                    lock (_sync) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_LockOnUnknownIdentifier_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                void M(object sync)
                {
                    lock (sync) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        // Parameter, not a field — conservative: don't flag
        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("false")]
    [InlineData("FALSE")]
    public void Analyze_ConfigFalse_ReturnsNoDiagnostics(string value)
    {
        string source = """
            class C
            {
                private readonly object _sync = new object();
                void M()
                {
                    lock (_sync) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = value,
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigMissing_ReturnsNoDiagnostics()
    {
        string source = """
            class C
            {
                private readonly object _sync = new object();
                void M()
                {
                    lock (_sync) { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleLocks_ReportsAll()
    {
        string source = """
            class C
            {
                private readonly object _a = new object();
                private readonly object _b = new object();
                void M()
                {
                    lock (_a) { }
                    lock (_b) { }
                }
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["csharp_prefer_system_threading_lock"] = "true",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Equal(2, diagnostics.Count);
    }
}
