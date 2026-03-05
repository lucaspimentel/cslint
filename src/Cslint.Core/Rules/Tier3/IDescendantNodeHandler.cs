using Microsoft.CodeAnalysis;

namespace Cslint.Core.Rules.Tier3;

internal interface IDescendantNodeHandler
{
    void VisitNode(SyntaxNode node, List<LintDiagnostic> diagnostics);
}
