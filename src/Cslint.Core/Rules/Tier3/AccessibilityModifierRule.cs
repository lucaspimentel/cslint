using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class AccessibilityModifierRule : IRuleDefinition
{
    public string RuleId => "CSLINT206";

    public string Name => "AccessibilityModifier";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_require_accessibility_modifiers"];

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity("dotnet_style_require_accessibility_modifiers");
        return string.Equals(pref, "always", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pref, "for_non_interface_members", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new AccessibilityWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class AccessibilityWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            CheckAccessibility(node.Modifiers, node.Keyword, "class", node.Identifier.Text);
            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            CheckAccessibility(node.Modifiers, node.Keyword, "struct", node.Identifier.Text);
            base.VisitStructDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            CheckAccessibility(node.Modifiers, node.Keyword, "interface", node.Identifier.Text);
            base.VisitInterfaceDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            CheckAccessibility(node.Modifiers, node.EnumKeyword, "enum", node.Identifier.Text);
            base.VisitEnumDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            // Skip interface members and explicit interface implementations
            if (node.Parent is not InterfaceDeclarationSyntax && node.ExplicitInterfaceSpecifier is null)
            {
                CheckAccessibility(node.Modifiers, node.ReturnType.GetFirstToken(), "method", node.Identifier.Text);
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            if (node.Parent is not InterfaceDeclarationSyntax && node.ExplicitInterfaceSpecifier is null)
            {
                CheckAccessibility(node.Modifiers, node.Type.GetFirstToken(), "property", node.Identifier.Text);
            }

            base.VisitPropertyDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            if (node.Parent is not InterfaceDeclarationSyntax)
            {
                string name = node.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "";
                CheckAccessibility(node.Modifiers, node.Declaration.Type.GetFirstToken(), "field", name);
            }

            base.VisitFieldDeclaration(node);
        }

        private void CheckAccessibility(SyntaxTokenList modifiers, SyntaxToken fallbackToken, string kind, string name)
        {
            bool hasAccessibility = modifiers.Any(SyntaxKind.PublicKeyword) ||
                                    modifiers.Any(SyntaxKind.PrivateKeyword) ||
                                    modifiers.Any(SyntaxKind.ProtectedKeyword) ||
                                    modifiers.Any(SyntaxKind.InternalKeyword);

            if (!hasAccessibility)
            {
                FileLinePositionSpan span = fallbackToken.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT206",
                        Message = $"Add accessibility modifier to {kind} '{name}'",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
