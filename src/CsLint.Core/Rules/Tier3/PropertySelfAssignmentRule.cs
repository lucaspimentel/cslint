using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PropertySelfAssignmentRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2245.severity";

    public string RuleId => "CA2245";

    public string Name => "DoNotAssignPropertyToItself";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (AssignmentExpressionSyntax assignment in
            context.Root.DescendantNodes()
                .OfType<AssignmentExpressionSyntax>())
        {
            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                continue;
            }

            // Skip assignments inside object initializers —
            // new Foo { X = X } assigns from this.X to target.X
            if (assignment.Parent is InitializerExpressionSyntax)
            {
                continue;
            }

            // Match: X = X or this.X = this.X or obj.X = obj.X
            string leftText = assignment.Left.ToString();
            string rightText = assignment.Right.ToString();

            if (!string.Equals(
                leftText, rightText, StringComparison.Ordinal))
            {
                continue;
            }

            // Must be a property access (member access or simple name)
            if (assignment.Left is not (
                MemberAccessExpressionSyntax or IdentifierNameSyntax))
            {
                continue;
            }

            // Skip if inside a property setter (CA2011 handles that)
            if (IsInsidePropertySetter(assignment, leftText))
            {
                continue;
            }

            FileLinePositionSpan span =
                assignment.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Property '{leftText}' is assigned to itself",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private static bool IsInsidePropertySetter(
        SyntaxNode node, string propertyName)
    {
        SyntaxNode? current = node.Parent;

        while (current is not null)
        {
            if (current is AccessorDeclarationSyntax accessor &&
                (accessor.IsKind(SyntaxKind.SetAccessorDeclaration) ||
                    accessor.IsKind(SyntaxKind.InitAccessorDeclaration)) &&
                accessor.Parent?.Parent is PropertyDeclarationSyntax prop &&
                prop.Identifier.Text == propertyName)
            {
                return true;
            }

            current = current.Parent;
        }

        return false;
    }
}
