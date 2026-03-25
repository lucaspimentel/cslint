using System.Collections.Immutable;
using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class UnusedLocalVariableRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "CSLINT301";

    public string Name => "UnusedLocalVariable";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.CSLINT301.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.CSLINT301.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        ImmutableArray<Diagnostic> allDiagnostics = model.GetDiagnostics();

        // If the compilation is missing references, Roslyn may report false CS0219 diagnostics
        // when types are unresolved. CS0246/CS0234 indicate unresolved types/namespaces,
        // so skip unused-local detection when present.
        bool hasMissingReferences = allDiagnostics.Any(d => d.Id is "CS0246" or "CS0234");

        if (hasMissingReferences)
        {
            return;
        }

        foreach (Diagnostic diagnostic in allDiagnostics)
        {
            if (diagnostic.Id != "CS0219")
            {
                continue;
            }

            LinePosition start = diagnostic.Location.GetLineSpan().StartLinePosition;

            diagnostics.Add(new LintDiagnostic
            {
                RuleId = RuleId,
                Message = diagnostic.GetMessage(),
                Severity = DefaultSeverity,
                FilePath = context.FilePath,
                Line = start.Line + 1,
                Column = start.Character + 1,
            });
        }
    }
}
