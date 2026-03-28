using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

internal static class FieldNamingHelper
{
    public static bool ShouldSkipField(FieldDeclarationSyntax node) =>
        node.Modifiers.Any(SyntaxKind.ConstKeyword) || HasStructLayoutAttribute(node);

    public static bool IsPrivate(FieldDeclarationSyntax node) =>
        !node.Modifiers.Any(SyntaxKind.PublicKeyword) &&
        !node.Modifiers.Any(SyntaxKind.ProtectedKeyword) &&
        !node.Modifiers.Any(SyntaxKind.InternalKeyword);

    public static bool IsStatic(FieldDeclarationSyntax node) =>
        node.Modifiers.Any(SyntaxKind.StaticKeyword);

    public static bool IsReadOnly(FieldDeclarationSyntax node) =>
        node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);

    public static void CheckFields(
        FieldDeclarationSyntax node,
        List<LintDiagnostic> diagnostics,
        Func<string, bool> isValidName,
        string ruleId,
        string fieldKind,
        string expectedCase)
    {
        foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
        {
            string name = variable.Identifier.ValueText;

            if (!isValidName(name))
            {
                FileLinePositionSpan span = variable.Identifier.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = ruleId,
                        Message = $"{fieldKind} '{name}' should use {expectedCase}",
                        Severity = LintSeverity.Warning,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }

    private static bool HasStructLayoutAttribute(FieldDeclarationSyntax node)
    {
        if (node.Parent is not TypeDeclarationSyntax typeDecl)
        {
            return false;
        }

        foreach (AttributeListSyntax attrList in typeDecl.AttributeLists)
        {
            foreach (AttributeSyntax attr in attrList.Attributes)
            {
                string attrName = attr.Name.ToString();

                if (attrName is "StructLayout" or "StructLayoutAttribute" or
                    "System.Runtime.InteropServices.StructLayout" or
                    "System.Runtime.InteropServices.StructLayoutAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
