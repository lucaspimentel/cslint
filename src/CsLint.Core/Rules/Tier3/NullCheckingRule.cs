using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NullCheckingRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT210";

    public string Name => "NullChecking";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_null_checking"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitConditionalExpression(
        ConditionalExpressionSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (IsNullCheck(node.Condition))
        {
            FileLinePositionSpan span = node.QuestionToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT210",
                    Message = "Use null coalescing operator (??) instead of null check with conditional expression",
                    Severity = LintSeverity.Info,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }

    void IStyleRuleHandler.VisitIfStatement(
        IfStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (IsNullEqualityCheck(node.Condition) &&
            node.Statement is BlockSyntax { Statements.Count: 1 } block &&
            block.Statements[0] is ThrowStatementSyntax)
        {
            FileLinePositionSpan span = node.IfKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT210",
                    Message = "Use null coalescing throw expression (?? throw) instead of null check with if statement",
                    Severity = LintSeverity.Info,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }

    private static bool IsNullCheck(ExpressionSyntax expression)
    {
        if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            return binary.Right.IsKind(SyntaxKind.NullLiteralExpression) ||
                   binary.Left.IsKind(SyntaxKind.NullLiteralExpression);
        }

        return false;
    }

    private static bool IsNullEqualityCheck(ExpressionSyntax expression)
    {
        if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.EqualsExpression))
        {
            return binary.Right.IsKind(SyntaxKind.NullLiteralExpression) ||
                   binary.Left.IsKind(SyntaxKind.NullLiteralExpression);
        }

        return false;
    }
}
