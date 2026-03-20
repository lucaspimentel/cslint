using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ElementsSeparatedByBlankLineRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_elements_separated_by_blank_line";

    public string RuleId => "CSLINT250";

    public string Name => "ElementsSeparatedByBlankLine";

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
        SyntaxList<MemberDeclarationSyntax>? members = node switch
        {
            TypeDeclarationSyntax typeDecl => typeDecl.Members,
            NamespaceDeclarationSyntax nsDecl => nsDecl.Members,
            CompilationUnitSyntax compilationUnit => compilationUnit.Members,
            _ => null,
        };

        if (members is not { Count: > 1 } memberList)
        {
            return;
        }

        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        for (int i = 1; i < memberList.Count; i++)
        {
            SyntaxToken lastTokenOfPrevious = memberList[i - 1].GetLastToken();
            SyntaxToken firstTokenOfCurrent = memberList[i].GetFirstToken();

            if (!NewLineHelper.HasBlankLineBetween(lastTokenOfPrevious, firstTokenOfCurrent))
            {
                FileLinePositionSpan span = firstTokenOfCurrent.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = "Elements should be separated by a blank line",
                        Severity = DefaultSeverity,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
