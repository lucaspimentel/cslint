using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NullCheckOverTypeCheckRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "IDE0150";

    public string Name => "NullCheckOverTypeCheck";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_null_check_over_type_check"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsObjectType(TypeSyntax type) =>
        type is PredefinedTypeSyntax predefined && predefined.Keyword.IsKind(SyntaxKind.ObjectKeyword);

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_null_check_over_type_check") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_null_check_over_type_check");

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
        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_prefer_null_check_over_type_check");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        switch (node)
        {
            // x is object (old-style BinaryExpression with IsExpression kind)
            case BinaryExpressionSyntax { RawKind: (int)SyntaxKind.IsExpression } binary
                when binary.Right is TypeSyntax typeSyntax && IsObjectType(typeSyntax):
            {
                FileLinePositionSpan span = binary.OperatorToken.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Use 'is not null' instead of 'is object'",
                        Severity = LintSeverity.Info,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            // x is object (pattern syntax form, C# 9+)
            case IsPatternExpressionSyntax { Pattern: TypePatternSyntax typePattern }
                when IsObjectType(typePattern.Type):
            {
                FileLinePositionSpan span = ((IsPatternExpressionSyntax)node).IsKeyword.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Use 'is not null' instead of 'is object'",
                        Severity = LintSeverity.Info,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            // x is not object
            case IsPatternExpressionSyntax
            {
                Pattern: UnaryPatternSyntax
                {
                    OperatorToken.RawKind: (int)SyntaxKind.NotKeyword,
                    Pattern: TypePatternSyntax negatedTypePattern,
                },
            } when IsObjectType(negatedTypePattern.Type):
            {
                FileLinePositionSpan span = ((IsPatternExpressionSyntax)node).IsKeyword.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Use 'is null' instead of 'is not object'",
                        Severity = LintSeverity.Info,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }
        }
    }
}
