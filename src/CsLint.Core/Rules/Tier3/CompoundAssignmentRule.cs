using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class CompoundAssignmentRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT214";

    public string Name => "CompoundAssignment";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_prefer_compound_assignment"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_prefer_compound_assignment") is not null;

    private static readonly Dictionary<SyntaxKind, string> SupportedOperators = new()
    {
        [SyntaxKind.AddExpression] = "+=",
        [SyntaxKind.SubtractExpression] = "-=",
        [SyntaxKind.MultiplyExpression] = "*=",
        [SyntaxKind.DivideExpression] = "/=",
        [SyntaxKind.ModuloExpression] = "%=",
        [SyntaxKind.BitwiseAndExpression] = "&=",
        [SyntaxKind.BitwiseOrExpression] = "|=",
        [SyntaxKind.ExclusiveOrExpression] = "^=",
        [SyntaxKind.LeftShiftExpression] = "<<=",
        [SyntaxKind.RightShiftExpression] = ">>=",
        [SyntaxKind.CoalesceExpression] = "??=",
    };

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("dotnet_style_prefer_compound_assignment");

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
        if (node is not AssignmentExpressionSyntax
            {
                RawKind: (int)SyntaxKind.SimpleAssignmentExpression
            } assignment)
        {
            return;
        }

        if (assignment.Right is not BinaryExpressionSyntax binaryExpr)
        {
            return;
        }

        if (!SupportedOperators.TryGetValue(binaryExpr.Kind(), out string? compoundOp))
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity("dotnet_style_prefer_compound_assignment");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string lhsText = assignment.Left.WithoutTrivia().ToFullString();
        string rhsLeftText = binaryExpr.Left.WithoutTrivia().ToFullString();

        if (!string.Equals(lhsText, rhsLeftText, StringComparison.Ordinal))
        {
            return;
        }

        FileLinePositionSpan span = assignment.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Use compound assignment '{compoundOp}'",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
