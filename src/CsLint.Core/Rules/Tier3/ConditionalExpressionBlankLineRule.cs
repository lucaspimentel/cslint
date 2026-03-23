using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConditionalExpressionBlankLineRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_allow_blank_line_after_token_in_conditional_expression";

    private const string StandardKey = ConfigKey + "_experimental";

    public string RuleId => "CSLINT232";

    public string Name => "ConditionalExpressionBlankLine";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey, StandardKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsActive(LintConfiguration config) =>
        string.Equals(config.GetFirstValue(ConfigKey, StandardKey), "false", StringComparison.OrdinalIgnoreCase);

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetFirstValue(ConfigKey, StandardKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsActive(context.Configuration))
        {
            return [];
        }

        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private void AddDiagnostic(SyntaxToken token, string message, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = message,
                Severity = LintSeverity.Warning,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    void IStyleRuleHandler.VisitConditionalExpression(
        ConditionalExpressionSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (!IsActive(config))
        {
            return;
        }

        if (NewLineHelper.HasBlankLineAfter(node.QuestionToken))
        {
            AddDiagnostic(node.QuestionToken, "No blank line allowed after '?' in conditional expression", diagnostics);
        }

        if (NewLineHelper.HasBlankLineAfter(node.ColonToken))
        {
            AddDiagnostic(node.ColonToken, "No blank line allowed after ':' in conditional expression", diagnostics);
        }
    }
}
