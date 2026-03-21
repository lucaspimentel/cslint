using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class UnnecessaryInitializationRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_no_unnecessary_initialization";

    private static readonly HashSet<string> IntegralTypes = new(StringComparer.Ordinal)
    {
        "int", "long", "short", "byte", "sbyte",
        "uint", "ulong", "ushort", "nint", "nuint",
    };

    private static readonly HashSet<string> ReferenceTypeKeywords = new(StringComparer.Ordinal)
    {
        "string", "object",
    };

    public string RuleId => "CSLINT238";

    public string Name => "UnnecessaryInitialization";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    private static bool IsUnnecessaryDefault(string typeText, ExpressionSyntax value)
    {
        // default or default(T) is always unnecessary for any explicit type
        if (value.IsKind(SyntaxKind.DefaultLiteralExpression) ||
            value is DefaultExpressionSyntax)
        {
            return true;
        }

        // Nullable types (e.g., int?, string?) — flag null
        if (typeText.EndsWith('?') && value.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return true;
        }

        // Reference type keywords — flag null
        if (ReferenceTypeKeywords.Contains(typeText) && value.IsKind(SyntaxKind.NullLiteralExpression))
        {
            return true;
        }

        // bool — flag false
        if (typeText == "bool" && value.IsKind(SyntaxKind.FalseLiteralExpression))
        {
            return true;
        }

        // char — flag '\0'
        if (typeText == "char" && value.IsKind(SyntaxKind.CharacterLiteralExpression))
        {
            var literal = (LiteralExpressionSyntax)value;

            if (literal.Token.ValueText == "\0")
            {
                return true;
            }
        }

        // Integral types — flag 0
        if (IntegralTypes.Contains(typeText) && IsZeroNumericLiteral(value))
        {
            return true;
        }

        // float — flag 0f, 0.0f, etc.
        if (typeText == "float" && IsZeroNumericLiteral(value))
        {
            return true;
        }

        // double — flag 0d, 0.0d, 0.0, etc.
        if (typeText == "double" && IsZeroNumericLiteral(value))
        {
            return true;
        }

        // decimal — flag 0m, 0.0m, etc.
        if (typeText == "decimal" && IsZeroNumericLiteral(value))
        {
            return true;
        }

        return false;
    }

    private static bool IsZeroNumericLiteral(ExpressionSyntax value)
    {
        if (!value.IsKind(SyntaxKind.NumericLiteralExpression))
        {
            return false;
        }

        var literal = (LiteralExpressionSyntax)value;

        return literal.Token.Value switch
        {
            int i => i == 0,
            long l => l == 0,
            short s => s == 0,
            byte b => b == 0,
            sbyte sb => sb == 0,
            uint ui => ui == 0,
            ulong ul => ul == 0,
            ushort us => us == 0,
            float f => f == 0f,
            double d => d == 0d,
            decimal m => m == 0m,
            _ => false,
        };
    }

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedStyleWalker([this], context.Configuration);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void IStyleRuleHandler.VisitFieldDeclaration(
        FieldDeclarationSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        VariableDeclarationSyntax declaration = node.Declaration;
        TypeSyntax typeSyntax = declaration.Type;

        // Skip var — can't resolve inferred types without semantic model
        if (typeSyntax.IsVar)
        {
            return;
        }

        string typeText = typeSyntax.ToString();

        foreach (VariableDeclaratorSyntax declarator in declaration.Variables)
        {
            if (declarator.Initializer is not { } initializer)
            {
                continue;
            }

            ExpressionSyntax value = initializer.Value;

            if (!IsUnnecessaryDefault(typeText, value))
            {
                continue;
            }

            FileLinePositionSpan span = initializer.EqualsToken.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Do not initialize field '{declarator.Identifier.Text}' to its default value",
                    Severity = LintSeverity.Info,
                    FilePath = span.Path,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
