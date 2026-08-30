using System.Collections.Generic;

namespace ForbiddenWordsScanner.Models;

public class ScanResult
{
    public string OriginalFilePath { get; set; } = "";
    public string CopiedOriginalPath { get; set; } = "";
    public string CleanedFilePath { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public int TotalReplacements { get; set; }

    public Dictionary<string, int> WordOccurrences { get; set; } = new();
}