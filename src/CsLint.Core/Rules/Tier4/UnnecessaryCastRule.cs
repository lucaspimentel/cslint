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
