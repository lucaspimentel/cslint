using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class IdentifierUnderscoreRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1707.severity";

    public string RuleId => "CA1707";

    public string Name => "IdentifiersShouldNotContainUnderscores";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
    {
        foreach (SyntaxToken modifier in modifiers)
        {
            if (modifier.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsVisible(SyntaxTokenList modifiers) =>
        HasModifier(modifiers, SyntaxKind.PublicKeyword) ||
        HasModifier(modifiers, SyntaxKind.ProtectedKeyword);

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            switch (node)
            {
                case MethodDeclarationSyntax method
                    when IsVisible(method.Modifiers) &&
                        !HasModifier(method.Modifiers, SyntaxKind.OverrideKeyword) &&
                        method.Identifier.Text.Contains('_'):
                    Report(method.Identifier, context.FilePath, ref diagnostics);
                    break;

                case PropertyDeclarationSyntax property
                    when IsVisible(property.Modifiers) &&
                        property.Identifier.Text.Contains('_'):
                    Report(property.Identifier, context.FilePath, ref diagnostics);
                    break;

                case EventDeclarationSyntax eventDecl
                    when IsVisible(eventDecl.Modifiers) &&
                        eventDecl.Identifier.Text.Contains('_'):
                    Report(eventDecl.Identifier, context.FilePath, ref diagnostics);
                    break;

                case TypeDeclarationSyntax typeDecl
                    when IsVisible(typeDecl.Modifiers) &&
                        typeDecl.Identifier.Text.Contains('_'):
                    Report(typeDecl.Identifier, context.FilePath, ref diagnostics);
                    break;

                case EnumMemberDeclarationSyntax enumMember
                    when enumMember.Identifier.Text.Contains('_'):
                    Report(enumMember.Identifier, context.FilePath, ref diagnostics);
                    break;
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void Report(
        SyntaxToken identifier,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

        (diagnostics ??= []).Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Identifier '{identifier.Text}' should not "
                    + "contain underscores",
                Severity = DefaultSeverity,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
