using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class DeclarationStatementSpacingRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_space_around_declaration_statements";

    public string RuleId => "CSLINT291";

    public string Name => "DeclarationStatementSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasMultipleSpaces(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) && trivia.Span.Length > 1)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration)
    {
        string? value = configuration.GetValue(ConfigKey);

        // "false" = enforce no extra spaces; "ignore" = don't check
        return string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);
    }

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
        if (node is not LocalDeclarationStatementSyntax localDecl)
        {
            return;
        }

        // Check for extra spaces around the type and variable names
        SyntaxToken typeLastToken = localDecl.Declaration.Type.GetLastToken();

        if (HasMultipleSpaces(typeLastToken))
        {
            FileLinePositionSpan span = typeLastToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Remove extra spaces in declaration statement",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
