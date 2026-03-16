using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class UnusedUsingRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "CSLINT300";

    public string Name => "UnusedUsingDirective";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.CSLINT300.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetSeverityForKey("dotnet_diagnostic.CSLINT300.severity") != LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        foreach (Diagnostic diagnostic in model.GetDiagnostics())
        {
            if (diagnostic.Id != "CS8019")
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
