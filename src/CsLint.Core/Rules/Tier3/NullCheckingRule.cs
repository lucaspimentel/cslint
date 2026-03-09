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

    public LintSeverity DefaultSeverity => LintSeverity.Info;

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
        if (TryGetNullCheckedIdentifier(node.Condition, out string? checkedName) &&
            StripParentheses(node.WhenTrue) is IdentifierNameSyntax trueName &&
            trueName.Identifier.Text == checkedName)
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
        if (!TryGetNullCheckedIdentifierFromEquality(node.Condition, out string? checkedName) ||
            node.Statement is not BlockSyntax { Statements.Count: 1 } block ||
            block.Statements[0] is not ThrowStatementSyntax)
        {
            return;
        }

        // Only flag when the next sibling statement is an assignment or return using
        // the checked identifier — that's the pattern convertible to `?? throw`.
        // Standalone guard clauses (no next statement or unrelated next statement) cannot
        // use `?? throw` and would be false positives.
        if (node.Parent is not BlockSyntax parentBlock)
        {
            return;
        }

        int index = parentBlock.Statements.IndexOf(node);

        if (index < 0 || index >= parentBlock.Statements.Count - 1)
        {
            return;
        }

        StatementSyntax nextStatement = parentBlock.Statements[index + 1];

        if (!NextStatementUsesIdentifier(nextStatement, checkedName!))
        {
            return;
        }

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

    private static bool TryGetNullCheckedIdentifier(ExpressionSyntax expression, out string? identifier)
    {
        if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression) &&
                StripParentheses(binary.Left) is IdentifierNameSyntax leftId)
            {
                identifier = leftId.Identifier.Text;
                return true;
            }

            if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression) &&
                StripParentheses(binary.Right) is IdentifierNameSyntax rightId)
            {
                identifier = rightId.Identifier.Text;
                return true;
            }
        }

        identifier = null;
        return false;
    }

    private static ExpressionSyntax StripParentheses(ExpressionSyntax expression)
    {
        while (expression is ParenthesizedExpressionSyntax paren)
        {
            expression = paren.Expression;
        }

        return expression;
    }

    private static bool TryGetNullCheckedIdentifierFromEquality(ExpressionSyntax expression, out string? identifier)
    {
        if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.EqualsExpression))
        {
            if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression) &&
                StripParentheses(binary.Left) is IdentifierNameSyntax leftId)
            {
                identifier = leftId.Identifier.Text;
                return true;
            }

            if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression) &&
                StripParentheses(binary.Right) is IdentifierNameSyntax rightId)
            {
                identifier = rightId.Identifier.Text;
                return true;
            }
        }

        identifier = null;
        return false;
    }

    private static bool NextStatementUsesIdentifier(StatementSyntax statement, string identifierName)
    {
        // Assignment: `_field = identifier;`
        if (statement is ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } &&
            StripParentheses(assignment.Right) is IdentifierNameSyntax assignId &&
            assignId.Identifier.Text == identifierName)
        {
            return true;
        }

        // Return: `return identifier;`
        if (statement is ReturnStatementSyntax { Expression: { } returnExpr } &&
            StripParentheses(returnExpr) is IdentifierNameSyntax returnId &&
            returnId.Identifier.Text == identifierName)
        {
            return true;
        }

        return false;
    }

}
