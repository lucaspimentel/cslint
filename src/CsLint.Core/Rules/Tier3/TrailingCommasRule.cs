using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class TrailingCommasRule : IRuleDefinition, IDescendantNodeHandler
{
    private const string ConfigKey = "csharp_trailing_commas_in_multi_line_initializers";

    public string RuleId => "CSLINT253";

    public string Name => "TrailingCommas";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

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
        (string? pref, string? _) = config.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        switch (node)
        {
            case EnumDeclarationSyntax enumDecl:
                CheckEnumMembers(enumDecl, filePath, diagnostics);
                break;

            case InitializerExpressionSyntax initializer:
                CheckInitializer(initializer, filePath, diagnostics);
                break;
        }
    }

    private void CheckEnumMembers(EnumDeclarationSyntax enumDecl, string filePath, List<LintDiagnostic> diagnostics)
    {
        if (enumDecl.Members.Count == 0)
        {
            return;
        }

        if (!IsMultiLine(enumDecl))
        {
            return;
        }

        // Check if the last separator is a comma
        SeparatedSyntaxList<EnumMemberDeclarationSyntax> members = enumDecl.Members;

        if (members.SeparatorCount < members.Count)
        {
            // No trailing comma after last member
            EnumMemberDeclarationSyntax lastMember = members[^1];
            FileLinePositionSpan span = lastMember.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Multi-line enum should use a trailing comma after the last value",
                    Severity = DefaultSeverity,
                    FilePath = filePath,
                    Line = span.EndLinePosition.Line + 1,
                    Column = span.EndLinePosition.Character + 1,
                });
        }
    }

    private void CheckInitializer(InitializerExpressionSyntax initializer, string filePath, List<LintDiagnostic> diagnostics)
    {
        if (initializer.Expressions.Count == 0)
        {
            return;
        }

        if (!IsMultiLine(initializer))
        {
            return;
        }

        SeparatedSyntaxList<ExpressionSyntax> expressions = initializer.Expressions;

        if (expressions.SeparatorCount < expressions.Count)
        {
            ExpressionSyntax lastExpr = expressions[^1];
            FileLinePositionSpan span = lastExpr.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Multi-line initializer should use a trailing comma after the last element",
                    Severity = DefaultSeverity,
                    FilePath = filePath,
                    Line = span.EndLinePosition.Line + 1,
                    Column = span.EndLinePosition.Character + 1,
                });
        }
    }

    private static bool IsMultiLine(SyntaxNode node)
    {
        FileLinePositionSpan span = node.GetLocation().GetLineSpan();
        return span.StartLinePosition.Line != span.EndLinePosition.Line;
    }
}
