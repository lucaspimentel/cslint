using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class InferredMemberNameRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT234";

    public string Name => "InferredMemberName";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_prefer_inferred_anonymous_type_member_names"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_prefer_inferred_anonymous_type_member_names") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("dotnet_style_prefer_inferred_anonymous_type_member_names");

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
        (string? pref, string? _) = config.GetValueWithSeverity("dotnet_style_prefer_inferred_anonymous_type_member_names");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        switch (node)
        {
            case AnonymousObjectMemberDeclaratorSyntax memberDecl:
                CheckAnonymousTypeMember(memberDecl, filePath, diagnostics);
                break;

            case ArgumentSyntax argument:
                CheckTupleArgument(argument, filePath, diagnostics);
                break;
        }
    }

    private void CheckAnonymousTypeMember(
        AnonymousObjectMemberDeclaratorSyntax memberDecl,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        // Must have explicit name assignment (e.g., `x = x`)
        if (memberDecl.NameEquals is null)
        {
            return;
        }

        // Expression must be a simple identifier (not member access, invocation, etc.)
        if (memberDecl.Expression is not IdentifierNameSyntax identifier)
        {
            return;
        }

        // Name must match expression exactly (case-sensitive)
        if (memberDecl.NameEquals.Name.Identifier.Text != identifier.Identifier.Text)
        {
            return;
        }

        FileLinePositionSpan span = memberDecl.NameEquals.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Member name '{identifier.Identifier.Text}' can be inferred",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    private void CheckTupleArgument(
        ArgumentSyntax argument,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        // Must be inside a tuple expression
        if (argument.Parent is not TupleExpressionSyntax)
        {
            return;
        }

        // Must have explicit name (e.g., `name: name`)
        if (argument.NameColon is null)
        {
            return;
        }

        // Expression must be a simple identifier
        if (argument.Expression is not IdentifierNameSyntax identifier)
        {
            return;
        }

        // Name must match expression exactly (case-sensitive)
        if (argument.NameColon.Name.Identifier.Text != identifier.Identifier.Text)
        {
            return;
        }

        FileLinePositionSpan span = argument.NameColon.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Tuple element name '{identifier.Identifier.Text}' can be inferred",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
