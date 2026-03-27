using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0251: Member can be made readonly.
/// Flags struct methods, properties, and indexers that don't mutate instance state.
/// </summary>
public sealed class ReadonlyStructMemberPreferenceRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_style_prefer_readonly_struct_member";

    public string RuleId => "IDE0251";

    public string Name => "ReadonlyStructMemberPreference";

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
        // Skip if already a readonly struct — all members are implicitly readonly
        if (structDecl.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
        {
            return;
        }

        HashSet<string> instanceFieldNames = StructMutationHelper.CollectInstanceFieldNames(structDecl);

        foreach (MemberDeclarationSyntax member in structDecl.Members)
        {
            CheckMember(member, instanceFieldNames, filePath, ref diagnostics);
        }
    }

    private static void CheckMember(
        MemberDeclarationSyntax member,
        HashSet<string> instanceFieldNames,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        switch (member)
        {
            case MethodDeclarationSyntax method:
                CheckMethod(method, instanceFieldNames, filePath, ref diagnostics);
                break;

            case PropertyDeclarationSyntax property:
                CheckProperty(property, instanceFieldNames, filePath, ref diagnostics);
                break;

            case IndexerDeclarationSyntax indexer:
                CheckIndexer(indexer, instanceFieldNames, filePath, ref diagnostics);
                break;
        }
    }

    private static void CheckMethod(
        MethodDeclarationSyntax method,
        HashSet<string> instanceFieldNames,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        if (ShouldSkipMember(method.Modifiers))
        {
            return;
        }

        SyntaxNode? body = (SyntaxNode?)method.Body ?? method.ExpressionBody;

        if (body is null)
        {
            return;
        }

        if (StructMutationHelper.MutatesInstanceState(body, instanceFieldNames))
        {
            return;
        }

        ReportDiagnostic(method.Identifier, $"Method '{method.Identifier.Text}'", filePath, ref diagnostics);
    }

    private static void CheckProperty(
        PropertyDeclarationSyntax property,
        HashSet<string> instanceFieldNames,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        if (ShouldSkipMember(property.Modifiers))
        {
            return;
        }

        // Expression-bodied property
        if (property.ExpressionBody is not null)
        {
            if (!StructMutationHelper.MutatesInstanceState(property.ExpressionBody, instanceFieldNames))
            {
                ReportDiagnostic(property.Identifier, $"Property '{property.Identifier.Text}'", filePath, ref diagnostics);
            }

            return;
        }

        // Auto-property (no body to check) — skip, auto-properties are implicitly handled
        if (property.AccessorList is null)
        {
            return;
        }

        // Check each accessor
        foreach (AccessorDeclarationSyntax accessor in property.AccessorList.Accessors)
        {
            SyntaxNode? accessorBody = (SyntaxNode?)accessor.Body ?? accessor.ExpressionBody;

            if (accessorBody is null)
            {
                continue;
            }

            if (StructMutationHelper.MutatesInstanceState(accessorBody, instanceFieldNames))
            {
                // If any accessor mutates state, the whole property can't be readonly
                return;
            }
        }

        ReportDiagnostic(property.Identifier, $"Property '{property.Identifier.Text}'", filePath, ref diagnostics);
    }

    private static void CheckIndexer(
        IndexerDeclarationSyntax indexer,
        HashSet<string> instanceFieldNames,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        if (ShouldSkipMember(indexer.Modifiers))
        {
            return;
        }

        if (indexer.AccessorList is null && indexer.ExpressionBody is null)
        {
            return;
        }

        SyntaxNode body = (SyntaxNode?)indexer.ExpressionBody ?? indexer.AccessorList!;

        if (StructMutationHelper.MutatesInstanceState(body, instanceFieldNames))
        {
            return;
        }

        ReportDiagnostic(indexer.ThisKeyword, "Indexer", filePath, ref diagnostics);
    }

    private static bool ShouldSkipMember(SyntaxTokenList modifiers) =>
        modifiers.Any(SyntaxKind.ReadOnlyKeyword) ||
        modifiers.Any(SyntaxKind.StaticKeyword);

    private static void ReportDiagnostic(
        SyntaxToken token,
        string memberDescription,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        (diagnostics ??= []).Add(
            new LintDiagnostic
            {
                RuleId = "IDE0251",
                Message = $"{memberDescription} can be made readonly",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
