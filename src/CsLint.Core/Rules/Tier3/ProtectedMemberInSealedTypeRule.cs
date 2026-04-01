using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ProtectedMemberInSealedTypeRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1047.severity";

    public string RuleId => "CA1047";

    public string Name => "DoNotDeclareProtectedMembersInSealedTypes";

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

    private static bool IsSealedType(TypeDeclarationSyntax typeDecl) =>
        HasModifier(typeDecl.Modifiers, SyntaxKind.SealedKeyword);

    private static SyntaxTokenList? GetMemberModifiers(MemberDeclarationSyntax member) =>
        member switch
        {
            MethodDeclarationSyntax m => m.Modifiers,
            PropertyDeclarationSyntax p => p.Modifiers,
            FieldDeclarationSyntax f => f.Modifiers,
            EventDeclarationSyntax e => e.Modifiers,
            EventFieldDeclarationSyntax ef => ef.Modifiers,
            IndexerDeclarationSyntax i => i.Modifiers,
            ConstructorDeclarationSyntax c => c.Modifiers,
            _ => null,
        };

    private static SyntaxToken? GetMemberIdentifier(MemberDeclarationSyntax member) =>
        member switch
        {
            MethodDeclarationSyntax m => m.Identifier,
            PropertyDeclarationSyntax p => p.Identifier,
            EventDeclarationSyntax e => e.Identifier,
            IndexerDeclarationSyntax => null, // indexers don't have a named identifier
            ConstructorDeclarationSyntax c => c.Identifier,
            _ => null,
        };

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (TypeDeclarationSyntax typeDecl in context.Root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            if (!IsSealedType(typeDecl))
            {
                continue;
            }

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                SyntaxTokenList? modifiers = GetMemberModifiers(member);

                if (modifiers is null)
                {
                    continue;
                }

                if (!HasModifier(modifiers.Value, SyntaxKind.ProtectedKeyword))
                {
                    continue;
                }

                // Skip override members — they inherit protected from base class
                if (HasModifier(modifiers.Value, SyntaxKind.OverrideKeyword))
                {
                    continue;
                }

                SyntaxToken? identifier = GetMemberIdentifier(member);

                // For fields/event fields, use the first variable declarator
                SyntaxToken reportToken = identifier ?? member switch
                {
                    FieldDeclarationSyntax f => f.Declaration.Variables.First().Identifier,
                    EventFieldDeclarationSyntax ef => ef.Declaration.Variables.First().Identifier,
                    _ => member.GetFirstToken(),
                };

                FileLinePositionSpan span = reportToken.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Do not declare protected member in sealed type",
                        Severity = DefaultSeverity,
                        FilePath = context.FilePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
