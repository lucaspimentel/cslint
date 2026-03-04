using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class PredefinedTypeRule : IRuleDefinition
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

    public bool IsEnabled(LintConfiguration configuration)
    {
        (string? pref1, string? _) = configuration.GetValueWithSeverity("dotnet_style_predefined_type_for_locals_parameters_members");
        (string? pref2, string? _) = configuration.GetValueWithSeverity("dotnet_style_predefined_type_for_member_access");

        return string.Equals(pref1, "true", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(pref2, "true", StringComparison.OrdinalIgnoreCase);
    }

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? localsPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_predefined_type_for_locals_parameters_members");
        (string? memberPref, string? _) = context.Configuration.GetValueWithSeverity("dotnet_style_predefined_type_for_member_access");

        bool checkLocals = string.Equals(localsPref, "true", StringComparison.OrdinalIgnoreCase);
        bool checkMemberAccess = string.Equals(memberPref, "true", StringComparison.OrdinalIgnoreCase);

        var diagnostics = new List<LintDiagnostic>();

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            switch (node)
            {
                case IdentifierNameSyntax id when checkLocals && FrameworkTypeNames.Contains(id.Identifier.Text):
                {
                    // Check it's a type usage, not a member access receiver
                    if (node.Parent is not MemberAccessExpressionSyntax)
                    {
                        AddDiagnostic(diagnostics, id.Identifier, context.FilePath);
                    }

                    break;
                }

                case MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax typeName } when checkMemberAccess:
                {
                    if (FrameworkTypeNames.Contains(typeName.Identifier.Text))
                    {
                        AddDiagnostic(diagnostics, typeName.Identifier, context.FilePath);
                    }

                    break;
                }

                case QualifiedNameSyntax { Right: IdentifierNameSyntax right } when checkLocals:
                {
                    if (FrameworkTypeNames.Contains(right.Identifier.Text))
                    {
                        AddDiagnostic(diagnostics, right.Identifier, context.FilePath);
                    }

                    break;
                }
            }
        }

        return diagnostics;
    }

    private static void AddDiagnostic(List<LintDiagnostic> diagnostics, SyntaxToken identifier, string filePath)
    {
        FileLinePositionSpan span = identifier.GetLocation().GetLineSpan();

        diagnostics.Add(
            new LintDiagnostic
            {
                RuleId = "CSLINT208",
                Message = $"Use predefined type keyword instead of '{identifier.Text}'",
                Severity = LintSeverity.Info,
                FilePath = filePath,
                Line = span.StartLinePosition.Line + 1,
                Column = span.StartLinePosition.Character + 1,
            });
    }
}
