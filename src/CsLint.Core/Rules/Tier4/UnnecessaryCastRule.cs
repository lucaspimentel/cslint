using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class UnnecessaryCastRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "CSLINT306";

    public string Name => "UnnecessaryCast";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_diagnostic.CSLINT306.severity"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsArithmeticOperand(CastExpressionSyntax cast) =>
        cast.Parent is BinaryExpressionSyntax binary &&
        binary.Kind() is SyntaxKind.AddExpression
            or SyntaxKind.SubtractExpression
            or SyntaxKind.MultiplyExpression
            or SyntaxKind.DivideExpression
            or SyntaxKind.ModuloExpression;

    private static bool IsMethodArgument(CastExpressionSyntax cast, SemanticModel model)
    {
        if (cast.Parent is not ArgumentSyntax)
        {
            return false;
        }

        // Find the containing invocation
        SyntaxNode? invocation = cast.Parent.Parent?.Parent;

        if (invocation is not InvocationExpressionSyntax inv)
        {
            return false;
        }

        SymbolInfo symbolInfo = model.GetSymbolInfo(inv);

        // If we can't resolve the method, be conservative — assume the cast is needed
        if (symbolInfo.Symbol is not IMethodSymbol method)
        {
            return true;
        }

        // Check if the method has overloads with the same parameter count —
        // if so, the cast may be needed for disambiguation
        INamedTypeSymbol? containingType = method.ContainingType;

        if (containingType is null)
        {
            return true;
        }

        return containingType.GetMembers(method.Name)
            .OfType<IMethodSymbol>()
            .Count(m => m.Parameters.Length == method.Parameters.Length) > 1;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetSeverityForKey("dotnet_diagnostic.CSLINT306.severity") != LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        foreach (CastExpressionSyntax cast in context.Root.DescendantNodes().OfType<CastExpressionSyntax>())
        {
            TypeInfo castTypeInfo = model.GetTypeInfo(cast);
            TypeInfo exprTypeInfo = model.GetTypeInfo(cast.Expression);

            ITypeSymbol? castTargetType = castTypeInfo.Type;
            ITypeSymbol? originalType = exprTypeInfo.Type;

            if (castTargetType is null || originalType is null)
            {
                continue;
            }

            if (castTargetType.TypeKind == TypeKind.Error || originalType.TypeKind == TypeKind.Error)
            {
                continue;
            }

            // Skip dynamic types
            if (castTargetType.TypeKind == TypeKind.Dynamic || originalType.TypeKind == TypeKind.Dynamic)
            {
                continue;
            }

            // Skip pointer types
            if (castTargetType.TypeKind == TypeKind.Pointer || originalType.TypeKind == TypeKind.Pointer ||
                castTargetType.TypeKind == TypeKind.FunctionPointer || originalType.TypeKind == TypeKind.FunctionPointer)
            {
                continue;
            }

            // Same type cast is always unnecessary
            if (SymbolEqualityComparer.Default.Equals(castTargetType, originalType))
            {
                ReportDiagnostic(cast, context.FilePath, diagnostics);
                continue;
            }

            // Skip nullable wrapping (e.g., (int?)intValue)
            if (castTargetType is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T })
            {
                continue;
            }

            // Check if the conversion is implicit (and not user-defined)
            Conversion conversion = model.ClassifyConversion(cast.Expression, castTargetType);

            if (conversion.IsImplicit && !conversion.IsUserDefined && !conversion.IsBoxing)
            {
                // Skip widening casts in arithmetic expressions — removing the cast
                // would change the operation semantics (e.g., integer vs float division)
                if (IsArithmeticOperand(cast) && conversion.IsNumeric)
                {
                    continue;
                }

                // Skip casts used for method overload resolution — removing the cast
                // may select a different overload (e.g., Write(int) vs Write(short))
                if (IsMethodArgument(cast, model))
                {
                    continue;
                }

                ReportDiagnostic(cast, context.FilePath, diagnostics);
            }
        }
    }

    private void ReportDiagnostic(CastExpressionSyntax cast, string filePath, List<LintDiagnostic> diagnostics)
    {
        LinePosition start = cast.GetLocation().GetLineSpan().StartLinePosition;

        diagnostics.Add(new LintDiagnostic
        {
            RuleId = RuleId,
            Message = $"Unnecessary cast to '{cast.Type}'",
            Severity = DefaultSeverity,
            FilePath = filePath,
            Line = start.Line + 1,
            Column = start.Character + 1,
        });
    }
}
