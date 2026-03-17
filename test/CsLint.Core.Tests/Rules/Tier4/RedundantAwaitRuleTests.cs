using Cslint.Core.Engine;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier4;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Tests.Rules.Tier4;

public class RedundantAwaitRuleTests
{
    private readonly RedundantAwaitRule _rule = new();

    [Fact]
    public void Analyze_SimpleReturnAwait_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async Task M()
                {
                    return await OtherAsync();
                }

                Task OtherAsync() => Task.CompletedTask;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT307", diagnostic.RuleId);
        Assert.Contains("M", diagnostic.Message);
    }

    [Fact]
    public void Analyze_GenericTaskReturnAwait_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async Task<int> M()
                {
                    return await GetAsync();
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT307", diagnostic.RuleId);
        Assert.Contains("M", diagnostic.Message);
    }

    [Fact]
    public void Analyze_ExpressionBodiedAwait_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async Task<int> M() => await GetAsync();

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT307", diagnostic.RuleId);
        Assert.Contains("M", diagnostic.Message);
    }

    [Fact]
    public void Analyze_AsyncLocalFunction_ReportsDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                void M()
                {
                    async Task<int> Local()
                    {
                        return await GetAsync();
                    }
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT307", diagnostic.RuleId);
        Assert.Contains("Local", diagnostic.Message);
    }

    [Fact]
    public void Analyze_AsyncLambda_ReportsDiagnostic()
    {
        const string source = """
            using System;
            using System.Threading.Tasks;
            class C
            {
                void M()
                {
                    Func<Task<int>> f = async () => await GetAsync();
                }

                static Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        LintDiagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("CSLINT307", diagnostic.RuleId);
        Assert.Contains("lambda", diagnostic.Message);
    }

    [Fact]
    public void Analyze_TryCatchAroundAwait_NoDiagnostic()
    {
        const string source = """
            using System;
            using System.Threading.Tasks;
            class C
            {
                async Task<int> M()
                {
                    try
                    {
                        return await GetAsync();
                    }
                    catch (Exception)
                    {
                        return -1;
                    }
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_UsingStatement_NoDiagnostic()
    {
        const string source = """
            using System;
            using System.IO;
            using System.Threading.Tasks;
            class C
            {
                async Task<int> M()
                {
                    using (var stream = new MemoryStream())
                    {
                        return await GetAsync();
                    }
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ConfigureAwait_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async Task<int> M()
                {
                    return await GetAsync().ConfigureAwait(false);
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_MultipleStatements_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async Task<int> M()
                {
                    var x = 1;
                    return await GetAsync();
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_ValueTaskReturn_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async ValueTask<int> M()
                {
                    return await GetAsync();
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NonAsyncMethod_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                Task<int> M()
                {
                    return GetAsync();
                }

                Task<int> GetAsync() => Task.FromResult(42);
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_AsyncVoidAwaitNoReturn_NoDiagnostic()
    {
        const string source = """
            using System.Threading.Tasks;
            class C
            {
                async void M()
                {
                    await DoAsync();
                }

                Task DoAsync() => Task.CompletedTask;
            }
            """;

        (RuleContext context, SemanticModel model) = CreateSemanticContext(source);
        var diagnostics = new List<LintDiagnostic>();

        _rule.Analyze(context, model, diagnostics);

        Assert.Empty(diagnostics);
    }

    private static (RuleContext Context, SemanticModel Model) CreateSemanticContext(
        string source)
    {
        RuleContext context = TestHelper.CreateContext(source);
        SemanticModel model =
            CompilationFactory.CreateSemanticModel(context.SyntaxTree);
        return (context, model);
    }
}
