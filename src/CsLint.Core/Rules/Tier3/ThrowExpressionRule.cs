using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0016: Use throw expression (?? throw) instead of null check with if statement.
/// </summary>
public sealed class ThrowExpressionRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_throw_expression";

    private const string OverrideKey = "dotnet_style_null_checking";

    public string RuleId => "IDE0016";

    public string Name => "ThrowExpression";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, OverrideKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

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
        if (statement is ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } &&
            StripParentheses(assignment.Right) is IdentifierNameSyntax assignId &&
            assignId.Identifier.Text == identifierName)
        {
            return true;
        }

        if (statement is ReturnStatementSyntax { Expression: { } returnExpr } &&
            StripParentheses(returnExpr) is IdentifierNameSyntax returnId &&
            returnId.Identifier.Text == identifierName)
        {
            return true;
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration)
    {
        if (configuration.GetValue(OverrideKey) is not null)
        {
            return configuration.GetBool(OverrideKey);
        }

        (string? value, string? _) = configuration.GetValueWithSeverity(ConfigKey);

        if (value is not null)
        {
            return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
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
                RuleId = RuleId,
                Message = "Use null coalescing throw expression (?? throw) instead of null check with if statement",
                Severity = LintSeverity.Info,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
