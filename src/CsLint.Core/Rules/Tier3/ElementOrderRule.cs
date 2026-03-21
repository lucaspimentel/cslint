using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ElementOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_element_ordering";

    public string RuleId => "CSLINT268";

    public string Name => "ElementOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    /// <summary>
    /// Returns canonical ordering rank per SA1201:
    /// const field (0) → field (1) → constructor (2) → destructor (3) → delegate (4) →
    /// event (5) → enum (6) → interface (7) → property (8) → indexer (9) →
    /// operator (10) → method (11) → struct (12) → class (13) → record (14).
    /// </summary>
    private static int GetKindRank(MemberDeclarationSyntax member) =>
        member switch
        {
            FieldDeclarationSyntax f when f.Modifiers.Any(SyntaxKind.ConstKeyword) => 0,
            FieldDeclarationSyntax => 1,
            ConstructorDeclarationSyntax => 2,
            DestructorDeclarationSyntax => 3,
            DelegateDeclarationSyntax => 4,
            EventDeclarationSyntax => 5,
            EventFieldDeclarationSyntax => 5,
            EnumDeclarationSyntax => 6,
            InterfaceDeclarationSyntax => 7,
            PropertyDeclarationSyntax => 8,
            IndexerDeclarationSyntax => 9,
            OperatorDeclarationSyntax => 10,
            ConversionOperatorDeclarationSyntax => 10,
            MethodDeclarationSyntax => 11,
            StructDeclarationSyntax => 12,
            ClassDeclarationSyntax => 13,
            RecordDeclarationSyntax => 14,
            _ => -1,
        };

    private static string GetKindName(MemberDeclarationSyntax member) =>
        member switch
        {
            FieldDeclarationSyntax f when f.Modifiers.Any(SyntaxKind.ConstKeyword) => "constant",
            FieldDeclarationSyntax => "field",
            ConstructorDeclarationSyntax => "constructor",
            DestructorDeclarationSyntax => "destructor",
            DelegateDeclarationSyntax => "delegate",
            EventDeclarationSyntax => "event",
            EventFieldDeclarationSyntax => "event",
            EnumDeclarationSyntax => "enum",
            InterfaceDeclarationSyntax => "interface",
            PropertyDeclarationSyntax => "property",
            IndexerDeclarationSyntax => "indexer",
            OperatorDeclarationSyntax => "operator",
            ConversionOperatorDeclarationSyntax => "operator",
            MethodDeclarationSyntax => "method",
            StructDeclarationSyntax => "struct",
            ClassDeclarationSyntax => "class",
            RecordDeclarationSyntax => "record",
            _ => "member",
        };

    private static FileLinePositionSpan GetMemberLocation(MemberDeclarationSyntax member) =>
        member switch
        {
            FieldDeclarationSyntax f => f.Declaration.Variables[0].Identifier.GetLocation().GetLineSpan(),
            ConstructorDeclarationSyntax c => c.Identifier.GetLocation().GetLineSpan(),
            DestructorDeclarationSyntax d => d.Identifier.GetLocation().GetLineSpan(),
            DelegateDeclarationSyntax del => del.Identifier.GetLocation().GetLineSpan(),
            MethodDeclarationSyntax m => m.Identifier.GetLocation().GetLineSpan(),
            PropertyDeclarationSyntax p => p.Identifier.GetLocation().GetLineSpan(),
            EventDeclarationSyntax e => e.Identifier.GetLocation().GetLineSpan(),
            EventFieldDeclarationSyntax ef => ef.Declaration.Variables[0].Identifier.GetLocation().GetLineSpan(),
            IndexerDeclarationSyntax i => i.ThisKeyword.GetLocation().GetLineSpan(),
            OperatorDeclarationSyntax o => o.OperatorToken.GetLocation().GetLineSpan(),
            ConversionOperatorDeclarationSyntax co => co.OperatorKeyword.GetLocation().GetLineSpan(),
            TypeDeclarationSyntax t => t.Identifier.GetLocation().GetLineSpan(),
            EnumDeclarationSyntax en => en.Identifier.GetLocation().GetLineSpan(),
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
            int highestKindRank = -1;
            string? highestKindName = null;

            foreach (MemberDeclarationSyntax member in typeDecl.Members)
            {
                int rank = GetKindRank(member);

                if (rank < 0)
                {
                    continue;
                }

                if (rank < highestKindRank)
                {
                    string kindName = GetKindName(member);
                    FileLinePositionSpan span = GetMemberLocation(member);

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = $"A {kindName} must not follow a {highestKindName}",
                            Severity = LintSeverity.Warning,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
                else if (rank > highestKindRank)
                {
                    highestKindRank = rank;
                    highestKindName = GetKindName(member);
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
