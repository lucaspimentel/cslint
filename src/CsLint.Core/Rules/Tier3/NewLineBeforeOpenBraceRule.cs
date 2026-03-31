using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cslint.Core.Rules.Tier3;

public sealed class NewLineBeforeOpenBraceRule : IRuleDefinition
{
    private const string ConfigKey = "csharp_new_line_before_open_brace";

    public string RuleId => "CSLINT279";

    public string Name => "NewLineBeforeOpenBrace";

    public IReadOnlyList<string> ConfigKeys { get; } = [ConfigKey];

    public LintSeverity DefaultSeverity => LintSeverity.Warning;

    private static string? GetBraceContext(SyntaxToken openBrace)
    {
        SyntaxNode? parent = openBrace.Parent;

        return parent switch
        {
            AccessorListSyntax => "accessors",
            AnonymousMethodExpressionSyntax => "anonymous_methods",
            AnonymousObjectCreationExpressionSyntax => "anonymous_types",
            BlockSyntax block => GetBlockContext(block),
            InitializerExpressionSyntax => "object_collection_array_initializers",
            SwitchStatementSyntax => "control_blocks",
            ClassDeclarationSyntax or StructDeclarationSyntax
                or InterfaceDeclarationSyntax or EnumDeclarationSyntax
                or RecordDeclarationSyntax => "types",
            NamespaceDeclarationSyntax => "types",
            _ => null,
        };
    }

    private static string? GetBlockContext(BlockSyntax block) =>
        block.Parent switch
        {
            MethodDeclarationSyntax => "methods",
            ConstructorDeclarationSyntax => "methods",
            DestructorDeclarationSyntax => "methods",
            OperatorDeclarationSyntax => "methods",
            ConversionOperatorDeclarationSyntax => "methods",
            LocalFunctionStatementSyntax => "local_functions",
            AccessorDeclarationSyntax => "accessors",
            SimpleLambdaExpressionSyntax or ParenthesizedLambdaExpressionSyntax => "lambdas",
            AnonymousMethodExpressionSyntax => "anonymous_methods",
            IfStatementSyntax or ElseClauseSyntax
                or ForStatementSyntax or ForEachStatementSyntax
                or WhileStatementSyntax or DoStatementSyntax
                or UsingStatementSyntax or LockStatementSyntax
                or TryStatementSyntax or CatchClauseSyntax
                or FinallyClauseSyntax or SwitchSectionSyntax => "control_blocks",
            PropertyDeclarationSyntax => "properties",
            IndexerDeclarationSyntax => "indexers",
            EventDeclarationSyntax => "events",
            _ => null,
        };

    private static bool IsOnSameLineAsPrevious(SyntaxToken token)
    {
        SyntaxToken previous = token.GetPreviousToken();

        if (previous == default)
        {
            return false;
        }

        int prevLine = previous.GetLocation().GetLineSpan().EndLinePosition.Line;
        int currLine = token.GetLocation().GetLineSpan().StartLinePosition.Line;
        return prevLine == currLine;
    }

    private static bool IsOnSameLineAsNext(SyntaxToken token)
    {
        SyntaxToken next = token.GetNextToken();

        if (next == default)
        {
            return false;
        }

        int currLine = token.GetLocation().GetLineSpan().EndLinePosition.Line;
        int nextLine = next.GetLocation().GetLineSpan().StartLinePosition.Line;
        return currLine == nextLine;
    }

    private static HashSet<string>? ParseContexts(string value)
    {
        if (string.Equals(value, "all", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "none", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var contexts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string part in value.Split(','))
        {
            string trimmed = part.Trim();

            if (trimmed.Length > 0)
            {
                contexts.Add(trimmed);
            }
        }

        return contexts.Count > 0 ? contexts : null;
    }

    public bool IsEnabled(LintConfiguration configuration) =>
        configuration.GetValue(ConfigKey) is not null;

    public IReadOnlyList<LintDiagnostic> Analyze(RuleContext context)
    {
        if (!IsEnabled(context.Configuration))
        {
            return [];
        }

        string? value = context.Configuration.GetValue(ConfigKey);

        if (value is null)
        {
            return [];
        }

        HashSet<string>? enabledContexts = ParseContexts(value);
        bool requireAll = string.Equals(value, "all", StringComparison.OrdinalIgnoreCase);
        bool requireNone = string.Equals(value, "none", StringComparison.OrdinalIgnoreCase);

        if (!requireAll && !requireNone && enabledContexts is null)
        {
            return [];
        }

        var diagnostics = new List<LintDiagnostic>();
        SyntaxToken token = context.Root.GetFirstToken();

        while (token != default)
        {
            if (token.IsKind(SyntaxKind.OpenBraceToken))
            {
                string? braceContext = GetBraceContext(token);

                if (braceContext is not null)
                {
                    bool requireNewLine = requireAll
                        || (enabledContexts?.Contains(braceContext) ?? false);

                    bool sameLine = IsOnSameLineAsPrevious(token);

                    if (requireNewLine && sameLine && !IsOnSameLineAsNext(token))
                    {
                        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                        diagnostics.Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message =
                                    $"Place opening brace on a new line ({braceContext})",
                                Severity = LintSeverity.Warning,
                                FilePath = context.FilePath,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });
                    }
                    else if (requireNone && !sameLine)
                    {
                        FileLinePositionSpan span = token.GetLocation().GetLineSpan();

                        diagnostics.Add(
                            new LintDiagnostic
                            {
                                RuleId = RuleId,
                                Message =
                                    $"Place opening brace on the same line ({braceContext})",
                                Severity = LintSeverity.Warning,
                                FilePath = context.FilePath,
                                Line = span.StartLinePosition.Line + 1,
                                Column = span.StartLinePosition.Character + 1,
                            });
                    }
                }
            }

            token = token.GetNextToken();
        }

        return diagnostics;
    }
}
