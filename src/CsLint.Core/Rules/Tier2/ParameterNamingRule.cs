using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

/// <summary>
/// SA1313: Parameter names should begin with lower-case letter.
/// </summary>
public sealed class ParameterNamingRule : IRuleDefinition, INamingRuleHandler
{
    private const string ConfigKey = "dotnet_naming_rule.locals_should_be_camel_case";

    public string RuleId => "SA1313";

    public string Name => "ParameterNaming";

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

    void INamingRuleHandler.VisitParameter(ParameterSyntax node, List<LintDiagnostic> diagnostics)
    {
        // Skip discard parameters
        if (node.Identifier.ValueText == "_")
        {
            return;
        }

        // Skip record positional parameters (they generate public properties, PascalCase is correct)
        if (node.Parent?.Parent is RecordDeclarationSyntax)
        {
            return;
        }

        string name = node.Identifier.ValueText;

        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        if (!NamingHelper.IsCamelCase(name))
        {
            FileLinePositionSpan span = node.Identifier.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"parameter '{name}' should use camelCase",
                    Severity = DefaultSeverity,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
