using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ModifierOrderRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT205";

    public string Name => "ModifierOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_preferred_modifier_order"];

    private Dictionary<string, int>? _orderMap;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_preferred_modifier_order") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        Initialize(context.Configuration);

        if (_orderMap is null)
        {
            return [];
        }

        var walker = new CombinedStyleWalker([this]);
        walker.Visit(context.Root);
        _orderMap = null;
        return walker.Diagnostics;
    }

    internal void Initialize(LintConfiguration config)
    {
        (string? orderStr, string? _) = config.GetValueWithSeverity("csharp_preferred_modifier_order");

        if (orderStr is null)
        {
            _orderMap = null;
            return;
        }

        string[] preferredOrder = orderStr.Split(',', StringSplitOptions.TrimEntries);
        _orderMap = new Dictionary<string, int>(StringComparer.Ordinal);

        for (int i = 0; i < preferredOrder.Length; i++)
        {
            _orderMap[preferredOrder[i]] = i;
        }
    }

    internal void Reset() => _orderMap = null;

    void IStyleRuleHandler.VisitClassDeclaration(ClassDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, diagnostics);

    void IStyleRuleHandler.VisitStructDeclaration(StructDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, diagnostics);

    void IStyleRuleHandler.VisitMethodDeclaration(MethodDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, diagnostics);

    void IStyleRuleHandler.VisitPropertyDeclaration(PropertyDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, diagnostics);

    void IStyleRuleHandler.VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, diagnostics);

    void IStyleRuleHandler.VisitEventFieldDeclaration(EventFieldDeclarationSyntax node, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, diagnostics);

    private void CheckModifiers(SyntaxTokenList modifiers, List<LintDiagnostic> diagnostics)
    {
        if (_orderMap is null || modifiers.Count < 2)
        {
            return;
        }

        int lastOrder = -1;

        foreach (SyntaxToken modifier in modifiers)
        {
            string text = modifier.Text;

            if (_orderMap.TryGetValue(text, out int order))
            {
                if (order < lastOrder)
                {
                    FileLinePositionSpan span = modifiers[0].GetLocation().GetLineSpan();

                    diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = "CSLINT205",
                            Message = "Modifiers are not in the preferred order",
                            Severity = LintSeverity.Warning,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }

                lastOrder = order;
            }
        }
    }
}
