using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class AvoidOutParametersRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1021.severity";

    public string RuleId => "CA1021";

    public string Name => "AvoidOutParameters";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsVisible(SyntaxTokenList modifiers)
    {
        foreach (SyntaxToken modifier in modifiers)
        {
            if (modifier.IsKind(SyntaxKind.PublicKeyword) ||
                modifier.IsKind(SyntaxKind.ProtectedKeyword))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetDiagnosticSeverity(ConfigKey) is not null and not LintSeverity.None;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        List<LintDiagnostic>? diagnostics = null;

        foreach (MethodDeclarationSyntax method in
            context.Root.DescendantNodes()
                .OfType<MethodDeclarationSyntax>())
        {
            if (!IsVisible(method.Modifiers))
            {
                continue;
            }

            // Skip Try* pattern methods — out params are idiomatic there
            if (method.Identifier.Text.StartsWith(
                "Try", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (ParameterSyntax param in method.ParameterList.Parameters)
            {
                foreach (SyntaxToken modifier in param.Modifiers)
                {
                    if (modifier.IsKind(SyntaxKind.OutKeyword))
                    {
                        FileLinePositionSpan span =
                            param.Identifier.GetLocation().GetLineSpan();

                        (diagnostics ??= []).Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message = $"Avoid out parameter "
                                    + $"'{param.Identifier.Text}' on "
                                    + "public method",
                                Severity = DefaultSeverity,
                                FilePath = context.FilePath,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });

                        break;
                    }
                }
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
