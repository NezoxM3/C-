using System;
using System.Linq;

namespace TextToolsLibrary;

public class TextProcessor
{
    public int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text
            .Split(
                new[] { ' ', '\t', '\n', '\r' },
                StringSplitOptions.RemoveEmptyEntries)
            .Length;
    }

    public string ToUpperCase(string text)
    {
        return text.ToUpper();
    }

    public bool IsPalindrome(string text)
    {
        string cleanedText = new string(
            text
                .Where(char.IsLetterOrDigit)
                .ToArray()
        ).ToLower();

        string reversedText = new string(
            cleanedText.Reverse().ToArray()
        );

        return cleanedText == reversedText;
    }
}