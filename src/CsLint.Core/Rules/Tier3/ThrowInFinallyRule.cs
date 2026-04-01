using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ThrowInFinallyRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2219.severity";

    public string RuleId => "CA2219";

    public string Name => "DoNotRaiseExceptionsInExceptionClauses";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsDirectlyInsideFinally(SyntaxNode node)
    {
        SyntaxNode? current = node.Parent;

        while (current is not null)
        {
            // If we hit a finally clause, this throw is inside it
            if (current is FinallyClauseSyntax)
            {
                return true;
            }

            // If we hit a nested try statement first, the throw is contained
            // within that inner try — not directly in the finally
            if (current is TryStatementSyntax)
            {
                return false;
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
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            SyntaxToken? keyword = node switch
            {
                ThrowStatementSyntax throwStmt => throwStmt.ThrowKeyword,
                ThrowExpressionSyntax throwExpr => throwExpr.ThrowKeyword,
                _ => null,
            };

            if (keyword is null || !IsDirectlyInsideFinally(node))
            {
                continue;
            }

            FileLinePositionSpan span = keyword.Value.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Do not raise exceptions in finally clauses",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
