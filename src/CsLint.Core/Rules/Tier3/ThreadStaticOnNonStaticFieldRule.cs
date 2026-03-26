using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ThreadStaticOnNonStaticFieldRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA2259.severity";

    public string RuleId => "CA2259";

    public string Name => "EnsureThreadStaticIsOnlyUsedWithStaticFields";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool HasAttribute(SyntaxList<AttributeListSyntax> attributeLists, string name)
    {
        foreach (AttributeListSyntax attrList in attributeLists)
        {
            foreach (AttributeSyntax attr in attrList.Attributes)
            {
                string attrName = attr.Name.ToString();

                if (attrName is "ThreadStatic" or "ThreadStaticAttribute" or
                    "System.ThreadStatic" or "System.ThreadStaticAttribute")
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasModifier(SyntaxTokenList modifiers, SyntaxKind kind)
    {
        foreach (SyntaxToken modifier in modifiers)
        {
            if (modifier.IsKind(kind))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (FieldDeclarationSyntax field in context.Root.DescendantNodes().OfType<FieldDeclarationSyntax>())
        {
            if (!HasAttribute(field.AttributeLists, "ThreadStatic"))
            {
                continue;
            }

            if (HasModifier(field.Modifiers, SyntaxKind.StaticKeyword))
            {
                continue;
            }

            foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
            {
                FileLinePositionSpan span = variable.Identifier.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = RuleId,
                        Message = $"[ThreadStatic] is only effective on static fields; '{variable.Identifier.Text}' is not static",
                        Severity = DefaultSeverity,
                        FilePath = context.FilePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
