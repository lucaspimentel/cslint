using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class StaticMemberOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_static_before_instance";

    public string RuleId => "CSLINT266";

    public string Name => "StaticMemberOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

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
            // Group members by kind, then check static-before-instance within each group
            var membersByKind = new Dictionary<SyntaxKind, List<MemberDeclarationSyntax>>();

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                SyntaxKind kind = GetMemberKind(member);

                if (kind == SyntaxKind.None)
                {
                    continue;
                }

                if (!membersByKind.TryGetValue(kind, out List<MemberDeclarationSyntax>? list))
                {
                    list = [];
                    membersByKind[kind] = list;
                }

                list.Add(member);
            }

            foreach ((SyntaxKind _, List<MemberDeclarationSyntax> members) in membersByKind)
            {
                bool seenInstanceMember = false;

                foreach (MemberDeclarationSyntax member in members)
                {
                    bool isStatic = member.Modifiers.Any(SyntaxKind.StaticKeyword);
                    bool isConst = member.Modifiers.Any(SyntaxKind.ConstKeyword);

                    // const fields are implicitly static, skip them
                    if (isConst)
                    {
                        continue;
                    }

                    if (isStatic)
                    {
                        if (seenInstanceMember)
                        {
                            FileLinePositionSpan span = GetMemberLocation(member);

                            (diagnostics ??= []).Add(
                                new LintDiagnostic
                                {
                                    RuleId = RuleId,
                                    Message = "Static members must appear before instance members of the same kind",
                                    Severity = LintSeverity.Warning,
                                    FilePath = span.Path,
                                    Line = span.StartLinePosition.Line + 1,
                                    Column = span.StartLinePosition.Character + 1,
                                });
                        }
                    }
                    else
                    {
                        seenInstanceMember = true;
                    }
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
