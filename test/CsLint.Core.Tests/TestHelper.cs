using Cslint.Core.Config;
using Cslint.Core.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Cslint.Core.Tests;

internal static class TestHelper
{
    public static RuleContext CreateContext(
        string source,
        LintConfiguration? configuration = null,
        string filePath = "test.cs")
    {
        SourceText sourceText = SourceText.From(source);
        SyntaxTree tree = CSharpSyntaxTree.ParseText(sourceText, path: filePath);

        return new RuleContext
        {
            FilePath = filePath,
            SourceText = sourceText,
            SyntaxTree = tree,
            Root = (CSharpSyntaxNode)tree.GetRoot(),
            Configuration = configuration ?? LintConfiguration.Empty,
        };
    }
}
