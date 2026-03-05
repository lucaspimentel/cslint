using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ExpressionBodiedRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT201";

    public string Name => "ExpressionBodied";

    public IReadOnlyList<string> ConfigKeys { get; } =
    [
        "csharp_style_expression_bodied_methods",
        "csharp_style_expression_bodied_properties",
        "csharp_style_expression_bodied_accessors",
    ];

    private LintConfiguration? _config;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_expression_bodied_methods") is not null ||
        configuration.GetValue("csharp_style_expression_bodied_properties") is not null ||
        configuration.GetValue("csharp_style_expression_bodied_accessors") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        _config = context.Configuration;
        var walker = new CombinedStyleWalker([this]);
        walker.Visit(context.Root);
        _config = null;
        return walker.Diagnostics;
    }

    internal void Initialize(LintConfiguration config) => _config = config;

    internal void Reset() => _config = null;

    void IStyleRuleHandler.VisitMethodDeclaration(MethodDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_config is null)
        {
            return;
        }

        (string? pref, string? _) = _config.GetValueWithSeverity("csharp_style_expression_bodied_methods");
        CheckExpressionBody(node.ExpressionBody, node.Body, node.Identifier, pref, "method", diagnostics);
    }

    void IStyleRuleHandler.VisitPropertyDeclaration(PropertyDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        if (_config is null)
        {
            return;
        }

        (string? pref, string? _) = _config.GetValueWithSeverity("csharp_style_expression_bodied_properties");

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

    private static void CheckExpressionBody(
        ArrowExpressionClauseSyntax? expressionBody,
        BlockSyntax? body,
        SyntaxToken identifier,
        string? preference,
        string kind,
        List<LintDiagnostic> diagnostics)
    {
        if (preference is null)
        {
            return;
        }

        bool preferExpression = string.Equals(preference, "true", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(preference, "when_on_single_line", StringComparison.OrdinalIgnoreCase);

        if (preferExpression && expressionBody is null && body is not null && IsSingleStatement(body))
        {
            AddDiagnostic(identifier, $"{kind} can use expression body", diagnostics);
        }
    }

    private static bool IsSingleStatement(BlockSyntax block) =>
        block.Statements.Count == 1 &&
        block.Statements[0] is ReturnStatementSyntax or ExpressionStatementSyntax;

    private static void AddDiagnostic(SyntaxToken token, string message, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "CSLINT201",
                Message = message,
                Severity = LintSeverity.Info,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
