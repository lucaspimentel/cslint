using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class VisibleNestedTypeRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1034.severity";

    public string RuleId => "CA1034";

    public string Name => "NestedTypesShouldNotBeVisible";

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

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (TypeDeclarationSyntax typeDecl in
            context.Root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            // Only check types nested inside another type
            if (typeDecl.Parent is not TypeDeclarationSyntax)
            {
                continue;
            }

            if (!HasModifier(typeDecl.Modifiers, SyntaxKind.PublicKeyword))
            {
                continue;
            }

            // Enums nested in types are a common acceptable pattern
            // but CA1034 flags them too — match .NET behavior
            FileLinePositionSpan span =
                typeDecl.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Nested type '{typeDecl.Identifier.Text}' "
                        + "should not be visible",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
