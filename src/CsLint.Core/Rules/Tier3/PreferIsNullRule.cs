using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0041: Use 'is null' instead of 'ReferenceEquals(x, null)'.
/// </summary>
public sealed class PreferIsNullRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "dotnet_style_prefer_is_null_check_over_reference_equality_method";

    public string RuleId => "IDE0041";

    public string Name => "PreferIsNull";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

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
        if (node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (!IsReferenceEqualsCall(invocation))
        {
            return;
        }

        ArgumentListSyntax args = invocation.ArgumentList;

        if (args.Arguments.Count != 2)
        {
            return;
        }

        bool firstIsNull = args.Arguments[0].Expression.IsKind(SyntaxKind.NullLiteralExpression);
        bool secondIsNull = args.Arguments[1].Expression.IsKind(SyntaxKind.NullLiteralExpression);

        if (!firstIsNull && !secondIsNull)
        {
            return;
        }

        FileLinePositionSpan span = invocation.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Use 'is null' instead of 'ReferenceEquals'",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private static bool IsReferenceEqualsCall(InvocationExpressionSyntax invocation)
    {
        switch (invocation.Expression)
        {
            // ReferenceEquals(x, null)
            case IdentifierNameSyntax identifier
                when identifier.Identifier.Text == "ReferenceEquals":
                return true;

            // object.ReferenceEquals(x, null) — 'object' is a keyword (PredefinedTypeSyntax)
            case MemberAccessExpressionSyntax memberAccess
                when memberAccess.Name.Identifier.Text == "ReferenceEquals" &&
                     memberAccess.Expression is PredefinedTypeSyntax predefined &&
                     predefined.Keyword.IsKind(SyntaxKind.ObjectKeyword):
                return true;

            // System.Object.ReferenceEquals(x, null)
            case MemberAccessExpressionSyntax memberAccess2
                when memberAccess2.Name.Identifier.Text == "ReferenceEquals" &&
                     memberAccess2.Expression is MemberAccessExpressionSyntax innerAccess &&
                     innerAccess.Name.Identifier.Text == "Object" &&
                     innerAccess.Expression is IdentifierNameSyntax ns &&
                     ns.Identifier.Text == "System":
                return true;

            // Object.ReferenceEquals(x, null) — capitalized
            case MemberAccessExpressionSyntax memberAccess3
                when memberAccess3.Name.Identifier.Text == "ReferenceEquals" &&
                     memberAccess3.Expression is IdentifierNameSyntax qualifier2 &&
                     qualifier2.Identifier.Text == "Object":
                return true;

            default:
                return false;
        }
    }
}
