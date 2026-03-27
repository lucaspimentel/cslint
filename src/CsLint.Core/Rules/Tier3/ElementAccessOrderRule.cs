using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ElementAccessOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_element_access_ordering";

    public string RuleId => "SA1202";

    public string Name => "ElementAccessOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    /// <summary>
    /// Returns a numeric rank for access modifier ordering:
    /// public (0) → internal (1) → protected internal (2) → protected (3) → private protected (4) → private (5) → implicit private (6).
    /// Lower rank = more visible = should come first.
    /// </summary>
    private static int GetAccessRank(SyntaxTokenList modifiers)
    {
        bool hasPublic = modifiers.Any(SyntaxKind.PublicKeyword);
        bool hasInternal = modifiers.Any(SyntaxKind.InternalKeyword);
        bool hasProtected = modifiers.Any(SyntaxKind.ProtectedKeyword);
        bool hasPrivate = modifiers.Any(SyntaxKind.PrivateKeyword);

        if (hasPublic)
        {
            return 0;
        }

        if (hasInternal && !hasProtected)
        {
            return 1;
        }

        if (hasProtected && hasInternal)
        {
            return 2;
        }

        if (hasProtected && !hasPrivate)
        {
            return 3;
        }

        if (hasPrivate && hasProtected)
        {
            return 4;
        }

        if (hasPrivate)
        {
            return 5;
        }

        // No access modifier — implicit private
        return 6;
    }

    private static string GetAccessLabel(SyntaxTokenList modifiers)
    {
        bool hasPublic = modifiers.Any(SyntaxKind.PublicKeyword);
        bool hasInternal = modifiers.Any(SyntaxKind.InternalKeyword);
        bool hasProtected = modifiers.Any(SyntaxKind.ProtectedKeyword);
        bool hasPrivate = modifiers.Any(SyntaxKind.PrivateKeyword);

        if (hasPublic)
        {
            return "public";
        }

        if (hasInternal && !hasProtected)
        {
            return "internal";
        }

        if (hasProtected && hasInternal)
        {
            return "protected internal";
        }

        if (hasProtected && !hasPrivate)
        {
            return "protected";
        }

        if (hasPrivate && hasProtected)
        {
            return "private protected";
        }

        if (hasPrivate)
        {
            return "private";
        }

        return "implicit private";
    }

    private static SyntaxKind GetMemberKind(MemberDeclarationSyntax member) =>
        member switch
        {
            FieldDeclarationSyntax => SyntaxKind.FieldDeclaration,
            ConstructorDeclarationSyntax => SyntaxKind.ConstructorDeclaration,
            MethodDeclarationSyntax => SyntaxKind.MethodDeclaration,
            PropertyDeclarationSyntax => SyntaxKind.PropertyDeclaration,
            EventDeclarationSyntax => SyntaxKind.EventDeclaration,
            EventFieldDeclarationSyntax => SyntaxKind.EventFieldDeclaration,
            IndexerDeclarationSyntax => SyntaxKind.IndexerDeclaration,
            OperatorDeclarationSyntax => SyntaxKind.OperatorDeclaration,
            DestructorDeclarationSyntax => SyntaxKind.DestructorDeclaration,
            DelegateDeclarationSyntax => SyntaxKind.DelegateDeclaration,
            _ => SyntaxKind.None,
        };

    private static FileLinePositionSpan GetMemberLocation(MemberDeclarationSyntax member) =>
        member switch
        {
            FieldDeclarationSyntax f => f.Declaration.Variables[0].Identifier.GetLocation().GetLineSpan(),
            ConstructorDeclarationSyntax c => c.Identifier.GetLocation().GetLineSpan(),
            MethodDeclarationSyntax m => m.Identifier.GetLocation().GetLineSpan(),
            PropertyDeclarationSyntax p => p.Identifier.GetLocation().GetLineSpan(),
            EventDeclarationSyntax e => e.Identifier.GetLocation().GetLineSpan(),
            EventFieldDeclarationSyntax ef => ef.Declaration.Variables[0].Identifier.GetLocation().GetLineSpan(),
            IndexerDeclarationSyntax i => i.ThisKeyword.GetLocation().GetLineSpan(),
            OperatorDeclarationSyntax o => o.OperatorToken.GetLocation().GetLineSpan(),
            DestructorDeclarationSyntax d => d.Identifier.GetLocation().GetLineSpan(),
            DelegateDeclarationSyntax del => del.Identifier.GetLocation().GetLineSpan(),
            _ => member.GetLocation().GetLineSpan(),
        };

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (TypeDeclarationSyntax typeDecl in context.Root.DescendantNodes().OfType<TypeDeclarationSyntax>())
        {
            // Group members by kind and static/instance, then check access ordering within each group
            var membersByGroup = new Dictionary<(SyntaxKind Kind, bool IsStatic), List<MemberDeclarationSyntax>>();

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                SyntaxKind kind = GetMemberKind(member);

                if (kind == SyntaxKind.None)
                {
                    continue;
                }

                bool isStatic = member.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                                member.Modifiers.Any(SyntaxKind.ConstKeyword);

                (SyntaxKind, bool) key = (kind, isStatic);

                if (!membersByGroup.TryGetValue(key, out List<MemberDeclarationSyntax>? list))
                {
                    list = [];
                    membersByGroup[key] = list;
                }

                list.Add(member);
            }

            foreach (((SyntaxKind, bool) _, List<MemberDeclarationSyntax> members) in membersByGroup)
            {
                int highestAccessRank = -1;

                foreach (MemberDeclarationSyntax member in members)
                {
                    int rank = GetAccessRank(member.Modifiers);

                    if (rank < highestAccessRank)
                    {
                        string accessLabel = GetAccessLabel(member.Modifiers);
                        FileLinePositionSpan span = GetMemberLocation(member);

                        (diagnostics ??= []).Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message = $"A {accessLabel} member must not follow a less visible member of the same kind",
                                Severity = LintSeverity.Warning,
                                FilePath = span.Path,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });
                    }
                    else
                    {
                        highestAccessRank = rank;
                    }
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
