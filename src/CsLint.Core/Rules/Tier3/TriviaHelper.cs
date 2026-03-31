using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

internal static class TriviaHelper
{
    public static bool HasTrailingSpace(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasLeadingSpace(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasTrailingNewline(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    public static bool HasTrailingSpaceOrNewline(SyntaxToken token)
    {
        foreach (SyntaxTrivia trivia in token.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) || trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsOnSameLineAsPrevious(SyntaxToken token)
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

    public static bool IsOnSameLineAsNext(SyntaxToken token)
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
}
