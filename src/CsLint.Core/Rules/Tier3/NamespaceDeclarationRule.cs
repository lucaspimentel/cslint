using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NamespaceDeclarationRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT203";

    public string Name => "NamespaceDeclaration";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_namespace_declarations"];

    private bool _preferFileScoped;
    private bool _active;
    private string _filePath = "";

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_namespace_declarations") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity("csharp_style_namespace_declarations");

        if (pref is null)
        {
            return [];
        }

        _preferFileScoped = string.Equals(pref, "file_scoped", StringComparison.OrdinalIgnoreCase);
        _active = true;
        _filePath = context.FilePath;
        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, diagnostics);
        }

        _active = false;
        return diagnostics;
    }

    internal void Initialize(LintConfiguration config, string filePath)
    {
        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_namespace_declarations");
        _preferFileScoped = string.Equals(pref, "file_scoped", StringComparison.OrdinalIgnoreCase);
        _active = pref is not null;
        _filePath = filePath;
    }

    internal void Reset() => _active = false;

    public void VisitNode(SyntaxNode node, List<LintDiagnostic> diagnostics)
    {
        if (!_active)
        {
            return;
        }

        if (_preferFileScoped && node is NamespaceDeclarationSyntax ns)
        {
            FileLinePositionSpan span = ns.NamespaceKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Use file-scoped namespace declaration",
                    Severity = LintSeverity.Warning,
                    FilePath = _filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
        else if (!_preferFileScoped && node is FileScopedNamespaceDeclarationSyntax fileScopedNs)
        {
            FileLinePositionSpan span = fileScopedNs.NamespaceKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Use block-scoped namespace declaration",
                    Severity = LintSeverity.Warning,
                    FilePath = _filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
