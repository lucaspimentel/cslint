using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// Shared helper for detecting instance state mutations within struct members.
/// Used by IDE0250 (readonly struct) and IDE0251 (readonly struct member).
/// </summary>
internal static class StructMutationHelper
{
    /// <summary>
    /// Checks whether the given syntax node subtree contains mutations to instance state.
    /// Detects assignments to instance fields, this-assignments, increments/decrements, and ref/out usage.
    /// </summary>
    public static bool MutatesInstanceState(SyntaxNode body, HashSet<string> instanceFieldNames)
    {
        foreach (SyntaxNode node in body.DescendantNodes())
        {
            switch (node)
            {
                case AssignmentExpressionSyntax assignment:
                    if (IsInstanceFieldTarget(assignment.Left, instanceFieldNames) ||
                        IsThisAssignment(assignment.Left))
                    {
                        return true;
                    }

                    break;

                case PrefixUnaryExpressionSyntax prefix
                    when prefix.IsKind(SyntaxKind.PreIncrementExpression) ||
                         prefix.IsKind(SyntaxKind.PreDecrementExpression):
                    if (IsInstanceFieldTarget(prefix.Operand, instanceFieldNames))
                    {
                        return true;
                    }

                    break;

                case PostfixUnaryExpressionSyntax postfix
                    when postfix.IsKind(SyntaxKind.PostIncrementExpression) ||
                         postfix.IsKind(SyntaxKind.PostDecrementExpression):
                    if (IsInstanceFieldTarget(postfix.Operand, instanceFieldNames))
                    {
                        return true;
                    }

                    break;

                case ArgumentSyntax { RefKindKeyword.RawKind: not 0 } arg:
                    if (IsInstanceFieldTarget(arg.Expression, instanceFieldNames) ||
                        arg.Expression is ThisExpressionSyntax)
                    {
                        return true;
                    }

                    break;
            }
        }

        return false;
    }

    /// <summary>
    /// Collects the names of all instance (non-static, non-const) fields in a struct.
    /// </summary>
    public static HashSet<string> CollectInstanceFieldNames(StructDeclarationSyntax structDecl)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);

        foreach (MemberDeclarationSyntax member in structDecl.Members)
        {
            if (member is not FieldDeclarationSyntax field)
            {
                continue;
            }

            if (field.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                field.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                names.Add(variable.Identifier.Text);
            }
        }

        return names;
    }

    private static bool IsInstanceFieldTarget(ExpressionSyntax expression, HashSet<string> instanceFieldNames) =>
        expression switch
        {
            IdentifierNameSyntax id => instanceFieldNames.Contains(id.Identifier.Text),
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: IdentifierNameSyntax name }
                => instanceFieldNames.Contains(name.Identifier.Text),
            _ => false,
        };

    private static bool IsThisAssignment(ExpressionSyntax target) =>
        target is ThisExpressionSyntax;
}
