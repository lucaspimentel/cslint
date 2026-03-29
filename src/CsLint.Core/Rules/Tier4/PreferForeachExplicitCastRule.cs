using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules.Tier4;

internal sealed class PreferForeachExplicitCastRule : IRuleDefinition, ISemanticRuleHandler
{
    public string RuleId => "IDE0220";

    public string Name => "PreferForeachExplicitCast";

    public IReadOnlyList<string> ConfigKeys { get; } =
        ["dotnet_diagnostic.IDE0220.severity", "dotnet_style_prefer_foreach_explicit_cast_in_source"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity("dotnet_diagnostic.IDE0220.severity") is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context) => [];

    public void Analyze(RuleContext context, SemanticModel model, List<LintDiagnostic> diagnostics)
    {
        (string? preferenceValue, _) = context.Configuration.GetValueWithSeverity(
            "dotnet_style_prefer_foreach_explicit_cast_in_source");

        bool alwaysFlag = string.Equals(preferenceValue, "always", StringComparison.OrdinalIgnoreCase);

        foreach (ForEachStatementSyntax forEach in context.Root.DescendantNodes().OfType<ForEachStatementSyntax>())
        {
            ForEachStatementInfo info = model.GetForEachStatementInfo(forEach);

            // No element type resolved
            if (info.ElementType is null or IErrorTypeSymbol)
            {
                continue;
            }

            // Identity conversion — types match exactly
            if (info.ElementConversion.IsIdentity)
            {
                continue;
            }

            // No conversion happening
            if (!info.ElementConversion.Exists)
            {
                continue;
            }

            // Implicit reference conversion (e.g., Derived → Base) is safe, skip
            if (info.ElementConversion.IsImplicit && info.ElementConversion.IsReference)
            {
                continue;
            }

            // For "when_strongly_typed" (default): skip non-generic collections
            // (e.g., IEnumerable, ArrayList) where the element type is object
            // because the collection has no type info. Generic collections like
            // List<object> are still strongly typed and should be flagged.
            if (!alwaysFlag && IsNonGenericCollection(model, forEach.Expression))
            {
                continue;
            }

            ITypeSymbol? iterationVariableType = model.GetTypeInfo(forEach.Type).Type;

            if (iterationVariableType is null or IErrorTypeSymbol)
            {
                continue;
            }

            LinePosition start = forEach.ForEachKeyword.GetLocation().GetLineSpan().StartLinePosition;

            diagnostics.Add(new LintDiagnostic
            {
                RuleId = RuleId,
                Message = $"Use an explicit cast when iterating " +
                    $"'{info.ElementType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}' " +
                    $"as '{iterationVariableType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)}'",
                Severity = DefaultSeverity,
                FilePath = context.FilePath,
                Line = start.Line + 1,
                Column = start.Character + 1,
            });
        }
    }

    private static bool IsNonGenericCollection(
        SemanticModel model,
        ExpressionSyntax collectionExpression)
    {
        ITypeSymbol? collectionType = model.GetTypeInfo(collectionExpression).Type;

        if (collectionType is null)
        {
            return true;
        }

        // Check if the collection type itself or any of its interfaces is generic IEnumerable<T>
        if (collectionType is INamedTypeSymbol { IsGenericType: true })
        {
            return false;
        }

        foreach (INamedTypeSymbol iface in collectionType.AllInterfaces)
        {
            if (iface.IsGenericType &&
                iface.OriginalDefinition.SpecialType ==
                    SpecialType.System_Collections_Generic_IEnumerable_T)
            {
                return false;
            }
        }

        return true;
    }
}
