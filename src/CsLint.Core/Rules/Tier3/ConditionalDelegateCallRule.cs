using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConditionalDelegateCallRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_conditional_delegate_call";

    public string RuleId => "CSLINT271";

    public string Name => "ConditionalDelegateCall";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static string? GetNullCheckedIdentifier(ExpressionSyntax condition)
    {
        if (condition is not BinaryExpressionSyntax binary
            || !binary.IsKind(SyntaxKind.NotEqualsExpression))
        {
            return null;
        }

        if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression)
            && binary.Left is IdentifierNameSyntax leftId)
        {
            return leftId.Identifier.Text;
        }

        if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression)
            && binary.Right is IdentifierNameSyntax rightId)
        {
            return rightId.Identifier.Text;
        }

        return null;
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

        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitIfStatement(
        IfStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (node.Else is not null)
        {
            return;
        }

        string? identifierName = GetNullCheckedIdentifier(node.Condition);

        if (identifierName is null)
        {
            return;
        }

        StatementSyntax body = node.Statement;

        if (body is BlockSyntax { Statements.Count: 1 } block)
        {
            body = block.Statements[0];
        }

        if (body is not ExpressionStatementSyntax { Expression: InvocationExpressionSyntax invocation })
        {
            return;
        }

        if (invocation.Expression is not IdentifierNameSyntax invokedName
            || invokedName.Identifier.Text != identifierName)
        {
            return;
        }

        FileLinePositionSpan span = node.IfKeyword.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message =
                    $"Use conditional delegate call '{identifierName}?.Invoke(...)' instead of null check",
                Severity = LintSeverity.Warning,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
