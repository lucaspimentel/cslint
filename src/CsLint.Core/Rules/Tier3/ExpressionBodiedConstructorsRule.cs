using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0021: Use expression body for constructors.
/// </summary>
public sealed class ExpressionBodiedConstructorsRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_expression_bodied_constructors";

    public string RuleId => "IDE0021";

    public string Name => "ExpressionBodiedConstructors";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsSingleStatement(BlockSyntax block) =>
        block.Statements.Count == 1 &&
        block.Statements[0] is ReturnStatementSyntax or ExpressionStatementSyntax;

    private static void AddDiagnostic(SyntaxToken token, string ruleId, string message, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = ruleId,
                Message = message,
                Severity = LintSeverity.Info,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitConstructorDeclaration(
        ConstructorDeclarationSyntax node,
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
            AddDiagnostic(node.Identifier, "IDE0021", "Constructor can use expression body", diagnostics);
        }
        else if (!preferExpression && node.ExpressionBody is not null)
        {
            AddDiagnostic(node.Identifier, "IDE0022", "Constructor can use block body", diagnostics);
        }
    }
}
