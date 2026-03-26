using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class ThrowInFinallyRuleTests
{
    private readonly ThrowInFinallyRule _rule = new();

    [Fact]
    public void Analyze_ThrowInFinally_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    finally
                    {
                        throw new Exception();
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2219", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_BareThrowInFinally_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    catch (Exception)
                    {
                        try { }
                        finally
                        {
                            throw;
                        }
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2219", diagnostic.RuleId);
    }

    [Fact]
    public void Analyze_ThrowInNestedTryInsideFinally_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    finally
                    {
                        try
                        {
                            throw new Exception();
                        }
                        catch { }
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThrowInCatch_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    catch (Exception)
                    {
                        throw new Exception();
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThrowInTryBody_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try
                    {
                        throw new Exception();
                    }
                    finally { }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    finally { throw new Exception(); }
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2219.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
