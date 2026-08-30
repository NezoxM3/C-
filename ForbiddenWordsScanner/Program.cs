using Avalonia;
using System;
using System.IO;

namespace ForbiddenWordsScanner;

class Program
{
    private static FileStream? _lockFile;
    private static readonly string LockFilePath =
        Path.Combine(Path.GetTempPath(), "ForbiddenWordsScanner.lock");

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            _lockFile = new FileStream(
                LockFilePath,
                FileMode.OpenOrCreate,
                FileAccess.ReadWrite,
                FileShare.None);
        }
        catch (IOException)
        {
            Console.WriteLine("Застосунок вже запущено. Завершення роботи.");
            return;
        }

        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        finally
        {
            _lockFile.Close();
            _lockFile.Dispose();
            try { File.Delete(LockFilePath); } catch { /* ігноруємо */ }
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}