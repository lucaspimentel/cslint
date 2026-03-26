using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class StaticHolderShouldBeSealedRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1052.severity";

    public string RuleId => "CA1052";

    public string Name => "StaticHolderTypesShouldBeSealed";

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

    private static bool HasOnlyStaticMembers(TypeDeclarationSyntax typeDecl)
    {
        if (typeDecl.Members.Count == 0)
        {
            return false;
        }

        foreach (MemberDeclarationSyntax member in typeDecl.Members)
        {
            SyntaxTokenList modifiers = member switch
            {
                MethodDeclarationSyntax m => m.Modifiers,
                PropertyDeclarationSyntax p => p.Modifiers,
                FieldDeclarationSyntax f => f.Modifiers,
                EventDeclarationSyntax e => e.Modifiers,
                EventFieldDeclarationSyntax ef => ef.Modifiers,
                ConstructorDeclarationSyntax c => c.Modifiers,
                // Nested types, operators, conversion operators are inherently static
                TypeDeclarationSyntax => default,
                OperatorDeclarationSyntax => default,
                ConversionOperatorDeclarationSyntax => default,
                EnumDeclarationSyntax => default,
                DelegateDeclarationSyntax => default,
                _ => default,
            };

            if (modifiers == default)
            {
                continue; // nested types etc. are fine
            }

            // Instance constructors disqualify
            if (member is ConstructorDeclarationSyntax ctor)
            {
                if (!HasModifier(ctor.Modifiers, SyntaxKind.StaticKeyword))
                {
                    return false;
                }

                continue;
            }

            // Const fields are effectively static
            if (member is FieldDeclarationSyntax field && HasModifier(field.Modifiers, SyntaxKind.ConstKeyword))
            {
                continue;
            }

            if (!HasModifier(modifiers, SyntaxKind.StaticKeyword))
            {
                return false;
            }
        }

        return true;
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

        foreach (ClassDeclarationSyntax classDecl in context.Root.DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            // Skip if already static, sealed, or abstract
            if (HasModifier(classDecl.Modifiers, SyntaxKind.StaticKeyword) ||
                HasModifier(classDecl.Modifiers, SyntaxKind.SealedKeyword) ||
                HasModifier(classDecl.Modifiers, SyntaxKind.AbstractKeyword) ||
                HasModifier(classDecl.Modifiers, SyntaxKind.PartialKeyword))
            {
                continue;
            }

            // Skip if it has a base type (might be inheriting instance members)
            if (classDecl.BaseList is not null)
            {
                continue;
            }

            if (!HasOnlyStaticMembers(classDecl))
            {
                continue;
            }

            FileLinePositionSpan span = classDecl.Identifier.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Static holder type '{classDecl.Identifier.Text}' should be static or sealed",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
