using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class FlagsEnumValuesRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2217.severity";

    public string RuleId => "CA2217";

    public string Name => "DoNotMarkEnumsWithFlagsAttribute";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasFlagsAttribute(EnumDeclarationSyntax enumDecl)
    {
        foreach (AttributeListSyntax attrList in enumDecl.AttributeLists)
        {
            foreach (AttributeSyntax attr in attrList.Attributes)
            {
                string name = attr.Name.ToString();

                if (name is "Flags" or "FlagsAttribute" or
                    "System.Flags" or "System.FlagsAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool TryGetExplicitValue(
        EnumMemberDeclarationSyntax member, out long value)
    {
        value = 0;

        if (member.EqualsValue is null)
        {
            return false;
        }

        ExpressionSyntax expr = member.EqualsValue.Value;

        // Handle negative: -1, -2, etc.
        if (expr is PrefixUnaryExpressionSyntax prefix &&
            prefix.IsKind(SyntaxKind.UnaryMinusExpression) &&
            prefix.Operand is LiteralExpressionSyntax innerLit &&
            innerLit.Token.Value is int innerIntVal)
        {
            value = -innerIntVal;
            return true;
        }

        if (expr is not LiteralExpressionSyntax literal)
        {
            return false;
        }

        // Only handle integer literal values
        if (literal.Token.Value is int intVal)
        {
            value = intVal;
            return true;
        }

        if (literal.Token.Value is long longVal)
        {
            value = longVal;
            return true;
        }

        if (literal.Token.Value is uint uintVal)
        {
            value = uintVal;
            return true;
        }

        if (literal.Token.Value is ulong ulongVal && ulongVal <= long.MaxValue)
        {
            value = (long)ulongVal;
            return true;
        }

        return false;
    }

    private static bool IsPowerOfTwoOrZero(long value) =>
        value >= 0 && (value & (value - 1)) == 0;

    private static bool IsCombinationOfOtherValues(
        long value, HashSet<long> powers)
    {
        if (value <= 0)
        {
            return false;
        }

        // Check if value is a bitwise OR of known powers of two
        long remaining = value;

        foreach (long power in powers)
        {
            if (power == 0)
            {
                continue;
            }

            if ((remaining & power) == power)
            {
                remaining &= ~power;
            }
        }

        return remaining == 0;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (EnumDeclarationSyntax enumDecl in
            context.Root.DescendantNodes().OfType<EnumDeclarationSyntax>())
        {
            if (!HasFlagsAttribute(enumDecl))
            {
                continue;
            }

            // Collect all explicit values that are powers of two
            var powers = new HashSet<long>();
            var allValues = new List<(EnumMemberDeclarationSyntax Member, long Value)>();
            bool hasUnresolvableValues = false;

            long implicitValue = 0;

            foreach (EnumMemberDeclarationSyntax member in enumDecl.Members)
            {
                long value;

                if (TryGetExplicitValue(member, out long explicitValue))
                {
                    value = explicitValue;
                    implicitValue = explicitValue + 1;
                }
                else if (member.EqualsValue is null)
                {
                    value = implicitValue;
                    implicitValue++;
                }
                else
                {
                    // Expression we can't resolve (e.g., A | B)
                    hasUnresolvableValues = true;
                    break;
                }

                allValues.Add((member, value));

                if (IsPowerOfTwoOrZero(value))
                {
                    powers.Add(value);
                }
            }

            if (hasUnresolvableValues)
            {
                continue;
            }

            // Check each value: must be 0, a power of 2,
            // or a combination of other powers of 2
            foreach ((EnumMemberDeclarationSyntax member, long value) in
                allValues)
            {
                if (IsPowerOfTwoOrZero(value))
                {
                    continue;
                }

                if (IsCombinationOfOtherValues(value, powers))
                {
                    continue;
                }

                FileLinePositionSpan span =
                    enumDecl.Identifier.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"[Flags] enum '{enumDecl.Identifier.Text}'"
                            + $" has member '{member.Identifier.Text}' with "
                            + $"value {value} that is not a power of 2 or a "
                            + "combination of other values",
                        Severity = DefaultSeverity,
                        FilePath = context.FilePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });

                break; // one diagnostic per enum is enough
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
