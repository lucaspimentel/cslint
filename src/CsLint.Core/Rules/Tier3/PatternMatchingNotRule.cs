using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PatternMatchingNotRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT219";

    public string Name => "PatternMatchingNot";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_not_pattern"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_not_pattern") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_not_pattern");

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
        if (node is not PrefixUnaryExpressionSyntax { RawKind: (int)SyntaxKind.LogicalNotExpression } notExpr)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_prefer_not_pattern");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ExpressionSyntax operand = notExpr.Operand;

        // Strip parentheses
        while (operand is ParenthesizedExpressionSyntax paren)
        {
            operand = paren.Expression;
        }

        // IsPatternExpressionSyntax = `x is T t` or `x is pattern`
        // BinaryExpressionSyntax with IsExpression = `x is T` (type-check without pattern)
        if (operand is not IsPatternExpressionSyntax &&
            !(operand is BinaryExpressionSyntax { RawKind: (int)SyntaxKind.IsExpression }))
        {
            return;
        }

        FileLinePositionSpan span = notExpr.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use 'is not' pattern instead of negating 'is' expression",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
