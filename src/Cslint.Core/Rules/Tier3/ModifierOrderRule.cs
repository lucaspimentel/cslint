using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ModifierOrderRule : IRuleDefinition
{
    public string RuleId => "CSLINT205";

    public string Name => "ModifierOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_preferred_modifier_order"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_preferred_modifier_order") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? orderStr, string? _) = context.Configuration.GetValueWithSeverity("csharp_preferred_modifier_order");

        if (orderStr is null)
        {
            return [];
        }

        string[] preferredOrder = orderStr.Split(',', StringSplitOptions.TrimEntries);
        var orderMap = new Dictionary<string, int>(StringComparer.Ordinal);

        for (int i = 0; i < preferredOrder.Length; i++)
        {
            orderMap[preferredOrder[i]] = i;
        }

        var walker = new ModifierWalker(context.FilePath, orderMap);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class ModifierWalker(string filePath, Dictionary<string, int> orderMap) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            CheckModifiers(node.Modifiers);
            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            CheckModifiers(node.Modifiers);
            base.VisitStructDeclaration(node);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            CheckModifiers(node.Modifiers);
            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            CheckModifiers(node.Modifiers);
            base.VisitPropertyDeclaration(node);
        }

        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            CheckModifiers(node.Modifiers);
            base.VisitFieldDeclaration(node);
        }

        public override void VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
        {
            CheckModifiers(node.Modifiers);
            base.VisitEventFieldDeclaration(node);
        }

        private void CheckModifiers(SyntaxTokenList modifiers)
        {
            if (modifiers.Count < 2)
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

                        Diagnostics.Add(
                            new LintDiagnostic
                            {
                                RuleId = "CSLINT205",
                                Message = "Modifiers are not in the preferred order",
                                Severity = LintSeverity.Warning,
                                FilePath = filePath,
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
}
