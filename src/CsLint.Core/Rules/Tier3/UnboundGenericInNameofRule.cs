using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class UnboundGenericInNameofRule : IRuleDefinition
{
    public string RuleId => "IDE0340";

    public string Name => "UnboundGenericInNameof";

    public IReadOnlyList<string> ConfigKeys { get; } = ["csharp_style_prefer_unbound_generic_type_in_nameof"];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("csharp_style_prefer_unbound_generic_type_in_nameof") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration
            .GetValueWithSeverity("csharp_style_prefer_unbound_generic_type_in_nameof");

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            // Look for nameof(SomeType<T1, T2>) invocations
            if (node is not InvocationExpressionSyntax invocation)
            {
                continue;
            }

            if (invocation.Expression is not IdentifierNameSyntax identifier ||
                identifier.Identifier.Text != "nameof")
            {
                continue;
            }

            if (invocation.ArgumentList.Arguments.Count != 1)
            {
                continue;
            }

            ExpressionSyntax argument = invocation.ArgumentList.Arguments[0].Expression;

            // The argument to nameof could be a GenericNameSyntax directly,
            // or a MemberAccessExpression ending in a GenericNameSyntax
            GenericNameSyntax? genericName = argument switch
            {
                GenericNameSyntax g => g,
                MemberAccessExpressionSyntax memberAccess
                    when memberAccess.Name is GenericNameSyntax g => g,
                AliasQualifiedNameSyntax alias
                    when alias.Name is GenericNameSyntax g => g,
                QualifiedNameSyntax qualified
                    when qualified.Right is GenericNameSyntax g => g,
                _ => null,
            };

            if (genericName is null || genericName.TypeArgumentList.Arguments.Count == 0)
            {
                continue;
            }

            // Check if any type argument is not an OmittedTypeArgumentSyntax
            bool hasConcreteTypeArgs = false;

            foreach (TypeSyntax typeArg in genericName.TypeArgumentList.Arguments)
            {
                if (typeArg is not OmittedTypeArgumentSyntax)
                {
                    hasConcreteTypeArgs = true;
                    break;
                }
            }

            if (!hasConcreteTypeArgs)
            {
                continue;
            }

            FileLinePositionSpan span = genericName.TypeArgumentList.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Use unbound generic type 'nameof({genericName.Identifier.Text}<>)' instead of closed generic type",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics;
    }
}
