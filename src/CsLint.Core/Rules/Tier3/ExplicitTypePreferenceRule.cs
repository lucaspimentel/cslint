using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0008: Use explicit type instead of 'var'.
/// Fires when 'var' is used but explicit type is preferred.
/// </summary>
public sealed class ExplicitTypePreferenceRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "IDE0008";

    public string Name => "ExplicitTypePreference";

    public IReadOnlyList<string> ConfigKeys { get; } =
    [
        "csharp_style_var_for_built_in_types",
        "csharp_style_var_elsewhere",
    ];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static void AddDiagnostic(SyntaxNode node, string message, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = node.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "IDE0008",
                Message = message,
                Severity = LintSeverity.Info,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_var_for_built_in_types") is not null ||
        configuration.GetValue("csharp_style_var_elsewhere") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitLocalDeclarationStatement(
        LocalDeclarationStatementSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        if (node.Modifiers.Any(SyntaxKind.ConstKeyword))
        {
            return;
        }

        if (!node.Declaration.Type.IsVar)
        {
            return;
        }

        VariableDeclaratorSyntax? declarator = node.Declaration.Variables.FirstOrDefault();

        if (declarator?.Initializer is null)
        {
            return;
        }

        ExpressionSyntax initializer = declarator.Initializer.Value;

        if (initializer.IsKind(SyntaxKind.NullLiteralExpression) ||
            initializer.IsKind(SyntaxKind.DefaultLiteralExpression))
        {
            return;
        }

        bool isApparent = VarTypeHelper.IsTypeApparent(initializer);
        bool isLiteral = initializer is LiteralExpressionSyntax;

        (string? varForBuiltIn, string? _) = config.GetValueWithSeverity("csharp_style_var_for_built_in_types");
        (string? varElsewhere, string? _) = config.GetValueWithSeverity("csharp_style_var_elsewhere");

        if (isLiteral && string.Equals(varForBuiltIn, "false", StringComparison.OrdinalIgnoreCase))
        {
            AddDiagnostic(node.Declaration.Type, "Use explicit type instead of 'var' for built-in types", diagnostics);
        }
        else if (!isApparent && string.Equals(varElsewhere, "false", StringComparison.OrdinalIgnoreCase))
        {
            AddDiagnostic(node.Declaration.Type, "Use explicit type instead of 'var' when type is not apparent", diagnostics);
        }
    }
}
