using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NullCheckingRule : IRuleDefinition
{
    public string RuleId => "CSLINT210";

    public string Name => "NullChecking";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_null_checking"];

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new NullCheckWalker(context.FilePath);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class NullCheckWalker(string filePath) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            // Detect: x != null ? x : default  =>  suggest x ?? default
            if (IsNullCheck(node.Condition, out _))
            {
                FileLinePositionSpan span = node.QuestionToken.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT210",
                        Message = "Use null coalescing operator (??) instead of null check with conditional expression",
                        Severity = LintSeverity.Info,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }

            base.VisitConditionalExpression(node);
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            // Detect: if (x == null) throw new ArgumentNullException => suggest ?? throw
            if (IsNullEqualityCheck(node.Condition) &&
                node.Statement is BlockSyntax { Statements.Count: 1 } block &&
                block.Statements[0] is ThrowStatementSyntax)
            {
                FileLinePositionSpan span = node.IfKeyword.GetLocation().GetLineSpan();

                Diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT210",
                        Message = "Use null coalescing throw expression (?? throw) instead of null check with if statement",
                        Severity = LintSeverity.Info,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }

            base.VisitIfStatement(node);
        }

        private static bool IsNullCheck(ExpressionSyntax expression, out ExpressionSyntax? checkedExpression)
        {
            checkedExpression = null;

            if (expression is BinaryExpressionSyntax binary)
            {
                if (binary.IsKind(SyntaxKind.NotEqualsExpression))
                {
                    if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
                    {
                        checkedExpression = binary.Left;
                        return true;
                    }

                    if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression))
                    {
                        checkedExpression = binary.Right;
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsNullEqualityCheck(ExpressionSyntax expression)
        {
            if (expression is BinaryExpressionSyntax binary && binary.IsKind(SyntaxKind.EqualsExpression))
            {
                return binary.Right.IsKind(SyntaxKind.NullLiteralExpression) ||
                       binary.Left.IsKind(SyntaxKind.NullLiteralExpression);
            }

            return false;
        }
    }
}
