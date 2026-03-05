using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PatternMatchingRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT209";

    public string Name => "PatternMatching";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_pattern_matching_over_is_with_cast_check"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitIfStatement(IfStatementSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (node.Condition is BinaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.IsExpression,
                Right: TypeSyntax,
            })
        {
            bool hasCast = node.Statement.DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Any();

            if (hasCast)
            {
                FileLinePositionSpan span = node.IfKeyword.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT209",
                        Message = "Use pattern matching ('is Type name') instead of 'is' check followed by cast",
                        Severity = LintSeverity.Info,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
