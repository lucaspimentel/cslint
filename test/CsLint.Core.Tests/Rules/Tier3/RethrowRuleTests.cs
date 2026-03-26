using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class RethrowRuleTests
{
    private readonly RethrowRule _rule = new();

    [Fact]
    public void Analyze_ThrowExInCatch_ReturnsDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CA2200", diagnostic.RuleId);
        Assert.Contains("throw", diagnostic.Message);
    }

    [Fact]
    public void Analyze_BareThrow_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThrowNewException_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    try { }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException("wrapped", ex);
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThrowDifferentVariable_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                Exception _saved;

                void M()
                {
                    try { }
                    catch (Exception ex)
                    {
                        throw _saved;
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ThrowOutsideCatch_NoDiagnostic()
    {
        const string source = """
            using System;

            class C
            {
                void M()
                {
                    Exception ex = new();
                    throw ex;
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
                    catch (Exception ex) { throw ex; }
                }
            }
            """;
        LintConfiguration config = new(new Dictionary<string, string>
        {
            ["dotnet_diagnostic.CA2200.severity"] = "none",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }
}
