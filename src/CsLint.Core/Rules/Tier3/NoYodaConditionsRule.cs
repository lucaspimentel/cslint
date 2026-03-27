using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NoYodaConditionsRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_no_yoda_conditions";

    public string RuleId => "SA1131";

    public string Name => "NoYodaConditions";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsComparisonOperator(SyntaxKind kind) =>
        kind is SyntaxKind.EqualsExpression
            or SyntaxKind.NotEqualsExpression
            or SyntaxKind.LessThanExpression
            or SyntaxKind.GreaterThanExpression
            or SyntaxKind.LessThanOrEqualExpression
            or SyntaxKind.GreaterThanOrEqualExpression;

    private static bool IsLiteral(ExpressionSyntax expression) =>
        expression is LiteralExpressionSyntax
            or DefaultExpressionSyntax
            || expression.IsKind(SyntaxKind.DefaultLiteralExpression);

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
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
        if (node is not BinaryExpressionSyntax binary)
        {
            return;
        }

        if (!IsComparisonOperator(binary.Kind()))
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (IsLiteral(binary.Left) && !IsLiteral(binary.Right))
        {
            FileLinePositionSpan span = binary.Left.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Constant should not appear on the left side of a comparison (Yoda condition)",
                    Severity = DefaultSeverity,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
