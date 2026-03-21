using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SingleTypePerFileRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_single_type_per_file";

    public string RuleId => "CSLINT252";

    public string Name => "SingleTypePerFile";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static void CollectTopLevelTypes(MemberDeclarationSyntax member, List<TypeDeclarationSyntax> types)
    {
        switch (member)
        {
            case TypeDeclarationSyntax typeDecl:
                types.Add(typeDecl);
                break;

            case NamespaceDeclarationSyntax nsDecl:
                foreach (MemberDeclarationSyntax nsMember in nsDecl.Members)
                {
                    CollectTopLevelTypes(nsMember, types);
                }

                break;

            case FileScopedNamespaceDeclarationSyntax fileNsDecl:
                foreach (MemberDeclarationSyntax nsMember in fileNsDecl.Members)
                {
                    CollectTopLevelTypes(nsMember, types);
                }

                break;
        }
    }

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

        if (context.Root is not CompilationUnitSyntax compilationUnit)
        {
            return [];
        }

        // Collect all top-level type declarations (not nested)
        var topLevelTypes = new List<TypeDeclarationSyntax>();

        foreach (MemberDeclarationSyntax member in compilationUnit.Members)
        {
            CollectTopLevelTypes(member, topLevelTypes);
        }

        if (topLevelTypes.Count <= 1)
        {
            return [];
        }

        // Flag the second and subsequent type declarations
        var diagnostics = new List<LintDiagnostic>();

        for (int i = 1; i < topLevelTypes.Count; i++)
        {
            TypeDeclarationSyntax typeDecl = topLevelTypes[i];
            FileLinePositionSpan span = typeDecl.Keyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"File should contain only a single type declaration, but also contains '{typeDecl.Identifier.Text}'",
                    Severity = DefaultSeverity,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics;
    }
}
