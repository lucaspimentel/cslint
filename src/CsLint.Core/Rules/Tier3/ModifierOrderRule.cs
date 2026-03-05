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

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_preferred_modifier_order") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? orderStr, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_preferred_modifier_order");

        if (orderStr is null)
        {
            return [];
        }

        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitClassDeclaration(
        ClassDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, config, diagnostics);

    void IStyleRuleHandler.VisitStructDeclaration(
        StructDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, config, diagnostics);

    void IStyleRuleHandler.VisitMethodDeclaration(
        MethodDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, config, diagnostics);

    void IStyleRuleHandler.VisitPropertyDeclaration(
        PropertyDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, config, diagnostics);

    void IStyleRuleHandler.VisitFieldDeclaration(
        FieldDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, config, diagnostics);

    void IStyleRuleHandler.VisitEventFieldDeclaration(
        EventFieldDeclarationSyntax node, LintConfiguration config, List<LintDiagnostic> diagnostics) =>
        CheckModifiers(node.Modifiers, config, diagnostics);

    private static void CheckModifiers(
        SyntaxTokenList modifiers,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        Dictionary<string, int>? orderMap = BuildOrderMap(config);

        if (orderMap is null || modifiers.Count < 2)
        {
            return;
        }

        int lastOrder = -1;

        foreach (SyntaxToken modifier in modifiers)
        {
            string text = modifier.Text;

            if (orderMap.TryGetValue(text, out int order))
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

    private static Dictionary<string, int>? BuildOrderMap(LintConfiguration config)
    {
        (string? orderStr, string? _) = config.GetValueWithSeverity("csharp_preferred_modifier_order");

        if (orderStr is null)
        {
            return null;
        }

        string[] preferredOrder = orderStr.Split(',', StringSplitOptions.TrimEntries);
        var orderMap = new Dictionary<string, int>(StringComparer.Ordinal);

        for (int i = 0; i < preferredOrder.Length; i++)
        {
            orderMap[preferredOrder[i]] = i;
        }

        return orderMap;
    }
}
