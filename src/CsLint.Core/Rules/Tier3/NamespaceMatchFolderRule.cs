using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NamespaceMatchFolderRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "dotnet_style_namespace_match_folder";

    public string RuleId => "IDE0130";

    public string Name => "NamespaceMatchFolder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static string[] GetDirectorySegments(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(directory))
        {
            return [];
        }

        return directory.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool NamespaceSuffixMatchesFolderSuffix(
        string[] nsSegments,
        string[] dirSegments)
    {
        // Compare from the end — the last N namespace segments should match
        // the last N directory segments (case-insensitive for cross-platform)
        int compareCount = Math.Min(nsSegments.Length, dirSegments.Length);

        if (compareCount == 0)
        {
            return true;
        }

        for (int i = 1; i <= compareCount; i++)
        {
            string nsPart = nsSegments[^i];
            string dirPart = dirSegments[^i];

            if (!string.Equals(nsPart, dirPart, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (node is not BaseNamespaceDeclarationSyntax nsDecl)
        {
            return;
        }

        string namespaceName = nsDecl.Name.ToString();
        string[] nsSegments = namespaceName.Split('.');
        string[] dirSegments = GetDirectorySegments(filePath);

        if (dirSegments.Length == 0)
        {
            return;
        }

        if (NamespaceSuffixMatchesFolderSuffix(nsSegments, dirSegments))
        {
            return;
        }

        FileLinePositionSpan span = nsDecl.Name.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message =
                    $"Namespace '{namespaceName}' does not match folder structure",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
