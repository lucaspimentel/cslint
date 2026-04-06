using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NewLineInObjectInitializerRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_new_line_before_members_in_object_initializers";

    public string RuleId => "CSLINT283";

    public string Name => "NewLineInObjectInitializer";

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
        if (node is not InitializerExpressionSyntax initializer)
        {
            return;
        }

        if (!initializer.IsKind(SyntaxKind.ObjectInitializerExpression)
            && !initializer.IsKind(SyntaxKind.CollectionInitializerExpression)
            && !initializer.IsKind(SyntaxKind.ArrayInitializerExpression))
        {
            return;
        }

        if (initializer.Expressions.Count < 2)
        {
            return;
        }

        // Skip single-line initializers (e.g., new Foo { A = 1, B = 2 })
        int openLine = initializer.OpenBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;
        int closeLine = initializer.CloseBraceToken.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (openLine == closeLine)
        {
            return;
        }

        bool requireNewLine = config.GetBool(ConfigKey);

        for (int i = 1; i < initializer.Expressions.Count; i++)
        {
            int prevLine = initializer.Expressions[i - 1]
                .GetLocation().GetLineSpan().EndLinePosition.Line;

            FileLinePositionSpan currSpan = initializer.Expressions[i]
                .GetLocation().GetLineSpan();

            int currLine = currSpan.StartLinePosition.Line;
            bool sameLine = prevLine == currLine;

            if (requireNewLine && sameLine)
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Place each member in an initializer on a new line",
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
                        Message = "Place initializer members on the same line",
                        Severity = LintSeverity.Warning,
                        FilePath = filePath,
                        Line = currSpan.StartLinePosition.Line + 1,
                        Column = currSpan.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
