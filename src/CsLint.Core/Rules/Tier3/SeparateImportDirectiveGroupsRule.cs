using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SeparateImportDirectiveGroupsRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_separate_import_directive_groups";

    public string RuleId => "CSLINT278";

    public string Name => "SeparateImportDirectiveGroups";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static string GetUsingName(UsingDirectiveSyntax u) =>
        u.NamespaceOrType?.ToString() ?? u.Name?.ToString() ?? string.Empty;

    private static bool IsSystem(string name) =>
        name == "System" || name.StartsWith("System.", StringComparison.Ordinal);

    private static int GetGroup(UsingDirectiveSyntax u)
    {
        if (u.StaticKeyword != default)
        {
            return 2; // static usings
        }

        if (u.Alias is not null)
        {
            return 3; // alias usings
        }

        string name = GetUsingName(u);
        return IsSystem(name) ? 0 : 1; // System vs non-System
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetBool(ConfigKey);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!context.Configuration.GetBool(ConfigKey))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        CheckUsings(context.Root, context.FilePath, diagnostics);

        return diagnostics;
    }

    private void CheckUsings(CSharpSyntaxNode root, string filePath, List<LintDiagnostic> diagnostics)
    {
        if (root is CompilationUnitSyntax compilationUnit && compilationUnit.Usings.Count > 0)
        {
            CheckUsingGroup(compilationUnit.Usings, filePath, diagnostics);
        }

        foreach (BaseNamespaceDeclarationSyntax ns in root.DescendantNodes()
                     .OfType<BaseNamespaceDeclarationSyntax>())
        {
            if (ns.Usings.Count > 0)
            {
                CheckUsingGroup(ns.Usings, filePath, diagnostics);
            }
        }
    }

    private void CheckUsingGroup(
        SyntaxList<UsingDirectiveSyntax> usings,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        for (int i = 1; i < usings.Count; i++)
        {
            int prevGroup = GetGroup(usings[i - 1]);
            int currGroup = GetGroup(usings[i]);

            if (prevGroup == currGroup)
            {
                continue;
            }

            // Group transition — check for blank line
            SyntaxToken prevLastToken = usings[i - 1].GetLastToken();
            SyntaxToken currFirstToken = usings[i].GetFirstToken();

            if (!NewLineHelper.HasBlankLineBetween(prevLastToken, currFirstToken))
            {
                FileLinePositionSpan span = usings[i].GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Using directive groups must be separated by a blank line",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
