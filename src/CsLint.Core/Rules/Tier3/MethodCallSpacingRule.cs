using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class MethodCallSpacingRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ParamListKey = "csharp_space_between_method_call_parameter_list_parentheses";

    private const string EmptyParamListKey = "csharp_space_between_method_call_empty_parameter_list_parentheses";

    private const string NameAndParenKey = "csharp_space_between_method_call_name_and_opening_parenthesis";

    public string RuleId => "CSLINT288";

    public string Name => "MethodCallSpacing";

    public IReadOnlyList<string> ConfigKeys { get; } = [ParamListKey, EmptyParamListKey, NameAndParenKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

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
        if (node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        ArgumentListSyntax argList = invocation.ArgumentList;

        // Name and open paren — check the token before the open paren
        if (invocation.Expression is IdentifierNameSyntax or MemberAccessExpressionSyntax)
        {
            SyntaxToken nameToken = invocation.Expression.GetLastToken();
            CheckSpacing(config, NameAndParenKey, nameToken, "method call name and parenthesis", filePath, diagnostics);
        }

        // Inside parentheses
        if (argList.Arguments.Count == 0)
        {
            CheckSpacing(config, EmptyParamListKey, argList.OpenParenToken, "empty argument list parentheses", filePath, diagnostics);
        }
        else
        {
            CheckSpacing(config, ParamListKey, argList.OpenParenToken, "argument list parentheses", filePath, diagnostics);
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
        bool hasSpace = TriviaHelper.HasTrailingSpace(token);

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
