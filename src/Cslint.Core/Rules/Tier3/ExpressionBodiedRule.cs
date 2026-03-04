using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ExpressionBodiedRule : IRuleDefinition
{
    public string RuleId => "CSLINT201";

    public string Name => "ExpressionBodied";

    public IReadOnlyList<string> ConfigKeys { get; } =
    [
        "csharp_style_expression_bodied_methods",
        "csharp_style_expression_bodied_properties",
        "csharp_style_expression_bodied_accessors",
    ];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_expression_bodied_methods") is not null ||
        configuration.GetValue("csharp_style_expression_bodied_properties") is not null ||
        configuration.GetValue("csharp_style_expression_bodied_accessors") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new ExpressionBodiedWalker(context.FilePath, context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class ExpressionBodiedWalker(string filePath, LintConfiguration config) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_expression_bodied_methods");
            CheckExpressionBody(node.ExpressionBody, node.Body, node.Identifier, pref, "method");
            base.VisitMethodDeclaration(node);
        }

        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_expression_bodied_properties");

            if (node.ExpressionBody is null && node.AccessorList?.Accessors.Count == 1)
            {
                AccessorDeclarationSyntax accessor = node.AccessorList.Accessors[0];

                if (accessor.IsKind(SyntaxKind.GetAccessorDeclaration) && accessor.Body is not null && IsSingleStatement(accessor.Body))
                {
                    if (string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(pref, "when_on_single_line", StringComparison.OrdinalIgnoreCase))
                    {
                        AddDiagnostic(node.Identifier, "Property can use expression body");
                    }
                }
            }

            base.VisitPropertyDeclaration(node);
        }

        private void CheckExpressionBody(
            ArrowExpressionClauseSyntax? expressionBody,
            BlockSyntax? body,
            SyntaxToken identifier,
            string? preference,
            string kind)
        {
            if (preference is null)
            {
                return;
            }

            bool preferExpression = string.Equals(preference, "true", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(preference, "when_on_single_line", StringComparison.OrdinalIgnoreCase);

            if (preferExpression && expressionBody is null && body is not null && IsSingleStatement(body))
            {
                AddDiagnostic(identifier, $"{kind} can use expression body");
            }
        }

        private static bool IsSingleStatement(BlockSyntax block) =>
            block.Statements.Count == 1 &&
            block.Statements[0] is ReturnStatementSyntax or ExpressionStatementSyntax;

        private void AddDiagnostic(SyntaxToken token, string message)
        {
            FileLinePositionSpan span = token.GetLocation().GetLineSpan();

            Diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT201",
                    Message = message,
                    Severity = LintSeverity.Info,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
