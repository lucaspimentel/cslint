using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

/// <summary>
/// IDE0060: Remove unused parameter.
/// Flags method parameters that are not referenced in the method body.
/// Supports "all" (flag all unused) and "non_virtual" (skip virtual/override/abstract) modes.
/// </summary>
public sealed class UnusedParameterRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_code_quality_unused_parameters";

    public string RuleId => "IDE0060";

    public string Name => "UnusedParameter";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Info;

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        (string? pref, string? _) = context.Configuration.GetValueWithSeverity(ConfigKey);

        if (!string.Equals(pref, "all", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(pref, "non_virtual", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        bool flagAll = string.Equals(pref, "all", StringComparison.OrdinalIgnoreCase);

        List<LintDiagnostic>? diagnostics = null;

        foreach (SyntaxNode node in context.Root.DescendantNodes())
        {
            switch (node)
            {
                case MethodDeclarationSyntax method:
                    CheckMethod(method, flagAll, context.FilePath, ref diagnostics);
                    break;

                case ConstructorDeclarationSyntax ctor:
                    CheckConstructor(ctor, context.FilePath, ref diagnostics);
                    break;

                case LocalFunctionStatementSyntax localFunc:
                    CheckLocalFunction(localFunc, context.FilePath, ref diagnostics);
                    break;
            }
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }

    private static void CheckMethod(
        MethodDeclarationSyntax method,
        bool flagAll,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        SyntaxNode? body = (SyntaxNode?)method.Body ?? method.ExpressionBody;

        if (body is null)
        {
            return;
        }

        // Skip interface members
        if (method.Parent is InterfaceDeclarationSyntax)
        {
            return;
        }

        // In non_virtual mode, skip virtual/override/abstract methods
        if (!flagAll)
        {
            SyntaxTokenList modifiers = method.Modifiers;

            if (modifiers.Any(SyntaxKind.VirtualKeyword) ||
                modifiers.Any(SyntaxKind.OverrideKeyword) ||
                modifiers.Any(SyntaxKind.AbstractKeyword))
            {
                return;
            }
        }

        // Skip Main entry points
        if (method.Identifier.Text is "Main")
        {
            return;
        }

        CheckParameters(method.ParameterList, body, filePath, ref diagnostics);
    }

    private static void CheckConstructor(
        ConstructorDeclarationSyntax ctor,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        SyntaxNode? body = (SyntaxNode?)ctor.Body ?? ctor.ExpressionBody;

        if (body is null)
        {
            return;
        }

        // For constructors with a `: this(...)` or `: base(...)` initializer,
        // parameters might be passed to the initializer — check that too
        CheckParameters(ctor.ParameterList, body, filePath, ref diagnostics, ctor.Initializer);
    }

    private static void CheckLocalFunction(
        LocalFunctionStatementSyntax localFunc,
        string filePath,
        ref List<LintDiagnostic>? diagnostics)
    {
        SyntaxNode? body = (SyntaxNode?)localFunc.Body ?? localFunc.ExpressionBody;

        if (body is null)
        {
            return;
        }

        CheckParameters(localFunc.ParameterList, body, filePath, ref diagnostics);
    }

    private static void CheckParameters(
        ParameterListSyntax? parameterList,
        SyntaxNode body,
        string filePath,
        ref List<LintDiagnostic>? diagnostics,
        ConstructorInitializerSyntax? ctorInitializer = null)
    {
        if (parameterList is null || parameterList.Parameters.Count == 0)
        {
            return;
        }

        // Collect parameter names (skip discards)
        var paramNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (ParameterSyntax param in parameterList.Parameters)
        {
            string name = param.Identifier.Text;

            if (name is not "_")
            {
                paramNames.Add(name);
            }
        }

        if (paramNames.Count == 0)
        {
            return;
        }

        // Scan body for identifier references
        foreach (SyntaxNode descendant in body.DescendantNodes())
        {
            if (descendant is IdentifierNameSyntax id && paramNames.Contains(id.Identifier.Text))
            {
                paramNames.Remove(id.Identifier.Text);

                if (paramNames.Count == 0)
                {
                    return;
                }
            }
        }

        // Also check constructor initializer (`: this(param)` or `: base(param)`)
        if (ctorInitializer is not null)
        {
            foreach (SyntaxNode descendant in ctorInitializer.DescendantNodes())
            {
                if (descendant is IdentifierNameSyntax id && paramNames.Contains(id.Identifier.Text))
                {
                    paramNames.Remove(id.Identifier.Text);

                    if (paramNames.Count == 0)
                    {
                        return;
                    }
                }
            }
        }

        // Remaining params are unused — flag them
        foreach (ParameterSyntax param in parameterList.Parameters)
        {
            if (paramNames.Contains(param.Identifier.Text))
            {
                FileLinePositionSpan span = param.Identifier.GetLocation().GetLineSpan();

                (diagnostics ??= []).Add(
                    new LintDiagnostic
                    {
                        RuleId = "IDE0060",
                        Message = $"Parameter '{param.Identifier.Text}' is unused",
                        Severity = LintSeverity.Info,
                        FilePath = filePath,
                        Line = span.StartLinePosition.Line + 1,
                        Column = span.StartLinePosition.Character + 1,
                    });
            }
        }
    }
}
