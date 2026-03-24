using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class InferredTupleNamesRule : IRuleDefinition
{
    public string RuleId => "IDE0037";

    public string Name => "InferredTupleNames";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_style_prefer_inferred_tuple_names"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static string? GetInferredName(ExpressionSyntax expression) =>
        expression switch
        {
            // (name: name) — inferred from simple identifier
            IdentifierNameSyntax identifier => identifier.Identifier.Text,

            // (Name: person.Name) — inferred from last member in member access
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.Text,

            // (Name: person?.Name) — inferred from conditional access
            ConditionalAccessExpressionSyntax conditionalAccess
                when conditionalAccess.WhenNotNull is MemberBindingExpressionSyntax memberBinding
                => memberBinding.Name.Identifier.Text,

            _ => null,
        };

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_prefer_inferred_tuple_names") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("dotnet_style_prefer_inferred_tuple_names");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            if (node is not ArgumentSyntax argument)
            {
                continue;
            }

            // Must be inside a tuple expression
            if (argument.Parent is not TupleExpressionSyntax)
            {
                continue;
            }

            // Must have an explicit name
            if (argument.NameColon is null)
            {
                continue;
            }

            string explicitName = argument.NameColon.Name.Identifier.Text;
            string? inferredName = GetInferredName(argument.Expression);

            if (inferredName is null)
            {
                continue;
            }

            // Case-sensitive match: the explicit name must match what would be inferred
            if (explicitName != inferredName)
            {
                continue;
            }

            FileLinePositionSpan span = argument.NameColon.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Tuple element name '{explicitName}' can be inferred",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics;
    }
}
