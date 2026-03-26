using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class StackallocInLoopRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2014.severity";

    public string RuleId => "CA2014";

    public string Name => "DoNotUseStackallocInLoops";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsInsideLoop(SyntaxNode node)
    {
        SyntaxNode? current = node.Parent;

        while (current is not null)
        {
            if (current is ForStatementSyntax or
                ForEachStatementSyntax or
                WhileStatementSyntax or
                DoStatementSyntax)
            {
                return true;
            }

            // Stop at method/lambda boundaries
            if (current is BaseMethodDeclarationSyntax or
                LambdaExpressionSyntax or
                AnonymousFunctionExpressionSyntax)
            {
                return false;
            }

            current = current.Parent;
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            if (node is not (StackAllocArrayCreationExpressionSyntax or ImplicitStackAllocArrayCreationExpressionSyntax))
            {
                continue;
            }

            if (!IsInsideLoop(node))
            {
                continue;
            }

            FileLinePositionSpan span = node.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Do not use stackalloc in loops",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
