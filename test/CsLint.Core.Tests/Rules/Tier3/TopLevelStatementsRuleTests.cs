using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class TopLevelStatementsRuleTests
{
    private readonly TopLevelStatementsRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["csharp_style_prefer_top_level_statements"] = "true:warning",
        });

    [Fact]
    public void Analyze_ClassWithStaticMain_ReturnsDiagnostic()
    {
        string source = """
            class Program
            {
                static void Main(string[] args)
                {
                    System.Console.WriteLine("Hello");
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0210", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_ClassWithAsyncMain_ReturnsDiagnostic()
    {
        string source = """
            using System.Threading.Tasks;
            class Program
            {
                static async Task Main(string[] args)
                {
                    await Task.CompletedTask;
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_MainInNamespace_ReturnsDiagnostic()
    {
        string source = """
            namespace MyApp
            {
                class Program
                {
                    static void Main()
                    {
                    }
                }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_MultipleTypes_ReturnsNoDiagnostics()
    {
        string source = """
            class Program
            {
                static void Main() { }
            }
            class Helper { }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NonStaticMain_ReturnsNoDiagnostics()
    {
        string source = """
            class Program
            {
                void Main() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_TopLevelStatements_ReturnsNoDiagnostics()
    {
        string source = """
            System.Console.WriteLine("Hello");
            """;
        RuleContext context = TestHelper.CreateContext(source, Enforced);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = """
            class Program
            {
                static void Main() { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }
}
