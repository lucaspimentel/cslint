using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class RangeOperatorRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT227";

    public string Name => "RangeOperator";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_range_operator"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_range_operator") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_range_operator");

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
        // Match str.Substring(n) — single-arg only
        if (node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax
            {
                Name.Identifier.Text: "Substring",
            })
        {
            return;
        }

        // Only flag single-argument Substring (skip two-arg form)
        if (invocation.ArgumentList.Arguments.Count != 1)
        {
            return;
        }

        FileLinePositionSpan span = invocation.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use range operator ('str[n..]') instead of 'Substring(n)'",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
