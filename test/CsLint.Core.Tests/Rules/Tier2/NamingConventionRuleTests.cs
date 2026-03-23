using Cslint.Core.Config;
using Cslint.Core.Rules;
using Cslint.Core.Rules.Tier2;

namespace Cslint.Core.Tests.Rules.Tier2;

public class NamingConventionRuleTests
{
    private readonly NamingConventionRule _rule = new();

    private static LintConfiguration PascalCaseTypes =>
        new(new Dictionary<string, string>
        {
            ["dotnet_naming_rule.types_pascal.symbols"] = "types",
            ["dotnet_naming_rule.types_pascal.style"] = "pascal",
            ["dotnet_naming_rule.types_pascal.severity"] = "warning",
            ["dotnet_naming_symbols.types.applicable_kinds"] = "class, struct, interface, enum",
            ["dotnet_naming_symbols.types.applicable_accessibilities"] = "*",
            ["dotnet_naming_style.pascal.capitalization"] = "pascal_case",
        });

    private static LintConfiguration InterfacePrefixRule =>
        new(new Dictionary<string, string>
        {
            ["dotnet_naming_rule.iface.symbols"] = "interfaces",
            ["dotnet_naming_rule.iface.style"] = "begins_with_i",
            ["dotnet_naming_rule.iface.severity"] = "warning",
            ["dotnet_naming_symbols.interfaces.applicable_kinds"] = "interface",
            ["dotnet_naming_symbols.interfaces.applicable_accessibilities"] = "*",
            ["dotnet_naming_style.begins_with_i.capitalization"] = "pascal_case",
            ["dotnet_naming_style.begins_with_i.required_prefix"] = "I",
        });

    [Fact]
    public void Analyze_PascalCaseClass_ReturnsNoDiagnostics()
    {
        string source = "class MyClass { }";
        RuleContext context = TestHelper.CreateContext(source, PascalCaseTypes);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NonPascalCaseClass_ReturnsDiagnostic()
    {
        string source = "class myClass { }";
        RuleContext context = TestHelper.CreateContext(source, PascalCaseTypes);

        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);

        Assert.Single(diagnostics);
        Assert.Equal("IDE1006", diagnostics[0].RuleId);
        Assert.Contains("myClass", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_InterfaceWithIPrefix_ReturnsNoDiagnostics()
    {
        string source = "interface IMyInterface { }";
        RuleContext context = TestHelper.CreateContext(source, InterfacePrefixRule);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_InterfaceWithoutIPrefix_ReturnsDiagnostic()
    {
        string source = "interface MyInterface { }";
        RuleContext context = TestHelper.CreateContext(source, InterfacePrefixRule);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_CamelCasePrivateFields()
    {
        string source = """
            class C
            {
                private int _myField;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_naming_rule.private_fields.symbols"] = "priv_fields",
            ["dotnet_naming_rule.private_fields.style"] = "underscore_camel",
            ["dotnet_naming_rule.private_fields.severity"] = "warning",
            ["dotnet_naming_symbols.priv_fields.applicable_kinds"] = "field",
            ["dotnet_naming_symbols.priv_fields.applicable_accessibilities"] = "private",
            ["dotnet_naming_style.underscore_camel.capitalization"] = "camel_case",
            ["dotnet_naming_style.underscore_camel.required_prefix"] = "_",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_PrivateFieldMissingPrefix_ReturnsDiagnostic()
    {
        string source = """
            class C
            {
                private int myField;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_naming_rule.private_fields.symbols"] = "priv_fields",
            ["dotnet_naming_rule.private_fields.style"] = "underscore_camel",
            ["dotnet_naming_rule.private_fields.severity"] = "warning",
            ["dotnet_naming_symbols.priv_fields.applicable_kinds"] = "field",
            ["dotnet_naming_symbols.priv_fields.applicable_accessibilities"] = "private",
            ["dotnet_naming_style.underscore_camel.capitalization"] = "camel_case",
            ["dotnet_naming_style.underscore_camel.required_prefix"] = "_",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        Assert.Single(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_NoNamingConfig_ReturnsNoDiagnostics()
    {
        string source = "class myClass { }";
        RuleContext context = TestHelper.CreateContext(source);

        Assert.Empty(_rule.Analyze(context));
    }

    [Fact]
    public void Analyze_RequiredModifiers_OnlyMatchesStatic()
    {
        string source = """
            class C
            {
                private int instanceField;
                private static int staticField;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_naming_rule.static_fields.symbols"] = "statics",
            ["dotnet_naming_rule.static_fields.style"] = "pascal",
            ["dotnet_naming_rule.static_fields.severity"] = "warning",
            ["dotnet_naming_symbols.statics.applicable_kinds"] = "field",
            ["dotnet_naming_symbols.statics.applicable_accessibilities"] = "*",
            ["dotnet_naming_symbols.statics.required_modifiers"] = "static",
            ["dotnet_naming_style.pascal.capitalization"] = "pascal_case",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // Only staticField should be flagged (not PascalCase), instanceField doesn't match the rule
        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);
        Assert.Single(diagnostics);
        Assert.Contains("staticField", diagnostics[0].Message);
    }

    [Fact]
    public void Analyze_ConstIsImplicitlyStatic_MatchesStaticRule()
    {
        string source = """
            class C
            {
                private const int myConst = 1;
            }
            """;
        var config = new LintConfiguration(new Dictionary<string, string>
        {
            ["dotnet_naming_rule.static_fields.symbols"] = "statics",
            ["dotnet_naming_rule.static_fields.style"] = "pascal",
            ["dotnet_naming_rule.static_fields.severity"] = "warning",
            ["dotnet_naming_symbols.statics.applicable_kinds"] = "field",
            ["dotnet_naming_symbols.statics.applicable_accessibilities"] = "*",
            ["dotnet_naming_symbols.statics.required_modifiers"] = "static",
            ["dotnet_naming_style.pascal.capitalization"] = "pascal_case",
        });
        RuleContext context = TestHelper.CreateContext(source, config);

        // const is implicitly static, so myConst should match the static rule and be flagged
        IReadOnlyList<LintDiagnostic> diagnostics = _rule.Analyze(context);
        Assert.Single(diagnostics);
        Assert.Contains("myConst", diagnostics[0].Message);
    }
}
