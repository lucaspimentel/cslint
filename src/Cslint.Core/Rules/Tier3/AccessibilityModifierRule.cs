using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class AccessibilityModifierRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT206";

    public string Name => "AccessibilityModifier";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_require_accessibility_modifiers"];

    private bool _active;

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity("dotnet_style_require_accessibility_modifiers");
        return string.Equals(pref, "always", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pref, "for_non_interface_members", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        _active = true;
        var walker = new CombinedStyleWalker([this]);
        walker.Visit(context.Root);
        _active = false;
        return walker.Diagnostics;
    }

    internal void Initialize(LintConfiguration config) => _active = true;

    internal void Reset() => _active = false;

    void IStyleRuleHandler.VisitClassDeclaration(ClassDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckAccessibility(node.Modifiers, node.Keyword, "class", node.Identifier.Text, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitStructDeclaration(StructDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckAccessibility(node.Modifiers, node.Keyword, "struct", node.Identifier.Text, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitInterfaceDeclaration(InterfaceDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckAccessibility(node.Modifiers, node.Keyword, "interface", node.Identifier.Text, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitEnumDeclaration(EnumDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active)
        {
            CheckAccessibility(node.Modifiers, node.EnumKeyword, "enum", node.Identifier.Text, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitMethodDeclaration(MethodDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active && node.Parent is not InterfaceDeclarationSyntax && node.ExplicitInterfaceSpecifier is null)
        {
            CheckAccessibility(node.Modifiers, node.ReturnType.GetFirstToken(), "method", node.Identifier.Text, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitPropertyDeclaration(PropertyDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active && node.Parent is not InterfaceDeclarationSyntax && node.ExplicitInterfaceSpecifier is null)
        {
            CheckAccessibility(node.Modifiers, node.Type.GetFirstToken(), "property", node.Identifier.Text, diagnostics);
        }
    }

    void IStyleRuleHandler.VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_active && node.Parent is not InterfaceDeclarationSyntax)
        {
            string name = node.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "";
            CheckAccessibility(node.Modifiers, node.Declaration.Type.GetFirstToken(), "field", name, diagnostics);
        }
    }

    private static void CheckAccessibility(
        SyntaxTokenList modifiers,
        SyntaxToken fallbackToken,
        string kind,
        string name,
        List<LintDiagnostic> diagnostics)
    {
        bool hasAccessibility = modifiers.Any(SyntaxKind.PublicKeyword) ||
                                modifiers.Any(SyntaxKind.PrivateKeyword) ||
                                modifiers.Any(SyntaxKind.ProtectedKeyword) ||
                                modifiers.Any(SyntaxKind.InternalKeyword);

        if (!hasAccessibility)
        {
            FileLinePositionSpan span = fallbackToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT206",
                    Message = $"Add accessibility modifier to {kind} '{name}'",
                    Severity = LintSeverity.Warning,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
