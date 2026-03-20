using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class EnumValuesOnSeparateLinesRule : IRuleDefinition, IStyleRuleHandler
{
    private const string ConfigKey = "csharp_enum_values_on_separate_lines";

    public string RuleId => "CSLINT246";

    public string Name => "EnumValuesOnSeparateLines";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

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

    void IStyleRuleHandler.VisitEnumDeclaration(
        EnumDeclarationSyntax node,
        LintConfiguration config,
        List<LintDiagnostic> diagnostics)
    {
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = node.Members;

        for (int i = 1; i < members.Count; i++)
        {
            int previousLine = members[i - 1].GetLocation().GetLineSpan().StartLinePosition.Line;
            FileLinePositionSpan currentSpan = members[i].GetLocation().GetLineSpan();
            int currentLine = currentSpan.StartLinePosition.Line;

            if (currentLine == previousLine)
            {
                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Each enum value should be on its own line",
                        Severity = DefaultSeverity,
                        FilePath = currentSpan.Path,
                        Line = currentSpan.StartLinePosition.Line + 1,
                        Column = currentSpan.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
