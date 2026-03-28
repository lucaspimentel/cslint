using Cslint.Core.Config;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

/// <summary>
/// SA1304: Non-private readonly fields should begin with upper-case letter.
/// </summary>
public sealed class ReadonlyFieldNamingRule : IRuleDefinition, INamingRuleHandler
{
    private const string ConfigKey = "dotnet_naming_rule.private_fields_should_be_underscore_camel_case";

    public string RuleId => "SA1304";

    public string Name => "ReadonlyFieldNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        !NamingConventionRule.HasStandardNamingConfig(configuration);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (FieldNamingHelper.ShouldSkipField(node))
        {
            return;
        }

        if (!FieldNamingHelper.IsPrivate(node) && FieldNamingHelper.IsReadOnly(node) && !FieldNamingHelper.IsStatic(node))
        {
            FieldNamingHelper.CheckFields(node, diagnostics, NamingHelper.IsPascalCase,
                RuleId, "Accessible readonly field", "PascalCase");
        }
    }
}
