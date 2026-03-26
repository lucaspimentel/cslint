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

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static void ReportDiagnostic(IfStatementSyntax node, List<LintDiagnostic> diagnostics)
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

    private static bool IsJumpStatement(StatementSyntax statement)
    {
        // Unwrap block with single statement: if (...) { return; }
        if (statement is BlockSyntax { Statements.Count: 1 } blockSyntax)
        {
            statement = blockSyntax.Statements[0];
        }

        return statement is ReturnStatementSyntax
            or ThrowStatementSyntax
            or BreakStatementSyntax
            or ContinueStatementSyntax;
    }

    private static bool TryGetNegatedIsExpression(
        ExpressionSyntax condition,
        out string? target,
        out string? type)
    {
        // Form 1: !(expr is Type)
        if (condition is PrefixUnaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.LogicalNotExpression,
                Operand: ParenthesizedExpressionSyntax
                {
                    Expression: BinaryExpressionSyntax
                    {
                        RawKind: (int)SyntaxKind.IsExpression,
                        Left: var left1,
                        Right: TypeSyntax type1,
                    },
                },
            })
        {
            target = left1.ToString();
            type = type1.ToString();
            return true;
        }

        // Form 2: expr is not Type
        if (condition is IsPatternExpressionSyntax
            {
                Expression: var left2,
                Pattern: UnaryPatternSyntax
                {
                    RawKind: (int)SyntaxKind.NotPattern,
                    Pattern: TypePatternSyntax { Type: var type2 },
                },
            })
        {
            target = left2.ToString();
            type = type2.ToString();
            return true;
        }

        target = null;
        type = null;
        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_pattern_matching_over_is_with_cast_check") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitIfStatement(
        IfStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        // Case 1: Intra-statement — if (x is Type) { ...(Type)x... }
        if (node.Condition is BinaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.IsExpression,
                Left: var isLeft,
                Right: TypeSyntax,
            })
        {
            string isTarget = isLeft.ToString();

            bool hasCast = node.Statement.DescendantNodes()
                .OfType<CastExpressionSyntax>()
                .Any(c => c.Expression.ToString() == isTarget);

            if (hasCast)
            {
                ReportDiagnostic(node, diagnostics);
            }
        }

        // Case 2: Guard clause — if (!(x is Type)) return; followed by (Type)x
        if (node.Else is null
            && IsJumpStatement(node.Statement)
            && TryGetNegatedIsExpression(node.Condition, out string? guardTarget, out string? guardType)
            && node.Parent is BlockSyntax block)
        {
            int ifIndex = block.Statements.IndexOf(node);

            for (int i = ifIndex + 1; i < block.Statements.Count; i++)
            {
                bool hasCast = block.Statements[i].DescendantNodes()
                    .OfType<CastExpressionSyntax>()
                    .Any(c => c.Type.ToString() == guardType && c.Expression.ToString() == guardTarget);

                if (hasCast)
                {
                    ReportDiagnostic(node, diagnostics);
                    break;
                }
            }
        }
    }
}
