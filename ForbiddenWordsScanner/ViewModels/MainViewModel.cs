using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ForbiddenWordsScanner.Models;
using ForbiddenWordsScanner.Services;

namespace ForbiddenWordsScanner.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly Window _window;

    public MainViewModel(Window window)
    {
        _window = window;
    }

    [ObservableProperty]
    public partial string ForbiddenWordsRaw { get; set; } = "";

    [ObservableProperty]
    public partial string DestinationFolder { get; set; } = "";

    [ObservableProperty]
    public partial string StatusText { get; set; } = "Готово до роботи";

    [ObservableProperty]
    public partial string ProgressText { get; set; } = "0 / 0 файлів";

    [ObservableProperty]
    public partial double ProgressPercent { get; set; } = 0;

    [ObservableProperty]
    public partial string PauseButtonText { get; set; } = "Пауза";

    public ObservableCollection<ScanResult> Results { get; } = new();

    private CancellationTokenSource? _cts;
    private readonly ManualResetEventSlim _pauseEvent = new ManualResetEventSlim(true);
    private bool _isRunning = false;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private async Task Start()
    {
        if (_isRunning) return;

        _isRunning = true;
        StartCommand.NotifyCanExecuteChanged();
        PauseResumeCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();

        _cts = new CancellationTokenSource();
        _pauseEvent.Set();
        PauseButtonText = "Пауза";
        Results.Clear();
        ProgressPercent = 0;
        ProgressText = "0 / 0 файлів";

        try
        {
            await Task.Run(() => RunScan(_cts.Token));
        }
        catch (OperationCanceledException)
        {
            StatusText = "Сканування зупинено користувачем.";
        }
        finally
        {
            _isRunning = false;
            StartCommand.NotifyCanExecuteChanged();
            PauseResumeCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanStart() => !_isRunning;

    [RelayCommand(CanExecute = nameof(CanPauseOrStop))]
    private void PauseResume()
    {
        if (!_isRunning) return;

        if (_pauseEvent.IsSet)
        {
            _pauseEvent.Reset();
            PauseButtonText = "Відновити";
            StatusText = "На паузі.";
        }
        else
        {
            _pauseEvent.Set();
            PauseButtonText = "Пауза";
            StatusText = "Сканування відновлено...";
        }
    }

    [RelayCommand(CanExecute = nameof(CanPauseOrStop))]
    private void Stop()
    {
        if (!_isRunning) return;

        _cts?.Cancel();
        _pauseEvent.Set();
    }

    private bool CanPauseOrStop() => _isRunning;

    [RelayCommand]
    private async Task LoadWordsFromFile()
    {
        var files = await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Оберіть файл із забороненими словами",
            AllowMultiple = false,
            FileTypeFilter = new[] { FilePickerFileTypes.TextPlain }
        });

        if (files.Count == 0) return;

        await using var stream = await files[0].OpenReadAsync();
        using var reader = new StreamReader(stream);
        ForbiddenWordsRaw = await reader.ReadToEndAsync();
    }

    [RelayCommand]
    private async Task ChooseDestinationFolder()
    {
        var folders = await _window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Оберіть папку призначення",
            AllowMultiple = false
        });

        if (folders.Count == 0) return;

        DestinationFolder = folders[0].Path.LocalPath;
    }

    [RelayCommand]
    private void OpenReportFolder()
    {
        if (string.IsNullOrWhiteSpace(DestinationFolder) || !Directory.Exists(DestinationFolder))
        {
            StatusText = "Папка призначення ще не обрана або не існує.";
            return;
        }

        System.Diagnostics.Process.Start("open", DestinationFolder);
    }


    //                      ОСНОВНА ЛОГІКА СКАНУВАННЯ


    private void RunScan(CancellationToken token)
    {
        var words = ForbiddenWordsRaw
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(w => w.Trim())
            .Where(w => w.Length > 0)
            .ToList();

        if (words.Count == 0)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusText = "Спочатку введіть заборонені слова.");
            return;
        }

        if (string.IsNullOrWhiteSpace(DestinationFolder) || !Directory.Exists(DestinationFolder))
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusText = "Оберіть папку призначення.");
            return;
        }

        var matcher = new WordMatcher(words);

        string originalsDir = Path.Combine(DestinationFolder, "originals");
        string cleanedDir = Path.Combine(DestinationFolder, "cleaned");
        Directory.CreateDirectory(originalsDir);
        Directory.CreateDirectory(cleanedDir);

        Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusText = "Пошук файлів на накопичувачах...");

        var allFiles = new List<string>();
        foreach (var root in GetRootDirectoriesToScan())
        {
            token.ThrowIfCancellationRequested();
            allFiles.AddRange(EnumerateFilesSafe(root, token));
        }

        int totalFiles = allFiles.Count;
        if (totalFiles == 0)
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusText = "Файли (.txt/.md) не знайдено.");
            return;
        }

        int processedCount = 0;
        var globalWordCounts = new ConcurrentDictionary<string, int>();
        var foundResults = new ConcurrentBag<ScanResult>();

        Avalonia.Threading.Dispatcher.UIThread.Post(() => StatusText = $"Сканування {totalFiles} файлів...");

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = token,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };

        Parallel.ForEach(allFiles, parallelOptions, filePath =>
        {
            _pauseEvent.Wait(token);
            token.ThrowIfCancellationRequested();

            try
            {
                ProcessSingleFile(filePath, matcher, originalsDir, cleanedDir, globalWordCounts, foundResults);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            int current = Interlocked.Increment(ref processedCount);

            if (current % 5 == 0 || current == totalFiles)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    ProgressPercent = current * 100.0 / totalFiles;
                    ProgressText = $"{current} / {totalFiles} файлів";
                });
            }
        });

        var resultsList = foundResults.ToList();
        WriteReport(resultsList, globalWordCounts, totalFiles);

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            foreach (var r in resultsList.OrderByDescending(r => r.TotalReplacements))
                Results.Add(r);

            StatusText = $"Завершено. Перевірено {totalFiles} файлів, знайдено {resultsList.Count} із порушеннями.";
        });
    }

    private void ProcessSingleFile(
        string filePath,
        WordMatcher matcher,
        string originalsDir,
        string cleanedDir,
        ConcurrentDictionary<string, int> globalWordCounts,
        ConcurrentBag<ScanResult> foundResults)
    {
        string content = File.ReadAllText(filePath);

        if (!matcher.HasMatch(content))
            return;

        var (cleanedText, occurrences) = matcher.FindAndReplace(content);

        foreach (var kvp in occurrences)
        {
            globalWordCounts.AddOrUpdate(kvp.Key, kvp.Value, (_, oldValue) => oldValue + kvp.Value);
        }

        string fileName = Path.GetFileName(filePath);
        string uniquePrefix = Guid.NewGuid().ToString("N").Substring(0, 8);

        string originalCopyPath = Path.Combine(originalsDir, $"{uniquePrefix}_{fileName}");
        string cleanedCopyPath = Path.Combine(cleanedDir, $"{uniquePrefix}_{fileName}");

        File.Copy(filePath, originalCopyPath, overwrite: true);
        File.WriteAllText(cleanedCopyPath, cleanedText);

        var fileInfo = new FileInfo(filePath);

        foundResults.Add(new ScanResult
        {
            OriginalFilePath = filePath,
            CopiedOriginalPath = originalCopyPath,
            CleanedFilePath = cleanedCopyPath,
            FileSizeBytes = fileInfo.Length,
            TotalReplacements = occurrences.Values.Sum(),
            WordOccurrences = occurrences
        });
    }

    private void WriteReport(List<ScanResult> results, ConcurrentDictionary<string, int> globalWordCounts, int totalFilesScanned)
    {
        var sb = new StringBuilder();

        sb.AppendLine("========================================");
        sb.AppendLine(" ЗВІТ ПРО СКАНУВАННЯ ЗАБОРОНЕНИХ СЛІВ");
        sb.AppendLine("========================================");
        sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Усього перевірено файлів: {totalFilesScanned}");
        sb.AppendLine($"Файлів із забороненими словами: {results.Count}");
        sb.AppendLine($"Загальна кількість замін: {results.Sum(r => r.TotalReplacements)}");
        sb.AppendLine();

        sb.AppendLine("---------- ТОП-10 НАЙПОПУЛЯРНІШИХ СЛІВ ----------");
        var top10 = globalWordCounts.OrderByDescending(kvp => kvp.Value).Take(10);
        int rank = 1;
        foreach (var kvp in top10)
        {
            sb.AppendLine($"{rank}. \"{kvp.Key}\" — {kvp.Value} раз(ів)");
            rank++;
        }
        sb.AppendLine();

        sb.AppendLine("---------- ДЕТАЛІ ПО ФАЙЛАХ ----------");
        foreach (var r in results.OrderByDescending(r => r.TotalReplacements))
        {
            sb.AppendLine($"Файл: {r.OriginalFilePath}");
            sb.AppendLine($"  Розмір: {r.FileSizeBytes} байт");
            sb.AppendLine($"  Кількість замін: {r.TotalReplacements}");
            sb.AppendLine($"  Копія оригіналу: {r.CopiedOriginalPath}");
            sb.AppendLine($"  Очищена версія: {r.CleanedFilePath}");
            sb.AppendLine($"  Знайдені слова:");
            foreach (var kvp in r.WordOccurrences.OrderByDescending(x => x.Value))
            {
                sb.AppendLine($"    - \"{kvp.Key}\": {kvp.Value} раз(ів)");
            }
            sb.AppendLine();
        }

        string reportPath = Path.Combine(DestinationFolder, "report.txt");
        File.WriteAllText(reportPath, sb.ToString());
    }

    private List<string> GetRootDirectoriesToScan()
    {
        var roots = new List<string>();

        string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (Directory.Exists(home))
            roots.Add(home);

        const string volumesPath = "/Volumes";
        if (Directory.Exists(volumesPath))
        {
            foreach (var volume in Directory.GetDirectories(volumesPath))
                roots.Add(volume);
        }

        return roots;
    }

    private IEnumerable<string> EnumerateFilesSafe(string rootDir, CancellationToken token)
    {
        var dirsToVisit = new Stack<string>();
        dirsToVisit.Push(rootDir);

        while (dirsToVisit.Count > 0)
        {
            token.ThrowIfCancellationRequested();
            string currentDir = dirsToVisit.Pop();

            string[] subDirs;
            try
            {
                subDirs = Directory.GetDirectories(currentDir);
            }
            catch (UnauthorizedAccessException) { continue; }
            catch (IOException) { continue; }

            foreach (var dir in subDirs)
                dirsToVisit.Push(dir);

            string[] files;
            try
            {
                files = Directory.GetFiles(currentDir);
            }
            catch (UnauthorizedAccessException) { continue; }
            catch (IOException) { continue; }

            foreach (var file in files)
            {
                if (file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
                {
                    yield return file;
                }
            }
        }
    }
}