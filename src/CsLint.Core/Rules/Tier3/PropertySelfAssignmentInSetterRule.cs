using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PropertySelfAssignmentInSetterRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2011.severity";

    public string RuleId => "CA2011";

    public string Name => "DoNotAssignPropertyWithinItsSetter";

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

        foreach (PropertyDeclarationSyntax property in context.Root.DescendantNodes().OfType<PropertyDeclarationSyntax>())
        {
            if (property.AccessorList is null)
            {
                continue;
            }

            // Skip explicit interface implementations — the setter
            // delegates to the class's own property, not self-assignment
            if (property.ExplicitInterfaceSpecifier is not null)
            {
                continue;
            }

            foreach (AccessorDeclarationSyntax accessor in property.AccessorList.Accessors)
            {
                if (!accessor.IsKind(SyntaxKind.SetAccessorDeclaration) &&
                    !accessor.IsKind(SyntaxKind.InitAccessorDeclaration))
                {
                    continue;
                }

                if (accessor.Body is null && accessor.ExpressionBody is null)
                {
                    continue;
                }

                string propertyName = property.Identifier.Text;

                // Check all assignment expressions within the setter
                IEnumerable<SyntaxNode> nodes = accessor.Body is not null
                    ? accessor.Body.DescendantNodes()
                    : accessor.ExpressionBody!.DescendantNodesAndSelf();

                foreach (AssignmentExpressionSyntax assignment in nodes.OfType<AssignmentExpressionSyntax>())
                {
                    if (assignment.Left is IdentifierNameSyntax lhs &&
                        lhs.Identifier.Text == propertyName)
                    {
                        FileLinePositionSpan span = assignment.GetLocation().GetLineSpan();

                        (diagnostics ??= []).Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message = $"Do not assign property '{propertyName}' within its own setter (causes infinite recursion)",
                                Severity = DefaultSeverity,
                                FilePath = context.FilePath,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });
                    }
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
