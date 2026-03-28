using Cslint.Core.Config;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

/// <summary>
/// SA1306: Field names should begin with lower-case letter (private instance fields use _camelCase).
/// </summary>
public sealed class PrivateFieldNamingRule : IRuleDefinition, INamingRuleHandler
{
    private const string ConfigKey = "dotnet_naming_rule.private_fields_should_be_underscore_camel_case";

    public string RuleId => "SA1306";

    public string Name => "PrivateFieldNaming";

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

        if (FieldNamingHelper.IsPrivate(node) && !FieldNamingHelper.IsStatic(node))
        {
            FieldNamingHelper.CheckFields(node, diagnostics, NamingHelper.IsUnderscoreCamelCase,
                RuleId, "Private field", "_camelCase");
        }
    }
}
