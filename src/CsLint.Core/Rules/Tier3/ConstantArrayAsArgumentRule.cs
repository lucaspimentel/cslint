using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class ConstantArrayAsArgumentRule : IRuleDefinition
{
    private const string ConfigKey = "dotnet_diagnostic.CA1861.severity";

    public string RuleId => "CA1861";

    public string Name => "AvoidConstantArraysAsArguments";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static bool IsConstantElement(ExpressionSyntax expr) =>
        expr is LiteralExpressionSyntax or
            MemberAccessExpressionSyntax or
            DefaultExpressionSyntax or
            TypeOfExpressionSyntax;

    private static bool IsAllConstantElements(
        InitializerExpressionSyntax initializer)
    {
        foreach (ExpressionSyntax expr in initializer.Expressions)
        {
            if (!IsConstantElement(expr))
            {
                return false;
            }
        }

        return true;
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

        foreach (ArgumentSyntax argument in
            context.Root.DescendantNodes().OfType<ArgumentSyntax>())
        {
            ExpressionSyntax expr = argument.Expression;

            InitializerExpressionSyntax? initializer = null;

            switch (expr)
            {
                // new[] { 1, 2, 3 }
                case ImplicitArrayCreationExpressionSyntax implicitArray:
                    initializer = implicitArray.Initializer;
                    break;

                // new int[] { 1, 2, 3 }
                case ArrayCreationExpressionSyntax arrayCreation
                    when arrayCreation.Initializer is not null:
                    initializer = arrayCreation.Initializer;
                    break;

                default:
                    continue;
            }

            if (initializer is null ||
                initializer.Expressions.Count == 0)
            {
                continue;
            }

            if (!IsAllConstantElements(initializer))
            {
                continue;
            }

            FileLinePositionSpan span = expr.GetLocation().GetLineSpan();

            (diagnostics ??= []).Add(
                new LintDiagnostic
                {
                    RuleId = RuleId,
                    Message = "Avoid allocating a constant array as an "
                        + "argument; extract to a static readonly field",
                    Severity = DefaultSeverity,
                    FilePath = context.FilePath,
                    Line = span.StartLinePosition.Line + 1,
                    Column = span.StartLinePosition.Character + 1,
                });
        }

        return diagnostics ?? (IReadOnlyList<LintDiagnostic>)[];
    }
}
