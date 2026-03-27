using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0250: Struct can be made readonly.
/// Flags structs where all instance fields are readonly and no member mutates instance state.
/// </summary>
public sealed class ReadonlyStructPreferenceRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_style_prefer_readonly_struct";

    public string RuleId => "IDE0250";

    public string Name => "ReadonlyStructPreference";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (StructDeclarationSyntax structDecl in context.Root.DescendantNodes().OfType<StructDeclarationSyntax>())
        {
            AnalyzeStruct(structDecl, context.FilePath, ref diagnostics);
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private static void AnalyzeStruct(
        StructDeclarationSyntax structDecl,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        SyntaxTokenList modifiers = structDecl.Modifiers;

        // Skip if already readonly, ref, or partial
        if (modifiers.Any(SyntaxKind.ReadOnlyKeyword) ||
            modifiers.Any(SyntaxKind.RefKeyword) ||
            modifiers.Any(SyntaxKind.PartialKeyword))
        {
            return;
        }

        // Check all instance fields are readonly
        foreach (MemberDeclarationSyntax member in structDecl.Members)
        {
            if (member is not FieldDeclarationSyntax field)
            {
                continue;
            }

            SyntaxTokenList fieldModifiers = field.Modifiers;

            // Skip static and const fields — they don't affect readonly struct eligibility
            if (fieldModifiers.Any(SyntaxKind.StaticKeyword) || fieldModifiers.Any(SyntaxKind.ConstKeyword))
            {
                continue;
            }

            // Any non-readonly instance field means the struct can't be readonly
            if (!fieldModifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                return;
            }
        }

        // All instance fields are readonly (or none exist).
        // Now check if any non-constructor member body mutates instance state.
        // Constructors are allowed to assign fields even in readonly structs.
        HashSet<string> instanceFieldNames = StructMutationHelper.CollectInstanceFieldNames(structDecl);

        foreach (MemberDeclarationSyntax member in structDecl.Members)
        {
            // Skip constructors — they're allowed to assign fields in readonly structs
            if (member is ConstructorDeclarationSyntax)
            {
                continue;
            }

            SyntaxNode? body = GetMemberBody(member);

            if (body is null)
            {
                continue;
            }

            if (StructMutationHelper.MutatesInstanceState(body, instanceFieldNames))
            {
                return;
            }
        }

        FileLinePositionSpan span = structDecl.Identifier.GetLocation().GetLineSpan();

        (diagnostics ??= []).Add(
            new LintDiagnostic
            {
                RuleId = "IDE0250",
                Message = $"Struct '{structDecl.Identifier.Text}' can be made readonly",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private static SyntaxNode? GetMemberBody(MemberDeclarationSyntax member) =>
        member switch
        {
            MethodDeclarationSyntax method => (SyntaxNode?)method.Body ?? method.ExpressionBody,
            PropertyDeclarationSyntax property => GetPropertyBody(property),
            IndexerDeclarationSyntax indexer => GetAccessorListBody(indexer.AccessorList),
            EventDeclarationSyntax eventDecl => GetAccessorListBody(eventDecl.AccessorList),
            ConstructorDeclarationSyntax ctor => (SyntaxNode?)ctor.Body ?? ctor.ExpressionBody,
            OperatorDeclarationSyntax op => (SyntaxNode?)op.Body ?? op.ExpressionBody,
            ConversionOperatorDeclarationSyntax conv => (SyntaxNode?)conv.Body ?? conv.ExpressionBody,
            _ => null,
        };

    private static SyntaxNode? GetPropertyBody(PropertyDeclarationSyntax property)
    {
        if (property.ExpressionBody is not null)
        {
            return property.ExpressionBody;
        }

        return property.AccessorList;
    }

    private static SyntaxNode? GetAccessorListBody(AccessorListSyntax? accessorList) =>
        accessorList;
}
