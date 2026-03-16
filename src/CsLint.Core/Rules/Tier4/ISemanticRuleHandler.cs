using Microsoft.CodeAnalysis;

namespace Cslint.Core.Rules.Tier4;

internal interface ISemanticRuleHandler
{
    void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics);
}
