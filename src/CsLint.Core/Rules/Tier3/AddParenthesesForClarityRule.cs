using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0048: Add parentheses for clarity.
/// Flags binary expressions nested inside other binary expressions (same category, different precedence)
/// that are not wrapped in parentheses.
/// </summary>
public sealed class AddParenthesesForClarityRule : IRuleDefinition
{
    private static readonly string[] AllConfigKeys =
    [
        "dotnet_style_parentheses_in_arithmetic_binary_operators",
        "dotnet_style_parentheses_in_relational_binary_operators",
        "dotnet_style_parentheses_in_other_binary_operators",
        "dotnet_style_parentheses_in_other_operators",
    ];

    public string RuleId => "IDE0048";

    public string Name => "AddParenthesesForClarity";

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
            if (node is BinaryExpressionSyntax outerBinary)
            {
                CheckBinaryExpression(outerBinary, context, diagnostics);
            }
        }

        return diagnostics;
    }

    private static void CheckBinaryExpression(
        BinaryExpressionSyntax outerBinary,
        RuleContext context,
        List<LintDiagnostic> diagnostics)
    {
        OperatorCategory outerCategory = OperatorPrecedenceHelper.GetCategory(outerBinary.Kind());

        if (outerCategory == OperatorCategory.None)
        {
            return;
        }

        string? configKey = OperatorPrecedenceHelper.GetConfigKey(outerCategory);

        if (configKey is null)
        {
            return;
        }

        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(configKey);

        if (!string.Equals(pref, "always_for_clarity", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        int outerPrecedence = OperatorPrecedenceHelper.GetPrecedence(outerBinary.Kind());

        CheckOperand(outerBinary.Left, outerCategory, outerPrecedence, context, diagnostics);
        CheckOperand(outerBinary.Right, outerCategory, outerPrecedence, context, diagnostics);
    }

    private static void CheckOperand(
        ExpressionSyntax operand,
        OperatorCategory outerCategory,
        int outerPrecedence,
        RuleContext context,
        List<LintDiagnostic> diagnostics)
    {
        // Skip if already wrapped in parentheses
        if (operand is ParenthesizedExpressionSyntax)
        {
            return;
        }

        if (operand is not BinaryExpressionSyntax innerBinary)
        {
            return;
        }

        OperatorCategory innerCategory = OperatorPrecedenceHelper.GetCategory(innerBinary.Kind());

        // Only flag when both operators are in the same category but at different precedence levels
        if (innerCategory != outerCategory)
        {
            return;
        }

        int innerPrecedence = OperatorPrecedenceHelper.GetPrecedence(innerBinary.Kind());

        if (innerPrecedence == outerPrecedence)
        {
            return;
        }

        FileLinePositionSpan span = innerBinary.OperatorToken.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "IDE0048",
                Message = "Add parentheses for clarity",
                Severity = LintSeverity.Info,
                FilePath = context.FilePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
