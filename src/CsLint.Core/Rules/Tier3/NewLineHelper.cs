using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cslint.Core.Rules.Tier3;

internal static class NewLineHelper
{
    public static bool HasBlankLineBetween(SyntaxToken first, SyntaxToken second)
    {
        int newlineCount = 0;

        foreach (SyntaxTrivia trivia in first.TrailingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                newlineCount++;
            }
        }

        foreach (SyntaxTrivia trivia in second.LeadingTrivia)
        {
            if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                newlineCount++;

                if (newlineCount >= 2)
                {
                    return true;
                }
            }
            else if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                newlineCount = 0;
            }
        }

        return false;
    }

    public static bool HasBlankLineAfter(SyntaxToken token)
    {
        SyntaxToken next = token.GetNextToken();

        if (next == default)
        {
            return false;
        }

        return HasBlankLineBetween(token, next);
    }
}
