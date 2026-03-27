using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

internal enum UsingDirectiveKind
{
    Regular,
    Static,
    Alias,
}

internal readonly record struct ClassifiedUsingGroup(
    List<UsingDirectiveSyntax> Regular,
    List<UsingDirectiveSyntax> Static,
    List<UsingDirectiveSyntax> Alias);

internal static class UsingDirectiveHelper
{
    public static string GetUsingName(UsingDirectiveSyntax u) =>
        u.NamespaceOrType?.ToString() ?? u.Name?.ToString() ?? string.Empty;

    public static UsingDirectiveKind Classify(UsingDirectiveSyntax u)
    {
        if (u.Alias is not null)
        {
            return UsingDirectiveKind.Alias;
        }

        if (u.StaticKeyword != default)
        {
            return UsingDirectiveKind.Static;
        }

        return UsingDirectiveKind.Regular;
    }

    public static bool IsSystemNamespace(string name) =>
        name == "System" || name.StartsWith("System.", StringComparison.Ordinal);

    /// <summary>
    /// Classifies a list of using directives into regular, static, and alias groups.
    /// </summary>
    public static ClassifiedUsingGroup ClassifyGroup(SyntaxList<UsingDirectiveSyntax> usings)
    {
        var regular = new List<UsingDirectiveSyntax>();
        var staticUsings = new List<UsingDirectiveSyntax>();
        var aliasUsings = new List<UsingDirectiveSyntax>();

        foreach (UsingDirectiveSyntax u in usings)
        {
            switch (Classify(u))
            {
                case UsingDirectiveKind.Alias:
                    aliasUsings.Add(u);
                    break;
                case UsingDirectiveKind.Static:
                    staticUsings.Add(u);
                    break;
                default:
                    regular.Add(u);
                    break;
            }
        }

        return new ClassifiedUsingGroup(regular, staticUsings, aliasUsings);
    }

    /// <summary>
    /// Collects all using directive groups from a syntax tree root (top-level and within namespaces).
    /// </summary>
    public static void ForEachUsingGroup(CSharpSyntaxNode root, Action<SyntaxList<UsingDirectiveSyntax>> action)
    {
        if (root is CompilationUnitSyntax compilationUnit && compilationUnit.Usings.Count > 0)
        {
            action(compilationUnit.Usings);
        }

        foreach (BaseNamespaceDeclarationSyntax ns in root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
        {
            if (ns.Usings.Count > 0)
            {
                action(ns.Usings);
            }
        }
    }
}
