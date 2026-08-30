using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ForbiddenWordsScanner.Services;

public class WordMatcher
{
    private readonly Regex _regex;

    public WordMatcher(IEnumerable<string> forbiddenWords)
    {
        var escapedWords = new List<string>();
        foreach (var word in forbiddenWords)
        {
            var trimmed = word.Trim();
            if (trimmed.Length > 0)
                escapedWords.Add(Regex.Escape(trimmed));
        }


        string pattern = @"\b(" + string.Join("|", escapedWords) + @")\b";

        _regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }

    public (string CleanedText, Dictionary<string, int> Occurrences) FindAndReplace(string text)
    {
        var occurrences = new Dictionary<string, int>();

        string cleaned = _regex.Replace(text, match =>
        {
            string foundWord = match.Value.ToLowerInvariant();

            if (occurrences.ContainsKey(foundWord))
                occurrences[foundWord]++;
            else
                occurrences[foundWord] = 1;

            return "*******";
        });

        return (cleaned, occurrences);
    }

    public bool HasMatch(string text) => _regex.IsMatch(text);
}