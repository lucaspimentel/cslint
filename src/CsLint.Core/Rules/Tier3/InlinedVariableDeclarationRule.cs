using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class InlinedVariableDeclarationRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_inlined_variable_declaration";

    public string RuleId => "CSLINT272";

    public string Name => "InlinedVariableDeclaration";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool HasOutArgument(StatementSyntax statement, string variableName)
    {
        foreach (ArgumentSyntax arg in statement.DescendantNodes().OfType<ArgumentSyntax>())
        {
            if (arg.RefKindKeyword.IsKind(SyntaxKind.OutKeyword)
                && arg.Expression is IdentifierNameSyntax id
                && id.Identifier.Text == variableName)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
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

        if (localDecl.Declaration.Variables.Count != 1)
        {
            return;
        }

        VariableDeclaratorSyntax variable = localDecl.Declaration.Variables[0];

        if (variable.Initializer is not null)
        {
            return;
        }

        string variableName = variable.Identifier.Text;

        if (localDecl.Parent is not BlockSyntax block)
        {
            return;
        }

        int index = block.Statements.IndexOf(localDecl);

        for (int i = index + 1; i < block.Statements.Count; i++)
        {
            if (!HasOutArgument(block.Statements[i], variableName))
            {
                continue;
            }

            FileLinePositionSpan span = localDecl.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message =
                        $"Variable '{variableName}' can be declared inline as an out variable",
                    Severity = LintSeverity.Warning,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });

            return;
        }
    }
}
