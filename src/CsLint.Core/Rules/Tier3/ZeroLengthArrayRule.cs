using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ZeroLengthArrayRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1825.severity";

    public string RuleId => "CA1825";

    public string Name => "AvoidZeroLengthArrayAllocations";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (ArrayCreationExpressionSyntax arrayCreation in
            context.Root.DescendantNodes()
                .OfType<ArrayCreationExpressionSyntax>())
        {
            // Match `new T[0]` — must have exactly one rank specifier
            // with a single size of literal 0
            if (arrayCreation.Type.RankSpecifiers.Count != 1)
            {
                continue;
            }

            ArrayRankSpecifierSyntax rank =
                arrayCreation.Type.RankSpecifiers[0];

            if (rank.Sizes.Count != 1)
            {
                continue;
            }

            if (rank.Sizes[0] is not LiteralExpressionSyntax literal ||
                !literal.IsKind(SyntaxKind.NumericLiteralExpression))
            {
                continue;
            }

            if (literal.Token.Value is not int intValue || intValue != 0)
            {
                continue;
            }

            // Skip if it has an initializer (new int[0] { } is unusual
            // but technically valid)
            if (arrayCreation.Initializer is not null)
            {
                continue;
            }

            FileLinePositionSpan span =
                arrayCreation.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Use Array.Empty<T>() instead of "
                        + "allocating a zero-length array",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
