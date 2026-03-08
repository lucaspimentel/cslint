using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConstructorInitializerBlankLineRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_allow_blank_line_after_colon_in_constructor_initializer";

    public string RuleId => "CSLINT231";

    public string Name => "ConstructorInitializerBlankLine";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!string.Equals(context.Configuration.GetValue(ConfigKey), "false", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (node is not ConstructorInitializerSyntax initializer)
        {
            return;
        }

        if (NewLineHelper.HasBlankLineAfter(initializer.ColonToken))
        {
            FileLinePositionSpan span = initializer.ColonToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "No blank line allowed after colon in constructor initializer",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
