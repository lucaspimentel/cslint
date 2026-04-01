using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class CatchGeneralExceptionRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1031.severity";

    public string RuleId => "CA1031";

    public string Name => "DoNotCatchGeneralExceptionTypes";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    // Exception types that are too general to catch
    private static readonly HashSet<string> GeneralExceptionTypes = new(
        StringComparer.Ordinal)
    {
        "Exception",
        "SystemException",
        "System.Exception",
        "System.SystemException",
    };

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (CatchClauseSyntax catchClause in
            context.Root.DescendantNodes().OfType<CatchClauseSyntax>())
        {
            // Bare `catch { }` without a type — catches everything
            if (catchClause.Declaration is null)
            {
                FileLinePositionSpan bareSpan =
                    catchClause.CatchKeyword.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Do not catch general exception types",
                        Severity = DefaultSeverity,
                        FilePath = context.FilePath,
                        Line = bareSpan.StartLinePosition.Line + 1,
                        Column = bareSpan.StartLinePosition.Character + 1,
                    });

                continue;
            }

            string typeName = catchClause.Declaration.Type.ToString();

            if (!GeneralExceptionTypes.Contains(typeName))
            {
                continue;
            }

            FileLinePositionSpan span =
                catchClause.Declaration.Type.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Do not catch general exception type "
                        + $"'{typeName}'",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
