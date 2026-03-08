using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ExtendedPropertyPatternRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT236";

    public string Name => "ExtendedPropertyPattern";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_extended_property_pattern"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_extended_property_pattern") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_extended_property_pattern");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (node is not SubpatternSyntax subpattern)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_prefer_extended_property_pattern");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Must have a property name (e.g., `Prop:`)
        if (subpattern.NameColon is null)
        {
            return;
        }

        // The pattern must be a recursive pattern (e.g., `{ B: 1 }`)
        if (subpattern.Pattern is not RecursivePatternSyntax recursive)
        {
            return;
        }

        // Must not have a type (e.g., `SomeType { B: 1 }`)
        if (recursive.Type is not null)
        {
            return;
        }

        // Must not have positional patterns
        if (recursive.PositionalPatternClause is not null)
        {
            return;
        }

        // Must have property sub-patterns
        if (recursive.PropertyPatternClause is null || recursive.PropertyPatternClause.Subpatterns.Count == 0)
        {
            return;
        }

        // Must not have a designation (e.g., `{ B: 1 } y`)
        if (recursive.Designation is not null)
        {
            return;
        }

        FileLinePositionSpan span = subpattern.NameColon.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Property pattern can be simplified using extended property pattern",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
