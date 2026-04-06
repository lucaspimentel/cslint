using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NewLineInAnonymousTypeRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_new_line_before_members_in_anonymous_types";

    public string RuleId => "CSLINT284";

    public string Name => "NewLineInAnonymousType";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
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
        if (node is not AnonymousObjectCreationExpressionSyntax anonType)
        {
            return;
        }

        if (anonType.Initializers.Count < 2)
        {
            return;
        }

        // Skip single-line anonymous types (e.g., new { A = 1, B = 2 })
        int openLine = anonType.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        int closeLine = anonType.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (openLine == closeLine)
        {
            return;
        }

        bool requireNewLine = config.GetBool(ConfigKey);

        for (int i = 1; i < anonType.Initializers.Count; i++)
        {
            int prevLine = anonType.Initializers[i - 1]
                .GetLocation().GetLineSpan().EndLinePosition.Line;

            FileLinePositionSpan currSpan = anonType.Initializers[i]
                .GetLocation().GetLineSpan();

            int currLine = currSpan.StartLinePosition.Line;
            bool sameLine = prevLine == currLine;

            if (requireNewLine && sameLine)
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Place each member in an anonymous type on a new line",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = currSpan.StartLinePosition.Line + 1,
                        Column = currSpan.StartLinePosition.Character + 1,
                    });
            }
            else if (!requireNewLine && !sameLine)
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Place anonymous type members on the same line",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = currSpan.StartLinePosition.Line + 1,
                        Column = currSpan.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
