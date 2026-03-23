using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SortSystemDirectivesFirstRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_sort_system_directives_first";

    public string RuleId => "CSLINT277";

    public string Name => "SortSystemDirectivesFirst";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static string GetUsingName(UsingDirectiveSyntax u) =>
        u.NamespaceOrType?.ToString() ?? u.Name?.ToString() ?? string.Empty;

    private static bool IsSystem(string name) =>
        name == "System" || name.StartsWith("System.", StringComparison.Ordinal);

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
        bool seenNonSystem = false;

        foreach (UsingDirectiveSyntax u in usings)
        {
            // Skip static and alias usings
            if (u.StaticKeyword != default || u.Alias is not null)
            {
                continue;
            }

            string name = GetUsingName(u);

            if (IsSystem(name) && seenNonSystem)
            {
                FileLinePositionSpan span = u.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "System using directives must appear before non-System directives",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break;
            }

            if (!IsSystem(name))
            {
                seenNonSystem = true;
            }
        }
    }
}
