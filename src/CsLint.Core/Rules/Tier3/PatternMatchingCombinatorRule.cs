using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PatternMatchingCombinatorRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT220";

    public string Name => "PatternMatchingCombinator";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_pattern_matching"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_pattern_matching") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_pattern_matching");

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
        if (node is not BinaryExpressionSyntax binaryExpr)
        {
            return;
        }

        SyntaxKind kind = binaryExpr.Kind();

        if (kind is not (SyntaxKind.LogicalAndExpression or SyntaxKind.LogicalOrExpression))
        {
            return;
        }

        // Skip if parent is the same logical kind (avoid double-reporting chains)
        if (binaryExpr.Parent is BinaryExpressionSyntax parentBinary && parentBinary.Kind() == kind)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_prefer_pattern_matching");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (!TryGetComparedVariable(binaryExpr.Left, out string? leftVar) ||
            !TryGetComparedVariable(binaryExpr.Right, out string? rightVar))
        {
            return;
        }

        if (!string.Equals(leftVar, rightVar, StringComparison.Ordinal))
        {
            return;
        }

        FileLinePositionSpan span = binaryExpr.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use pattern matching combinators ('and'/'or')",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private static bool TryGetComparedVariable(ExpressionSyntax expr, out string? variableName)
    {
        variableName = null;

        // Strip parentheses
        while (expr is ParenthesizedExpressionSyntax paren)
        {
            expr = paren.Expression;
        }

        // Relational: x < 10, x >= 5, etc.
        if (expr is BinaryExpressionSyntax binary &&
            binary.Kind() is SyntaxKind.LessThanExpression
                          or SyntaxKind.LessThanOrEqualExpression
                          or SyntaxKind.GreaterThanExpression
                          or SyntaxKind.GreaterThanOrEqualExpression
                          or SyntaxKind.EqualsExpression
                          or SyntaxKind.NotEqualsExpression)
        {
            variableName = binary.Left.WithoutTrivia().ToFullString();
            return true;
        }

        return false;
    }
}
