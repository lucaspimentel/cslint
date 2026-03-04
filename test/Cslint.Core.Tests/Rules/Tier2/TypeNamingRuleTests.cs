using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class TypeNamingRuleTests
{
    private readonly TypeNamingRule _rule = new();

    [Theory]
    [InlineData("class MyClass { }")]
    [InlineData("struct MyStruct { }")]
    [InlineData("enum MyEnum { }")]
    [InlineData("record MyRecord;")]
    [InlineData("delegate void MyDelegate();")]
    public void Analyze_PascalCaseTypes_ReturnsNoDiagnostics(string source)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Empty(diagnostics);
    }

    [Theory]
    [InlineData("class myClass { }", "class", "myClass")]
    [InlineData("struct my_struct { }", "struct", "my_struct")]
    [InlineData("enum myEnum { }", "enum", "myEnum")]
    public void Analyze_NonPascalCaseTypes_ReturnsDiagnostics(string source, string kind, string name)
    {
        RuleContext context = TestHelper.CreateContext(source);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("CSLINT100", diagnostics[0].RuleId);
        Assert.Contains(kind, diagnostics[0].Message);
        Assert.Contains(name, diagnostics[0].Message);
    }
}
