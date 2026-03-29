using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// SA1216: Using static directives must be placed after other using directives.
/// </summary>
public sealed class StaticUsingDirectiveOrderRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_using_directive_ordering";

    public string RuleId => "SA1216";

    public string Name => "StaticUsingDirectiveOrder";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref, string? _) = configuration.GetValueWithSeverity(ConfigKey);
        return string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        UsingDirectiveHelper.ForEachUsingGroup(context.Root, usings =>
        {
            ClassifiedUsingGroup group = UsingDirectiveHelper.ClassifyGroup(usings);
            CheckStaticAfterRegular(group.Regular, group.Static, ref diagnostics);
        });

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private void CheckStaticAfterRegular(
        List<UsingDirectiveSyntax> regular,
        List<UsingDirectiveSyntax> staticUsings,
        ref List<LintDiagnostic>? diagnostics)
    {
        if (staticUsings.Count == 0 || regular.Count == 0)
        {
            return;
        }

        int lastRegularLine = regular[^1].GetLocation().GetLineSpan().StartLinePosition.Line;
        int firstStaticLine = staticUsings[0].GetLocation().GetLineSpan().StartLinePosition.Line;

        if (firstStaticLine < lastRegularLine)
        {
            foreach (UsingDirectiveSyntax s in staticUsings)
            {
                int line = s.GetLocation().GetLineSpan().StartLinePosition.Line;

                if (line < lastRegularLine)
                {
                    FileLinePositionSpan span = s.GetLocation().GetLineSpan();

                    (diagnostics ??= []).Add(
                        new LintDiagnostic
                        {
                            RuleId = RuleId,
                            Message = "Static using directives must appear after regular using directives",
                            Severity = DefaultSeverity,
                            FilePath = span.Path,
                            Line = span.StartLinePosition.Line + 1,
                            Column = span.StartLinePosition.Character + 1,
                        });

                    break;
                }
            }
        }
    }
}
