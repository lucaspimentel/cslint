using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class SimplifyInterpolationRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT225";

    public string Name => "SimplifyInterpolation";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_prefer_simplified_interpolation"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_prefer_simplified_interpolation") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("dotnet_style_prefer_simplified_interpolation");

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
        if (node is not InterpolationSyntax interpolation)
        {
            return;
        }

        // Look for {x.ToString()} — invocation with no arguments on an identifier
        if (interpolation.Expression is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax memberAccess,
                ArgumentList.Arguments.Count: 0,
            })
        {
            return;
        }

        if (memberAccess.Name.Identifier.Text != "ToString")
        {
            return;
        }

        // No format clause (already simplified would just be {x})
        if (interpolation.FormatClause is not null)
        {
            return;
        }

        FileLinePositionSpan span = interpolation.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Simplify interpolation: remove redundant '.ToString()' call",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
