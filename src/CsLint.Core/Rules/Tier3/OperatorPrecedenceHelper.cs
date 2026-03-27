using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

internal enum OperatorCategory
{
    None,
    Arithmetic,
    Relational,
    OtherBinary,
}

internal static class OperatorPrecedenceHelper
{
    public static string? GetConfigKey(OperatorCategory category) =>
        category switch
        {
            OperatorCategory.Arithmetic => "dotnet_style_parentheses_in_arithmetic_binary_operators",
            OperatorCategory.Relational => "dotnet_style_parentheses_in_relational_binary_operators",
            OperatorCategory.OtherBinary => "dotnet_style_parentheses_in_other_binary_operators",
            _ => null,
        };

    public static OperatorCategory GetCategory(SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.MultiplyExpression or
            SyntaxKind.DivideExpression or
            SyntaxKind.ModuloExpression or
            SyntaxKind.AddExpression or
            SyntaxKind.SubtractExpression => OperatorCategory.Arithmetic,

            SyntaxKind.LessThanExpression or
            SyntaxKind.GreaterThanExpression or
            SyntaxKind.LessThanOrEqualExpression or
            SyntaxKind.GreaterThanOrEqualExpression or
            SyntaxKind.EqualsExpression or
            SyntaxKind.NotEqualsExpression or
            SyntaxKind.IsExpression or
            SyntaxKind.AsExpression => OperatorCategory.Relational,

            SyntaxKind.LogicalAndExpression or
            SyntaxKind.LogicalOrExpression or
            SyntaxKind.CoalesceExpression or
            SyntaxKind.BitwiseAndExpression or
            SyntaxKind.BitwiseOrExpression or
            SyntaxKind.ExclusiveOrExpression or
            SyntaxKind.LeftShiftExpression or
            SyntaxKind.RightShiftExpression => OperatorCategory.OtherBinary,

            _ => OperatorCategory.None,
        };

    /// <summary>
    /// Returns the C# operator precedence level. Higher values bind tighter.
    /// </summary>
    public static int GetPrecedence(SyntaxKind kind) =>
        kind switch
        {
            SyntaxKind.MultiplyExpression or
            SyntaxKind.DivideExpression or
            SyntaxKind.ModuloExpression => 14,

            SyntaxKind.AddExpression or
            SyntaxKind.SubtractExpression => 13,

            SyntaxKind.LeftShiftExpression or
            SyntaxKind.RightShiftExpression => 12,

            SyntaxKind.LessThanExpression or
            SyntaxKind.GreaterThanExpression or
            SyntaxKind.LessThanOrEqualExpression or
            SyntaxKind.GreaterThanOrEqualExpression or
            SyntaxKind.IsExpression or
            SyntaxKind.AsExpression => 11,

            SyntaxKind.EqualsExpression or
            SyntaxKind.NotEqualsExpression => 10,

            SyntaxKind.BitwiseAndExpression => 9,

            SyntaxKind.ExclusiveOrExpression => 8,

            SyntaxKind.BitwiseOrExpression => 7,

            SyntaxKind.LogicalAndExpression => 6,

            SyntaxKind.LogicalOrExpression => 5,

            SyntaxKind.CoalesceExpression => 4,

            _ => 0,
        };
}
