using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PatternMatchingRule : IRuleDefinition
{
    public string RuleId => "CSLINT209";

    public string Name => "PatternMatching";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_pattern_matching_over_is_with_cast_check"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new PatternWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class PatternWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            // Detect: if (x is Type) { var y = (Type)x; }
            if (node.Condition is BinaryExpressionSyntax
                {
                    RawKind: (int)SyntaxKind.IsExpression,
                    Right: TypeSyntax,
                })
            {
                // Check if the body contains a cast of the same variable
                bool hasCast = node.Statement.DescendantNodes()
                    .OfType<CastExpressionSyntax>()
                    .Any();

                if (hasCast)
                {
                    FileLinePositionSpan span = node.IfKeyword.GetLocation().GetLineSpan();

                    Diagnostics.Add(
                        new LintDiagnostic
                        {
                            RuleId = "CSLINT209",
                            Message = "Use pattern matching ('is Type name') instead of 'is' check followed by cast",
                            Severity = LintSeverity.Info,
                            FilePath = filePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }

            base.VisitIfStatement(node);
        }
    }
}
