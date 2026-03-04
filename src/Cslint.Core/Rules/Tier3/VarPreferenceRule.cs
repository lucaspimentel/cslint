using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class VarPreferenceRule : IRuleDefinition
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
        var walker = new VarWalker(context.FilePath, context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    private sealed class VarWalker(string filePath, LintConfiguration config) : CSharpSyntaxWalker
    {
        public List<LintDiagnostic> Diagnostics { get; } = [];

        public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
        {
            if (node.Modifiers.Any(SyntaxKind.ConstKeyword))
            {
                base.VisitLocalDeclarationStatement(node);
                return;
            }

            bool usesVar = node.Declaration.Type.IsVar;
            VariableDeclaratorSyntax? declarator = node.Declaration.Variables.FirstOrDefault();

            if (declarator?.Initializer is null)
            {
                base.VisitLocalDeclarationStatement(node);
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
                // Using var - check if explicit type is preferred
                // Literals (42, "hello", true) imply built-in types
                if (isLiteral && string.Equals(varForBuiltIn, "false", StringComparison.OrdinalIgnoreCase))
                {
                    AddDiagnostic(node.Declaration.Type, "Use explicit type instead of 'var' for built-in types");
                }
                else if (!isApparent && string.Equals(varElsewhere, "false", StringComparison.OrdinalIgnoreCase))
                {
                    AddDiagnostic(node.Declaration.Type, "Use explicit type instead of 'var' when type is not apparent");
                }
            }
            else
            {
                // Using explicit type - check if var is preferred
                bool isBuiltIn = IsBuiltInType(node.Declaration.Type);

                if (isApparent && string.Equals(varWhenApparent, "true", StringComparison.OrdinalIgnoreCase))
                {
                    AddDiagnostic(node.Declaration.Type, "Use 'var' when type is apparent from the right-hand side");
                }
                else if (isBuiltIn && string.Equals(varForBuiltIn, "true", StringComparison.OrdinalIgnoreCase))
                {
                    AddDiagnostic(node.Declaration.Type, "Use 'var' for built-in types");
                }
            }

            base.VisitLocalDeclarationStatement(node);
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

        private void AddDiagnostic(SyntaxNode node, string message)
        {
            FileLinePositionSpan span = node.GetLocation().GetLineSpan();

            Diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = "CSLINT200",
                    Message = message,
                    Severity = LintSeverity.Info,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
