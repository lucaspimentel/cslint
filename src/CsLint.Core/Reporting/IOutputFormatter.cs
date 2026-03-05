using Cslint.Core.Rules;

namespace Cslint.Core.Reporting;

public interface IOutputFormatter
{
    public string Format(IReadOnlyList<LintDiagnostic> diagnostics);
}
