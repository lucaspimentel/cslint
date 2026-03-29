using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class EmptyFinalizerRule : IRuleDefinition, IStyleRuleHandler
{
    private const string CustomKey = "csharp_no_empty_finalizers";

    private const string DiagnosticKey = "dotnet_diagnostic.CA1821.severity";

    public string RuleId => "CA1821";

    public string Name => "EmptyFinalizer";

    public IReadOnlyList<string> ConfigKeys { get; } = [CustomKey, DiagnosticKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        CaRuleHelper.IsEnabledByDefault(configuration, DiagnosticKey, CustomKey);

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitDestructorDeclaration(
        DestructorDeclarationSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (!CaRuleHelper.IsEnabledByDefault(config, DiagnosticKey, CustomKey))
        {
            return;
        }

        // Expression body (~Foo() => expr) is non-empty by definition
        if (node.ExpressionBody is not null)
        {
            return;
        }

        // No body at all (partial destructor, error recovery) — skip
        if (node.Body is null)
        {
            return;
        }

        if (node.Body.Statements.Count > 0)
        {
            return;
        }

        FileLinePositionSpan span = node.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = "Remove empty finalizer",
                Severity = LintSeverity.Warning,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
