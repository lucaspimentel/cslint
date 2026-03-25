using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0025: Use expression body for properties.
/// </summary>
public sealed class ExpressionBodiedPropertiesRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_expression_bodied_properties";

    public string RuleId => "IDE0025";

    public string Name => "ExpressionBodiedProperties";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsSingleStatement(BlockSyntax block) =>
        block.Statements.Count == 1 &&
        block.Statements[0] is ReturnStatementSyntax or ExpressionStatementSyntax;

    private static void AddDiagnostic(SyntaxToken token, string message, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "IDE0025",
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

    void IStyleRuleHandler.VisitPropertyDeclaration(
        PropertyDeclarationSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (node.ExpressionBody is null && node.AccessorList?.Accessors.Count == 1)
        {
            AccessorDeclarationSyntax accessor = node.AccessorList.Accessors[0];

            if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration) && accessor.Body is not null && IsSingleStatement(accessor.Body))
            {
                if (string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(pref, "when_on_single_line", StringComparison.OrdinalIgnoreCase))
                {
                    AddDiagnostic(node.Identifier, "Property can use expression body", diagnostics);
                }
            }
        }
    }
}
