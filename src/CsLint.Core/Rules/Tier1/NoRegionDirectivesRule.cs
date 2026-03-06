using Cslint.Core.Config;

namespace Cslint.Core.Rules.Tier1;

public sealed class NoRegionDirectivesRule : IRuleDefinition
{
    public string RuleId => "CSLINT006";

    public string Name => "NoRegionDirectives";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_no_region_directives"];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetBool("csharp_no_region_directives");

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var diagnostics = new List<LintDiagnostic>();
        int lineNumber = 0;

        foreach (ReadOnlySpan<char> line in context.SourceString.AsSpan().EnumerateLines())
        {
            lineNumber++;

            ReadOnlySpan<char> trimmed = line.TrimStart();

            if (trimmed.StartsWith("#region") || trimmed.StartsWith("#endregion"))
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Region directives are not allowed",
                        Severity = LintSeverity.Warning,
                        FilePath = context.FilePath,
                        Line = lineNumber,
                        Column = line.Length - trimmed.Length + 1,
                    });
            }
        }

        return diagnostics;
    }
}
