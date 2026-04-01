using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class DuplicateIndexedElementInitRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2244.severity";

    public string RuleId => "CA2244";

    public string Name => "DoNotDuplicateIndexedElementInitializations";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (InitializerExpressionSyntax initializer in
            context.Root.DescendantNodes()
                .OfType<InitializerExpressionSyntax>())
        {
            var seenIndices = new HashSet<string>(StringComparer.Ordinal);

            foreach (ExpressionSyntax expr in initializer.Expressions)
            {
                // Match: { [key] = value } pattern
                if (expr is not AssignmentExpressionSyntax assignment)
                {
                    continue;
                }

                if (assignment.Left is not
                    ImplicitElementAccessSyntax elementAccess)
                {
                    continue;
                }

                string indexText = elementAccess.ArgumentList.ToString();

                if (!seenIndices.Add(indexText))
                {
                    FileLinePositionSpan span =
                        assignment.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Duplicate indexed element "
                                + $"initialization {indexText}",
                            Severity = DefaultSeverity,
                            FilePath = context.FilePath,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
