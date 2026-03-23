using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class NamespaceMatchFolderRuleTests
{
    private readonly NamespaceMatchFolderRule _rule = new();

    private static LintConfiguration Enforced =>
        new(new Dictionary<string, string>
        {
            ["dotnet_style_namespace_match_folder"] = "true:warning",
        });

    [Theory]
    [InlineData("MyApp.Models", "/src/MyApp/Models/User.cs")]
    [InlineData("MyApp.Services", "/src/MyApp/Services/UserService.cs")]
    [InlineData("MyApp", "/src/MyApp/Program.cs")]
    public void Analyze_NamespaceMatchesFolder_ReturnsNoDiagnostics(
        string ns, string filePath)
    {
        string source = $"namespace {ns} {{ class C {{ }} }}";
        RuleContext context = TestHelper.CreateContext(source, Enforced, filePath: filePath);

        Assert.Empty(_rule.Analyze(context));
    }

    [Theory]
    [InlineData("MyApp.Controllers", "/src/MyApp/Models/User.cs")]
    [InlineData("WrongNamespace", "/src/MyApp/Services/UserService.cs")]
    public void Analyze_NamespaceDoesNotMatchFolder_ReturnsDiagnostic(
        string ns, string filePath)
    {
        string source = $"namespace {ns} {{ class C {{ }} }}";
        RuleContext context = TestHelper.CreateContext(source, Enforced, filePath: filePath);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE0130", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_FileScopedNamespace_ReturnsDiagnostic()
    {
        string source = """
            namespace MyApp.Wrong;
            class C { }
            """;
        RuleContext context = TestHelper.CreateContext(
            source, Enforced, filePath: "/src/MyApp/Models/User.cs");

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_ConfigAbsent_ReturnsNoDiagnostics()
    {
        string source = "namespace Wrong { class C { } }";
        RuleContext context = TestHelper.CreateContext(
            source, filePath: "/src/MyApp/Models/User.cs");

        Assert.Empty(_rule.Analyze(context));
    }
}
