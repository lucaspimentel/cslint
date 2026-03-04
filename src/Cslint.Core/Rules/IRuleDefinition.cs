using Cslint.Core.Config;

namespace Cslint.Core.Rules;

public interface IRuleDefinition
{
    public string RuleId { get; }

    public string Name { get; }

    public IReadOnlyList<string> ConfigKeys { get; }

    public bool IsEnabled(LintConfiguration configuration);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context);
}
