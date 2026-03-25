using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ImplicitlyTypedLambdaRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "IDE0350";

    public string Name => "ImplicitlyTypedLambda";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_implicitly_typed_lambda_expression"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_implicitly_typed_lambda_expression") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_implicitly_typed_lambda_expression");

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
        if (node is not ParenthesizedLambdaExpressionSyntax lambda)
        {
            return;
        }

        // No parameters — nothing to flag
        if (lambda.ParameterList.Parameters.Count == 0)
        {
            return;
        }

        // Explicit return type requires explicit parameter types
        if (lambda.ReturnType is not null)
        {
            return;
        }

        // Check if all parameters have explicit types
        bool allExplicit = true;

        foreach (ParameterSyntax param in lambda.ParameterList.Parameters)
        {
            if (param.Type is null)
            {
                allExplicit = false;
                break;
            }
        }

        if (!allExplicit)
        {
            return;
        }

        // Skip if any parameter has params modifier
        foreach (ParameterSyntax param in lambda.ParameterList.Parameters)
        {
            if (param.Modifiers.Any(SyntaxKind.ParamsKeyword))
            {
                return;
            }
        }

        (string? pref, string? _) = config.GetValueWithSeverity("csharp_style_prefer_implicitly_typed_lambda_expression");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        FileLinePositionSpan span = lambda.ParameterList.OpenParenToken.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Lambda parameter types can be inferred",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
