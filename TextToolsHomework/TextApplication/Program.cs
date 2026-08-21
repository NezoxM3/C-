using TextToolsLibrary;

class Program
{
    static void Main()
    {
        TextProcessor processor = new TextProcessor();

        Console.Write("Enter text: ");
        string text = Console.ReadLine() ?? "";

        int wordCount = processor.CountWords(text);
        string upperText = processor.ToUpperCase(text);
        bool palindrome = processor.IsPalindrome(text);

        Console.WriteLine();
        Console.WriteLine($"Word count: {wordCount}");
        Console.WriteLine($"Upper case: {upperText}");
        Console.WriteLine($"Palindrome: {palindrome}");
    }
}