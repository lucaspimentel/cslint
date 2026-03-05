namespace Cslint.Core.Rules;

public sealed class LintDiagnostic
{
    public required string RuleId { get; init; }

    public required string Message { get; init; }

    public required LintSeverity Severity { get; init; }

    public required string FilePath { get; init; }

    public required int Line { get; init; }

    public required int Column { get; init; }
}
