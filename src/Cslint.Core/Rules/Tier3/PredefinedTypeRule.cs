using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PredefinedTypeRule : IRuleDefinition, IDescendantNodeHandler
{
    public string RuleId => "CSLINT208";

    public string Name => "PredefinedType";

    public IReadOnlyList<string> ConfigKeys { get; } =
    [
        "dotnet_style_predefined_type_for_locals_parameters_members",
        "dotnet_style_predefined_type_for_member_access",
    ];

    private static readonly HashSet<string> FrameworkTypeNames = new(StringComparer.Ordinal)
    {
        "Int16", "Int32", "Int64", "UInt16", "UInt32", "UInt64",
        "Single", "Double", "Decimal", "Boolean", "Char", "String",
        "Byte", "SByte", "Object",
    };

    private bool _checkLocals;
    private bool _checkMemberAccess;
    private bool _active;
    private string _filePath = "";

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref1, string? _) = configuration.GetValueWithSeverity(
            "dotnet_style_predefined_type_for_locals_parameters_members");
        (string? pref2, string? _) = configuration.GetValueWithSeverity(
            "dotnet_style_predefined_type_for_member_access");

        return string.Equals(pref1, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pref2, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? localsPref, string? _) = context.Configuration.GetValueWithSeverity(
            "dotnet_style_predefined_type_for_locals_parameters_members");
        (string? memberPref, string? _) = context.Configuration.GetValueWithSeverity(
            "dotnet_style_predefined_type_for_member_access");

        _checkLocals = string.Equals(localsPref, "true", StringComparison.OrdinalIgnoreCase);
        _checkMemberAccess = string.Equals(memberPref, "true", StringComparison.OrdinalIgnoreCase);
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
        (string? localsPref, string? _) = config.GetValueWithSeverity(
            "dotnet_style_predefined_type_for_locals_parameters_members");
        (string? memberPref, string? _) = config.GetValueWithSeverity(
            "dotnet_style_predefined_type_for_member_access");

        _checkLocals = string.Equals(localsPref, "true", StringComparison.OrdinalIgnoreCase);
        _checkMemberAccess = string.Equals(memberPref, "true", StringComparison.OrdinalIgnoreCase);
        _active = _checkLocals || _checkMemberAccess;
        _filePath = filePath;
    }

    internal void Reset() => _active = false;

    public void VisitNode(SyntaxNode node, List<LintDiagnostic> diagnostics)
    {
        if (!_active)
        {
            return;
        }

        switch (node)
        {
            case IdentifierNameSyntax id when _checkLocals && FrameworkTypeNames.Contains(id.Identifier.Text):
            {
                if (node.Parent is not MemberAccessExpressionSyntax)
                {
                    AddDiagnostic(diagnostics, id.Identifier);
                }

                break;
            }

            case MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax typeName } when _checkMemberAccess:
            {
                if (FrameworkTypeNames.Contains(typeName.Identifier.Text))
                {
                    AddDiagnostic(diagnostics, typeName.Identifier);
                }

                break;
            }

            case QualifiedNameSyntax { Right: IdentifierNameSyntax right } when _checkLocals:
            {
                if (FrameworkTypeNames.Contains(right.Identifier.Text))
                {
                    AddDiagnostic(diagnostics, right.Identifier);
                }

                break;
            }
        }
    }

    private void AddDiagnostic(List<LintDiagnostic> diagnostics, SyntaxToken identifier)
    {
        FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "CSLINT208",
                Message = $"Use predefined type keyword instead of '{identifier.Text}'",
                Severity = LintSeverity.Info,
                FilePath = _filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
