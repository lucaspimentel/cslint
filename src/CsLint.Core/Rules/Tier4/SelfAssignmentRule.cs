using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class SelfAssignmentRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "CSLINT304";

    public string Name => "SelfAssignment";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.CSLINT304.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.CSLINT304.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        foreach (AssignmentExpressionSyntax assignment in context.Root.DescendantNodes().OfType<AssignmentExpressionSyntax>())
        {
            if (!assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                continue;
            }

            ISymbol? leftSymbol = model.GetSymbolInfo(assignment.Left).Symbol;
            ISymbol? rightSymbol = model.GetSymbolInfo(assignment.Right).Symbol;

            if (leftSymbol is not null &&
                rightSymbol is not null &&
                SymbolEqualityComparer.Default.Equals(leftSymbol, rightSymbol))
            {
                LinePosition start = assignment.GetLocation().GetLineSpan().StartLinePosition;

                diagnostics.Add(new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"'{assignment.Left}' is assigned to itself",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = start.Line + 1,
                    Column = start.Character + 1,
                });
            }
        }
    }
}
