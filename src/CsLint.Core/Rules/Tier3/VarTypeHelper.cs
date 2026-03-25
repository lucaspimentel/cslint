using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal static class VarTypeHelper
{
    internal static bool IsTypeApparent(ExpressionSyntax expression) =>
        expression is ObjectCreationExpressionSyntax or
                      ImplicitObjectCreationExpressionSyntax or
                      CastExpressionSyntax or
                      DefaultExpressionSyntax or
                      ArrayCreationExpressionSyntax;

    internal static bool IsBuiltInType(TypeSyntax type)
    {
        string text = type.ToString();
        return text is "int" or "long" or "short" or "byte" or "sbyte" or
                      "uint" or "ulong" or "ushort" or
                      "float" or "double" or "decimal" or
                      "bool" or "char" or "string" or "object";
    }

    internal static bool LiteralMatchesDeclaredType(ExpressionSyntax literal, TypeSyntax declaredType)
    {
        string type = declaredType.ToString();
        string literalText = literal.ToString();

        return literal.Kind() switch
        {
            SyntaxKind.StringLiteralExpression => type is "string",
            SyntaxKind.CharacterLiteralExpression => type is "char",
            SyntaxKind.TrueLiteralExpression or SyntaxKind.FalseLiteralExpression => type is "bool",
            SyntaxKind.NumericLiteralExpression => MatchesNumericType(literalText, type),
            _ => false,
        };
    }

    private static bool MatchesNumericType(string literalText, string declaredType)
    {
        ReadOnlySpan<char> span = literalText.AsSpan().TrimEnd();

        if (span.Length == 0)
        {
            return false;
        }

        if (span.Length >= 2)
        {
            char secondToLast = char.ToUpperInvariant(span[^2]);
            char last = char.ToUpperInvariant(span[^1]);

            if ((secondToLast == 'U' && last == 'L') || (secondToLast == 'L' && last == 'U'))
            {
                return declaredType is "ulong";
            }
        }

        char suffix = char.ToUpperInvariant(span[^1]);

        if (suffix == 'U')
        {
            return declaredType is "uint";
        }

        if (suffix == 'L')
        {
            return declaredType is "long";
        }

        if (suffix == 'M')
        {
            return declaredType is "decimal";
        }

        if (suffix == 'F')
        {
            return declaredType is "float";
        }

        if (suffix == 'D' && !span.StartsWith("0X", StringComparison.OrdinalIgnoreCase))
        {
            return declaredType is "double";
        }

        if (span.Contains('.'))
        {
            return declaredType is "double";
        }

        return declaredType is "int";
    }
}
