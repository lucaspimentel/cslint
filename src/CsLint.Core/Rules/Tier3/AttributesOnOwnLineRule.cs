using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class AttributesOnOwnLineRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_attributes_on_own_line";

    public string RuleId => "SA1134";

    public string Name => "AttributesOnOwnLine";

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

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, context.Configuration, context.FilePath, diagnostics);
        }

        return diagnostics;
    }

    public void VisitNode(
        SyntaxNode node,
        LintConfiguration config,
        string filePath,
        List<LintDiagnostic> diagnostics)
    {
        if (node is not MemberDeclarationSyntax member)
        {
            return;
        }

        SyntaxList<AttributeListSyntax> attributeLists = member.AttributeLists;

        if (attributeLists.Count == 0)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        AttributeListSyntax lastAttrList = attributeLists[^1];
        int attrEndLine = lastAttrList.GetLocation().GetLineSpan().EndLinePosition.Line;

        // Find the first token after the attribute lists (keyword, identifier, modifier, etc.)
        SyntaxToken firstTokenAfterAttrs = lastAttrList.CloseBracketToken.GetNextToken();

        if (firstTokenAfterAttrs == default)
        {
            return;
        }

        int memberStartLine = firstTokenAfterAttrs.GetLocation().GetLineSpan().StartLinePosition.Line;

        if (attrEndLine == memberStartLine)
        {
            FileLinePositionSpan span = lastAttrList.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Attributes should be on their own line, separate from the declaration",
                    Severity = DefaultSeverity,
                    FilePath = filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
