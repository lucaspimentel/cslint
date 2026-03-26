using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0024: Use expression body for operators.
/// </summary>
public sealed class ExpressionBodiedOperatorsRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_expression_bodied_operators";

    public string RuleId => "IDE0024";

    public string Name => "ExpressionBodiedOperators";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsSingleStatement(BlockSyntax block) =>
        block.Statements.Count == 1 &&
        block.Statements[0] is ReturnStatementSyntax or ExpressionStatementSyntax;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitOperatorDeclaration(
        OperatorDeclarationSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (pref is null)
        {
            return;
        }

        bool preferExpression = string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(pref, "when_on_single_line", StringComparison.OrdinalIgnoreCase);

        if (preferExpression && node.ExpressionBody is null && node.Body is not null && IsSingleStatement(node.Body))
        {
            FileLinePositionSpan span = node.OperatorToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "IDE0024",
                    Message = "Operator can use expression body",
                    Severity = LintSeverity.Info,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
