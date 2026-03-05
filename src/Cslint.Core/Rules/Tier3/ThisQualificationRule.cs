using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ThisQualificationRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT204";

    public string Name => "ThisQualification";

    public IReadOnlyList<string> ConfigKeys { get; } =
    [
        "dotnet_style_qualification_for_field",
        "dotnet_style_qualification_for_property",
        "dotnet_style_qualification_for_method",
        "dotnet_style_qualification_for_event",
    ];

    private bool _active;
    private string _filePath = "";

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue("dotnet_style_qualification_for_field") is not null ||
        configuration.GetValue("dotnet_style_qualification_for_property") is not null ||
        configuration.GetValue("dotnet_style_qualification_for_method") is not null ||
        configuration.GetValue("dotnet_style_qualification_for_event") is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? fieldPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_field");
        (string? propPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_property");
        (string? methodPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_method");
        (string? eventPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_qualification_for_event");

        bool anyDisallowed =
            string.Equals(fieldPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(propPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(methodPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(eventPref, "false", StringComparison.OrdinalIgnoreCase);

        if (!anyDisallowed)
        {
            return [];
        }

        _active = true;
        _filePath = context.FilePath;
        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            VisitNode(node, diagnostics);
        }

        _active = false;
        return diagnostics;
    }

    internal void Initialize(LintConfiguration config, string filePath)
    {
        (string? fieldPref, string? _) = config.GetValueWithSeverity("dotnet_style_qualification_for_field");
        (string? propPref, string? _) = config.GetValueWithSeverity("dotnet_style_qualification_for_property");
        (string? methodPref, string? _) = config.GetValueWithSeverity("dotnet_style_qualification_for_method");
        (string? eventPref, string? _) = config.GetValueWithSeverity("dotnet_style_qualification_for_event");

        _active =
            string.Equals(fieldPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(propPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(methodPref, "false", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(eventPref, "false", StringComparison.OrdinalIgnoreCase);
        _filePath = filePath;
    }

    internal void Reset() => _active = false;

    public void VisitNode(SyntaxNode node, List<LintDiagnostic> diagnostics)
    {
        if (!_active)
        {
            return;
        }

        if (node is MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess)
        {
            FileLinePositionSpan span = memberAccess.Expression.GetLocation().GetLineSpan();

            diagnostics.Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = $"Remove 'this.' qualification from '{memberAccess.Name}'",
                    Severity = LintSeverity.Warning,
                    FilePath = _filePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }
    }
}
