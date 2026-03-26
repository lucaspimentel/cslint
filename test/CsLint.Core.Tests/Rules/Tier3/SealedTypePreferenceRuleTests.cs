using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier3;

namespace Cslint.Core.Tests.Rules.Tier3;

public class SealedTypePreferenceRuleTests
{
    private readonly SealedTypePreferenceRule _rule = new();

    [Theory]
    [InlineData("class Foo { }")]
    [InlineData("public class Foo { }")]
    [InlineData("internal class Foo { }")]
    [InlineData("record Foo;")]
    [InlineData("public record Foo;")]
    [InlineData("record class Foo;")]
    [InlineData("public partial class Foo { }")]
    public void Analyze_UnsealedType_ReturnsDiagnostic(string declaration)
    {
        string source = declaration;
        RuleContext context = TestHelper.CreateContext(source, CreateConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1852", diagnostics[0].RuleId);
        Assert.Equal("Type should be sealed", diagnostics[0].Message);
    }

    [Theory]
    [InlineData("sealed class Foo { }")]
    [InlineData("public sealed class Foo { }")]
    [InlineData("abstract class Foo { }")]
    [InlineData("public abstract class Foo { }")]
    [InlineData("static class Foo { }")]
    [InlineData("public static class Foo { }")]
    [InlineData("sealed record Foo;")]
    [InlineData("abstract record Foo;")]
    [InlineData("struct Foo { }")]
    [InlineData("record struct Foo;")]
    [InlineData("public record struct Foo;")]
    [InlineData("interface IFoo { }")]
    [InlineData("enum Foo { A, B }")]
    [InlineData("public sealed partial class Foo { }")]
    public void Analyze_SealedOrExemptType_NoDiagnostic(string declaration)
    {
        string source = declaration;
        RuleContext context = TestHelper.CreateContext(source, CreateConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Analyze_NestedUnsealedClass_ReturnsDiagnostic()
    {
        const string source = """
            sealed class Outer
            {
                class Inner { }
            }
            """;
        RuleContext context = TestHelper.CreateContext(source, CreateConfig());

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CA1852", diagnostics[0].RuleId);
    }

    [Fact]
    public void Analyze_Disabled_NoDiagnostic()
    {
        const string source = "class Foo { }";
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    private static LintConfiguration CreateConfig() =>
        new(new Dictionary<string, string>
        {
            ["csharp_prefer_sealed_types"] = "true",
        });
}
