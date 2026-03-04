using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ThisQualificationRule : IRuleDefinition
{
    public string RuleId => "CSLINT204";

    public string Name => "ThisQualification";

    public IReadOnlyList<string> ConfigKeys { get; } =
    [
        "dotnet_style_qualification_for_field",
        "dotnet_style_qualification_for_property",
        "dotnet_style_qualification_for_method",
        "dotnet_style_qualification_for_event",
    ];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_qualification_for_field") is not null ||
        configuration.GetValue("dotnet_style_qualification_for_property") is not null ||
        configuration.GetValue("dotnet_style_qualification_for_method") is not null ||
        configuration.GetValue("dotnet_style_qualification_for_event") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        // Syntax-only: detect 'this.' usage and report if preference is false
        (string? fieldPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_field");
        (string? propPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_property");
        (string? methodPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_method");
        (string? eventPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_event");

        bool anyDisallowed =
            string.Equals(fieldPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(propPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(methodPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(eventPref, "false", StringComparison.OrdinalIgnoreCase);

        if (!anyDisallowed)
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            if (node is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess)
            {
                FileLinePositionSpan span = memberAccess.Expression.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Remove 'this.' qualification from '{memberAccess.Name}'",
                        Severity = LintSeverity.Warning,
                        FilePath = context.FilePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        return diagnostics;
    }
}
