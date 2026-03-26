using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ObsoleteWithoutMessageRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1041.severity";

    public string RuleId => "CA1041";

    public string Name => "ProvideObsoleteAttributeMessage";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsObsoleteAttribute(AttributeSyntax attr)
    {
        string name = attr.Name.ToString();

        return name is "Obsolete" or "ObsoleteAttribute" or
            "System.Obsolete" or "System.ObsoleteAttribute";
    }

    private static bool HasEmptyOrMissingMessage(AttributeSyntax attr)
    {
        // [Obsolete] — no argument list
        if (attr.ArgumentList is null || attr.ArgumentList.Arguments.Count == 0)
        {
            return true;
        }

        // [Obsolete("")] — empty string message
        AttributeArgumentSyntax firstArg = attr.ArgumentList.Arguments[0];

        if (firstArg.Expression is LiteralExpressionSyntax literal &&
            literal.Token.ValueText.Length == 0)
        {
            return true;
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (AttributeSyntax attr in context.Root.DescendantNodes().OfType<AttributeSyntax>())
        {
            if (!IsObsoleteAttribute(attr))
            {
                continue;
            }

            if (!HasEmptyOrMissingMessage(attr))
            {
                continue;
            }

            FileLinePositionSpan span = attr.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Provide a message for the ObsoleteAttribute",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
