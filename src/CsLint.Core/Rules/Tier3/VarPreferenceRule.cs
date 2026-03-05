using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class VarPreferenceRule : IRuleDefinition, IStyleRuleHandler
{
    public string RuleId => "CSLINT200";

    public string Name => "VarPreference";

    public IReadOnlyList<string> ConfigKeys { get; } =
    [
        "csharp_style_var_for_built_in_types",
        "csharp_style_var_when_type_is_apparent",
        "csharp_style_var_elsewhere",
    ];

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_var_when_type_is_apparent") is not null ||
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

        bool usesVar = node.Declaration.Type.IsVar;
        VariableDeclaratorSyntax? declarator = node.Declaration.Variables.FirstOrDefault();

        if (declarator?.Initializer is null)
        {
            return;
        }

        ExpressionSyntax initializer = declarator.Initializer.Value;
        bool isApparent = IsTypeApparent(initializer);
        bool isLiteral = initializer is LiteralExpressionSyntax;

        (string? varForBuiltIn, string? _) = config.GetValueWithSeverity("csharp_style_var_for_built_in_types");
        (string? varWhenApparent, string? _) = config.GetValueWithSeverity("csharp_style_var_when_type_is_apparent");
        (string? varElsewhere, string? _) = config.GetValueWithSeverity("csharp_style_var_elsewhere");

        if (usesVar)
        {
            if (isLiteral && string.Equals(varForBuiltIn, "false", StringComparison.OrdinalIgnoreCase))
            {
                AddDiagnostic(node.Declaration.Type, "Use explicit type instead of 'var' for built-in types", diagnostics);
            }
            else if (!isApparent && string.Equals(varElsewhere, "false", StringComparison.OrdinalIgnoreCase))
            {
                AddDiagnostic(node.Declaration.Type, "Use explicit type instead of 'var' when type is not apparent", diagnostics);
            }
        }
        else
        {
            bool isBuiltIn = IsBuiltInType(node.Declaration.Type);

            if (isApparent && string.Equals(varWhenApparent, "true", StringComparison.OrdinalIgnoreCase))
            {
                AddDiagnostic(node.Declaration.Type, "Use 'var' when type is apparent from the right-hand side", diagnostics);
            }
            else if (isBuiltIn && string.Equals(varForBuiltIn, "true", StringComparison.OrdinalIgnoreCase))
            {
                AddDiagnostic(node.Declaration.Type, "Use 'var' for built-in types", diagnostics);
            }
        }
    }

    private static bool IsTypeApparent(ExpressionSyntax expression) =>
        expression is ObjectCreationExpressionSyntax or
                      ImplicitObjectCreationExpressionSyntax or
                      CastExpressionSyntax or
                      LiteralExpressionSyntax or
                      DefaultExpressionSyntax or
                      ArrayCreationExpressionSyntax;

    private static bool IsBuiltInType(TypeSyntax type)
    {
        string text = type.ToString();
        return text is "int" or "long" or "short" or "byte" or "sbyte" or
                      "uint" or "ulong" or "ushort" or
                      "float" or "double" or "decimal" or
                      "bool" or "char" or "string" or "object";
    }

    private static void AddDiagnostic(SyntaxNode node, string message, List<LintDiagnostic> diagnostics)
    {
        FileLinePositionSpan span = node.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "CSLINT200",
                Message = message,
                Severity = LintSeverity.Info,
                FilePath = span.Path,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
