using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class MethodGroupConversionRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_style_prefer_method_group_conversion";

    public string RuleId => "IDE0200";

    public string Name => "MethodGroupConversion";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool ParametersMatchArguments(
        SeparatedSyntaxList<ParameterSyntax> parameters,
        SeparatedSyntaxList<ArgumentSyntax> arguments)
    {
        if (parameters.Count != arguments.Count || parameters.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < parameters.Count; i++)
        {
            if (arguments[i].Expression is not IdentifierNameSyntax id
                || id.Identifier.Text != parameters[i].Identifier.Text)
            {
                return false;
            }

            // Must not have ref/out/in modifiers on the argument
            if (arguments[i].RefKindKeyword != default)
            {
                return false;
            }
        }

        return true;
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
        InvocationExpressionSyntax? invocation;
        SeparatedSyntaxList<ParameterSyntax> parameters;
        SyntaxNode reportNode;

        switch (node)
        {
            // x => Method(x)
            case SimpleLambdaExpressionSyntax simpleLambda
                when simpleLambda.ExpressionBody is InvocationExpressionSyntax inv:
                invocation = inv;
                parameters = [simpleLambda.Parameter];
                reportNode = simpleLambda;
                break;

            // (x, y) => Method(x, y)
            case ParenthesizedLambdaExpressionSyntax parenLambda
                when parenLambda.ExpressionBody is InvocationExpressionSyntax inv:
                invocation = inv;
                parameters = parenLambda.ParameterList.Parameters;
                reportNode = parenLambda;
                break;

            default:
                return;
        }

        // Invocation target must be a simple name or member access (not another lambda)
        if (invocation.Expression is not (IdentifierNameSyntax or MemberAccessExpressionSyntax))
        {
            return;
        }

        if (!ParametersMatchArguments(parameters, invocation.ArgumentList.Arguments))
        {
            return;
        }

        FileLinePositionSpan span = reportNode.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Use method group '{invocation.Expression}' instead of lambda",
                Severity = LintSeverity.Warning,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
