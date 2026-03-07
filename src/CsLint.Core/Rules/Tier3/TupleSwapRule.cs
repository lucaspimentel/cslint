using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class TupleSwapRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT223";

    public string Name => "TupleSwap";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_tuple_swap"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_tuple_swap") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_tuple_swap");

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
        if (node is not BlockSyntax block)
        {
            return;
        }

        SyntaxList<StatementSyntax> statements = block.Statements;

        for (int i = 0; i + 2 < statements.Count; i++)
        {
            // Statement 1: var t = a;  (or explicit type)
            if (statements[i] is not LocalDeclarationStatementSyntax localDecl)
            {
                continue;
            }

            if (localDecl.Declaration.Variables.Count != 1)
            {
                continue;
            }

            VariableDeclaratorSyntax tempVar = localDecl.Declaration.Variables[0];

            if (tempVar.Initializer is null)
            {
                continue;
            }

            string tempName = tempVar.Identifier.Text;
            string aExpr = tempVar.Initializer.Value.ToString();

            // Statement 2: a = b;
            if (statements[i + 1] is not ExpressionStatementSyntax
                {
                    Expression: AssignmentExpressionSyntax
                    {
                        RawKind: (int)SyntaxKind.SimpleAssignmentExpression,
                    } assign1,
                })
            {
                continue;
            }

            if (assign1.Left.ToString() != aExpr)
            {
                continue;
            }

            string bExpr = assign1.Right.ToString();

            // Statement 3: b = t;
            if (statements[i + 2] is not ExpressionStatementSyntax
                {
                    Expression: AssignmentExpressionSyntax
                    {
                        RawKind: (int)SyntaxKind.SimpleAssignmentExpression,
                    } assign2,
                })
            {
                continue;
            }

            if (assign2.Left.ToString() != bExpr)
            {
                continue;
            }

            if (assign2.Right.ToString() != tempName)
            {
                continue;
            }

            // Verify temp var isn't used after the swap
            bool tempUsedLater = false;

            for (int j = i + 3; j < statements.Count; j++)
            {
                if (statements[j].ToString().Contains(tempName))
                {
                    tempUsedLater = true;
                    break;
                }
            }

            if (tempUsedLater)
            {
                continue;
            }

            FileLinePositionSpan span = localDecl.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Use tuple swap '({aExpr}, {bExpr}) = ({bExpr}, {aExpr})' instead of temp variable",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
