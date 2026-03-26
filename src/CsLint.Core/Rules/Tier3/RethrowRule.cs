using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class RethrowRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2200.severity";

    public string RuleId => "CA2200";

    public string Name => "RethrowToPreserveStackDetails";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static CatchClauseSyntax? FindAncestorCatch(SyntaxNode node)
    {
        SyntaxNode? current = node.Parent;

        while (current is not null)
        {
            if (current is CatchClauseSyntax catchClause)
            {
                return catchClause;
            }

            // Stop if we hit a method/lambda boundary
            if (current is BaseMethodDeclarationSyntax or
                LambdaExpressionSyntax or
                AnonymousFunctionExpressionSyntax)
            {
                return null;
            }

            current = current.Parent;
        }

        return null;
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

        foreach (ThrowStatementSyntax throwStatement in context.Root.DescendantNodes().OfType<ThrowStatementSyntax>())
        {
            // Skip bare `throw;` (already correct rethrow)
            if (throwStatement.Expression is not IdentifierNameSyntax identifier)
            {
                continue;
            }

            // Check if the identifier matches the catch clause's exception variable
            CatchClauseSyntax? catchClause = FindAncestorCatch(throwStatement);

            if (catchClause?.Declaration is null)
            {
                continue;
            }

            if (catchClause.Declaration.Identifier.Text == identifier.Identifier.Text)
            {
                FileLinePositionSpan span = throwStatement.ThrowKeyword.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"Use 'throw' instead of 'throw {identifier.Identifier.Text}' to preserve stack details",
                        Severity = DefaultSeverity,
                        FilePath = context.FilePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
