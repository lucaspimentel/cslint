using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0026: Use expression body for indexers.
/// </summary>
public sealed class ExpressionBodiedIndexersRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_style_expression_bodied_indexers";

    public string RuleId => "IDE0026";

    public string Name => "ExpressionBodiedIndexers";

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

    void IStyleRuleHandler.VisitIndexerDeclaration(
        IndexerDeclarationSyntax node,
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

        if (!preferExpression || node.ExpressionBody is not null || node.AccessorList?.Accessors.Count != 1)
        {
            return;
        }

        AccessorDeclarationSyntax accessor = node.AccessorList.Accessors[0];

        if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration) && accessor.Body is not null && IsSingleStatement(accessor.Body))
        {
            FileLinePositionSpan span = node.ThisKeyword.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "IDE0026",
                    Message = "Indexer can use expression body",
                    Severity = LintSeverity.Info,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
