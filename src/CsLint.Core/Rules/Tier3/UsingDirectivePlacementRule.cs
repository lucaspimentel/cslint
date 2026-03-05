using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class UsingDirectivePlacementRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT207";

    public string Name => "UsingDirectivePlacement";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_using_directive_placement"];

    private bool _preferOutside;
    private bool _active;
    private bool _hasNamespace;
    private string _filePath = "";

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_using_directive_placement") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity("csharp_using_directive_placement");

        if (pref is null)
        {
            return [];
        }

        _preferOutside = string.Equals(pref, "outside_namespace", StringComparison.OrdinalIgnoreCase);
        _active = true;
        _filePath = context.FilePath;
        _hasNamespace = HasNamespace(context.Root);
        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, diagnostics);
        }

        _active = false;
        return diagnostics;
    }

    internal void Initialize(LintConfiguration config, string filePath, SyntaxNode root)
    {
        (string? pref, string? _) = config.GetValueWithSeverity("csharp_using_directive_placement");
        _preferOutside = string.Equals(pref, "outside_namespace", StringComparison.OrdinalIgnoreCase);
        _active = pref is not null;
        _filePath = filePath;
        _hasNamespace = HasNamespace(root);
    }

    internal void Reset() => _active = false;

    public void VisitNode(SyntaxNode node, List<LintDiagnostic> diagnostics)
    {
        if (!_active || node is not UsingDirectiveSyntax usingDirective)
        {
            return;
        }

        bool insideNamespace = usingDirective.Parent is NamespaceDeclarationSyntax;

        if (_preferOutside && insideNamespace)
        {
            FileLinePositionSpan span = usingDirective.UsingKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Using directives should be placed outside the namespace",
                    Severity = LintSeverity.Warning,
                    FilePath = _filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
        else if (!_preferOutside && !insideNamespace && _hasNamespace)
        {
            FileLinePositionSpan span = usingDirective.UsingKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Using directives should be placed inside the namespace",
                    Severity = LintSeverity.Warning,
                    FilePath = _filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }

    private static bool HasNamespace(SyntaxNode root) =>
        root.DescendantNodes().Any(n => n is NamespaceDeclarationSyntax);
}
