using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class MethodDeclarationSpacingRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ParamListKey = "csharp_space_between_method_declaration_parameter_list_parentheses";

    private const string EmptyParamListKey = "csharp_space_between_method_declaration_empty_parameter_list_parentheses";

    private const string NameAndParenKey = "csharp_space_between_method_declaration_name_and_open_parenthesis";

    public string RuleId => "CSLINT287";

    public string Name => "MethodDeclarationSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ParamListKey, EmptyParamListKey, NameAndParenKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasTrailingSpace(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ParamListKey) is not null
        || configuration.GetValue(EmptyParamListKey) is not null
        || configuration.GetValue(NameAndParenKey) is not null;

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
        if (node is not MethodDeclarationSyntax method)
        {
            return;
        }

        ParameterListSyntax paramList = method.ParameterList;

        // Name and open paren
        CheckSpacing(
            config, NameAndParenKey,
            method.Identifier, "method name and open parenthesis",
            filePath, diagnostics);

        // Inside parentheses (after open paren)
        if (paramList.Parameters.Count == 0)
        {
            CheckSpacing(
                config, EmptyParamListKey,
                paramList.OpenParenToken, "empty parameter list parentheses",
                filePath, diagnostics);
        }
        else
        {
            CheckSpacing(
                config, ParamListKey,
                paramList.OpenParenToken, "parameter list parentheses",
                filePath, diagnostics);
        }
    }

    private void CheckSpacing(
        LintConfiguration config,
        string key,
        SyntaxToken token,
        string context,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        string? value = config.GetValue(key);

        if (value is null)
        {
            return;
        }

        bool requireSpace = string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        bool hasSpace = HasTrailingSpace(token);

        if (requireSpace == hasSpace)
        {
            return;
        }

        string action = requireSpace ? "Add" : "Remove";
        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"{action} space in {context}",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
