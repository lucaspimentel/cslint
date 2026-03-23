using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class TopLevelStatementsRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_prefer_top_level_statements";

    public string RuleId => "IDE0210";

    public string Name => "TopLevelStatements";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsMainMethod(MethodDeclarationSyntax method) =>
        method.Identifier.Text == "Main"
        && method.Modifiers.Any(SyntaxKind.StaticKeyword);

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (node is not MethodDeclarationSyntax method)
        {
            return;
        }

        if (!IsMainMethod(method))
        {
            return;
        }

        // Must be inside a type declaration
        if (method.Parent is not TypeDeclarationSyntax typeDecl)
        {
            return;
        }

        // The file should have only one type declaration to be a candidate
        SyntaxNode? root = typeDecl.Parent;

        if (root is BaseNamespaceDeclarationSyntax ns)
        {
            root = ns.Parent;
        }

        if (root is not CompilationUnitSyntax compilationUnit)
        {
            return;
        }

        // Count type declarations (top-level and inside namespaces)
        int typeCount = 0;

        foreach (MemberDeclarationSyntax member in compilationUnit.Members)
        {
            if (member is TypeDeclarationSyntax)
            {
                typeCount++;
            }
            else if (member is BaseNamespaceDeclarationSyntax nsDecl)
            {
                foreach (MemberDeclarationSyntax nsMember in nsDecl.Members)
                {
                    if (nsMember is TypeDeclarationSyntax)
                    {
                        typeCount++;
                    }
                }
            }
        }

        if (typeCount != 1)
        {
            return;
        }

        FileLinePositionSpan span = method.Identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use top-level statements instead of a Main method",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
