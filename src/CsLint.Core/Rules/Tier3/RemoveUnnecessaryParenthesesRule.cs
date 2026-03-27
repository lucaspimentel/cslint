using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0047: Remove unnecessary parentheses.
/// Flags parenthesized binary expressions where the parentheses don't change evaluation order.
/// </summary>
public sealed class RemoveUnnecessaryParenthesesRule : IRuleDefinition
{
    private static readonly string[] AllConfigKeys =
    [
        "dotnet_style_parentheses_in_arithmetic_binary_operators",
        "dotnet_style_parentheses_in_relational_binary_operators",
        "dotnet_style_parentheses_in_other_binary_operators",
    ];

    public string RuleId => "IDE0047";

    public string Name => "RemoveUnnecessaryParentheses";

    public IReadOnlyList<string> ConfigKeys { get; } = AllConfigKeys;

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration)
    {
        foreach (string key in AllConfigKeys)
        {
            if (configuration.GetValue(key) is not null)
            {
                return true;
            }
        }

        return false;
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            if (node is ParenthesizedExpressionSyntax parenthesized)
            {
                CheckParenthesizedExpression(parenthesized, context, diagnostics);
            }
        }

        return diagnostics;
    }

    private static void CheckParenthesizedExpression(
        ParenthesizedExpressionSyntax parenthesized,
        RuleContext context,
        List<LintDiagnostic> diagnostics)
    {
        // Inner expression must be a binary expression
        if (parenthesized.Expression is not BinaryExpressionSyntax innerBinary)
        {
            return;
        }

        OperatorCategory innerCategory = OperatorPrecedenceHelper.GetCategory(innerBinary.Kind());

        if (innerCategory == OperatorCategory.None)
        {
            return;
        }

        string? configKey = OperatorPrecedenceHelper.GetConfigKey(innerCategory);

        if (configKey is null)
        {
            return;
        }

        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(configKey);

        if (!string.Equals(pref, "never_if_unnecessary", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Check if parentheses are actually unnecessary
        if (!AreParenthesesUnnecessary(parenthesized, innerBinary))
        {
            return;
        }

        FileLinePositionSpan span = parenthesized.OpenParenToken.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "IDE0047",
                Message = "Parentheses can be removed",
                Severity = LintSeverity.Info,
                FilePath = context.FilePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private static bool AreParenthesesUnnecessary(
        ParenthesizedExpressionSyntax parenthesized,
        BinaryExpressionSyntax innerBinary)
    {
        SyntaxNode? parent = parenthesized.Parent;

        // If parent is a binary expression, compare precedence
        if (parent is BinaryExpressionSyntax outerBinary)
        {
            int innerPrecedence = OperatorPrecedenceHelper.GetPrecedence(innerBinary.Kind());
            int outerPrecedence = OperatorPrecedenceHelper.GetPrecedence(outerBinary.Kind());

            if (innerPrecedence == 0 || outerPrecedence == 0)
            {
                return false;
            }

            if (innerPrecedence > outerPrecedence)
            {
                // Inner binds tighter, parentheses are unnecessary
                return true;
            }

            if (innerPrecedence == outerPrecedence)
            {
                // Same precedence — parentheses are unnecessary on the left operand
                // (all C# binary operators are left-associative, except ?? which is right-associative)
                bool isLeftOperand = outerBinary.Left == parenthesized;

                if (outerBinary.IsKind(SyntaxKind.CoalesceExpression))
                {
                    // ?? is right-associative: right operand doesn't need parens
                    return !isLeftOperand;
                }

                return isLeftOperand;
            }

            // Inner has lower precedence — parentheses change meaning, keep them
            return false;
        }

        // If parent is not a binary expression (e.g., assignment, return, argument),
        // parentheses around a binary expression are always unnecessary
        if (parent is AssignmentExpressionSyntax or
            EqualsValueClauseSyntax or
            ReturnStatementSyntax or
            ArgumentSyntax or
            ArrowExpressionClauseSyntax or
            ExpressionStatementSyntax or
            InitializerExpressionSyntax)
        {
            return true;
        }

        return false;
    }
}
