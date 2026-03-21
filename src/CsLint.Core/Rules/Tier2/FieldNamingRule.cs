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

    private static void CheckFields(
        FieldDeclarationSyntax node,
        List<LintDiagnostic> diagnostics,
        Func<string, bool> isValidName,
        string ruleId,
        string fieldKind,
        string expectedCase)
    {
        foreach (VariableDeclaratorSyntax variable in node.Declaration.Variables)
        {
            string name = variable.Identifier.ValueText;

            if (!isValidName(name))
            {
                FileLinePositionSpan span = variable.Identifier.GetLocation().GetLineSpan();

                diagnostics.Add(
                    new LintDiagnostic
                    {
                        RuleId = ruleId,
                        Message = $"{fieldKind} '{name}' should use {expectedCase}",
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

    public bool IsEnabled(LintConfiguration configuration) => true;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        var walker = new CombinedNamingWalker([this]);
        walker.Visit(context.Root);
        return walker.Diagnostics;
    }

    void INamingRuleHandler.VisitFieldDeclaration(FieldDeclarationSyntax node, List<LintDiagnostic> diagnostics)
    {
        // Skip const fields (handled by ConstantNamingRule)
        if (node.Modifiers.Any(SyntaxKind.ConstKeyword))
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

        bool isStatic = node.Modifiers.Any(SyntaxKind.StaticKeyword);
        bool isReadOnly = node.Modifiers.Any(SyntaxKind.ReadOnlyKeyword);

        if (isPrivate && !isStatic)
        {
            // Private instance fields: _camelCase (CSLINT104)
            CheckFields(node, diagnostics, NamingHelper.IsUnderscoreCamelCase,
                "CSLINT104", "Private field", "_camelCase");
        }
        else if (!isPrivate && (isReadOnly || isStatic))
        {
            // Non-private readonly or static readonly fields: PascalCase (SA1304/SA1307/SA1311)
            CheckFields(node, diagnostics, NamingHelper.IsPascalCase,
                "CSLINT104", "Accessible field", "PascalCase");
        }
    }
}
