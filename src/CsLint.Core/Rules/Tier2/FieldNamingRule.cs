using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier2;

public sealed class FieldNamingRule : IRuleDefinition, INamingRuleHandler
{
    public string RuleId => "CSLINT104";

    public string Name => "FieldNaming";

    public IReadOnlyList<string> ConfigKeys { get; } = ["dotnet_naming_rule.private_fields_should_be_underscore_camel_case"];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        // Only check private/internal instance fields (not const, not static, not events)
        if (node.Modifiers.Any(SyntaxKind.ConstKeyword) ||
            node.Modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return;
        }

        // Skip fields in [StructLayout] types — field names must match native APIs
        if (HasStructLayoutAttribute(node))
        {
            return;
        }

        bool isPrivate = !node.Modifiers.Any(SyntaxKind.PublicKeyword) &&
                         !node.Modifiers.Any(SyntaxKind.ProtectedKeyword) &&
                         !node.Modifiers.Any(SyntaxKind.InternalKeyword);

        if (!isPrivate)
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
        {
            string name = variable.Identifier.Text;

            if (!NamingHelper.IsUnderscoreCamelCase(name))
            {
                FileLinePositionSpan span = variable.Identifier.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = "CSLINT104",
                        Message = $"Private field '{name}' should use _camelCase",
                        Severity = LintSeverity.Warning,
                        FilePath = span.Path,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }

    private static bool HasStructLayoutAttribute(FieldDeclarationSyntax node)
    {
        if (node.Parent is not TypeDeclarationSyntax typeDecl)
        {
            return false;
        }

        foreach (AttributeListSyntax attrList in typeDecl.AttributeLists)
        {
            foreach (AttributeSyntax attr in attrList.Attributes)
            {
                string attrName = attr.Name.ToString();

                if (attrName is "StructLayout" or "StructLayoutAttribute" or
                    "System.Runtime.InteropServices.StructLayout" or
                    "System.Runtime.InteropServices.StructLayoutAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }
}
