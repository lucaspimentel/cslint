using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PatternMatchingAsRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_pattern_matching_over_as_with_null_check";

    public string RuleId => "CSLINT270";

    public string Name => "PatternMatchingAs";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsNullCheckOnVariable(
        ExpressionSyntax condition,
        string variableName)
    {
        if (condition is BinaryExpressionSyntax binary
            && binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression)
                && binary.Left is IdentifierNameSyntax leftId
                && leftId.Identifier.Text == variableName)
            {
                return true;
            }

            if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression)
                && binary.Right is IdentifierNameSyntax rightId
                && rightId.Identifier.Text == variableName)
            {
                return true;
            }
        }

        if (condition is IsPatternExpressionSyntax
            {
                Expression: IdentifierNameSyntax patternId,
                Pattern: UnaryPatternSyntax
                {
                    RawKind: (int)SyntaxKind.NotPattern,
                    Pattern: ConstantPatternSyntax
                    {
                        Expression.RawKind: (int)SyntaxKind.NullLiteralExpression,
                    },
                },
            }
            && patternId.Identifier.Text == variableName)
        {
            return true;
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

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
        if (node is not LocalDeclarationStatementSyntax localDecl)
        {
            return;
        }

        if (localDecl.Declaration.Variables.Count != 1)
        {
            return;
        }

        VariableDeclaratorSyntax variable = localDecl.Declaration.Variables[0];

        if (variable.Initializer?.Value is not BinaryExpressionSyntax
            {
                RawKind: (int)SyntaxKind.AsExpression,
            })
        {
            return;
        }

        string variableName = variable.Identifier.Text;

        if (localDecl.Parent is not BlockSyntax block)
        {
            return;
        }

        int index = block.Statements.IndexOf(localDecl);

        if (index < 0 || index >= block.Statements.Count - 1)
        {
            return;
        }

        if (block.Statements[index + 1] is not IfStatementSyntax ifStatement)
        {
            return;
        }

        if (!IsNullCheckOnVariable(ifStatement.Condition, variableName))
        {
            return;
        }

        FileLinePositionSpan span = localDecl.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message =
                    $"Use pattern matching ('is Type {variableName}') instead of 'as' with null check",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
