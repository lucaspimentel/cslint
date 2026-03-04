using Cslint.Core.Config;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Rules;

public sealed class RuleContext
{
    public required string FilePath { get; init; }

    public required SourceText SourceText { get; init; }

    public required SyntaxTree SyntaxTree { get; init; }

    public required CSharpSyntaxNode Root { get; init; }

    public required LintConfiguration Configuration { get; init; }
}
