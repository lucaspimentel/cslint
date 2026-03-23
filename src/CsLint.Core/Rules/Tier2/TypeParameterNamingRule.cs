using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class TypeParameterNamingRule : IRuleDefinition, INamingRuleHandler
{
    public string RuleId => "CSLINT106";

    public string Name => "TypeParameterNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.type_parameters_should_begin_with_t"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        !NamingConventionRule.HasStandardNamingConfig(configuration);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitTypeParameter(TypeParameterSyntax node, List<LintDiagnostic> diagnostics)
    {
        string name = node.Identifier.ValueText;

        if (name.Length >= 1 && name[0] == 'T' && (name.Length == 1 || char.IsUpper(name[1]) || char.IsDigit(name[1])))
        {
            return;
        }

        FileLinePositionSpan span = node.Identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Type parameter '{name}' should begin with 'T'",
                Severity = DefaultSeverity,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
