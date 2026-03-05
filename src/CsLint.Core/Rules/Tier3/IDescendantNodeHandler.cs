using Cslint.Core.Config;
using Microsoft.CodeAnalysis;

namespace Cslint.Core.Rules.Tier3;

internal interface IDescendantNodeHandler
{
    void VisitNode(SyntaxNode node, LintConfiguration config, string filePath, List<LintDiagnostic> diagnostics);
}
