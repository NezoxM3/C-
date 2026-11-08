using System.Net;
using System.Text.RegularExpressions;

const string startUrl = "https://uk.wikipedia.org/wiki/C_Sharp";

string outputDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    "pages"
);

Directory.CreateDirectory(outputDirectory);

using HttpClient httpClient = new HttpClient();

httpClient.Timeout = TimeSpan.FromSeconds(60);
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
    "WikipediaDownloader/1.0 (educational project)"
);

Uri baseUri = new Uri(startUrl);

Console.WriteLine("Downloading the main Wikipedia page...");

string mainHtml;

try
{
    mainHtml = await httpClient.GetStringAsync(startUrl);

    string mainFileName = GetFileName(baseUri);
    string mainFilePath = Path.Combine(outputDirectory, mainFileName);

    await File.WriteAllTextAsync(mainFilePath, mainHtml);

    Console.WriteLine($"Main page saved: {mainFileName}");
}
catch (Exception ex)
{
    Console.WriteLine($"Could not download the main page: {ex.Message}");
    return;
}

Regex linkRegex = new Regex(
    @"<a\b[^>]*?\bhref\s*=\s*(?:""(?<url>[^""]*)""|'(?<url>[^']*)'|(?<url>[^\s>]+))",
    RegexOptions.IgnoreCase | RegexOptions.Singleline
);

HashSet<string> links = new HashSet<string>(
    StringComparer.OrdinalIgnoreCase
);

foreach (Match match in linkRegex.Matches(mainHtml))
{
    string rawUrl = WebUtility.HtmlDecode(match.Groups["url"].Value);

    if (string.IsNullOrWhiteSpace(rawUrl))
    {
        continue;
    }

    if (!Uri.TryCreate(baseUri, rawUrl, out Uri? uri))
    {
        continue;
    }

    if (uri.Host != "uk.wikipedia.org")
    {
        continue;
    }

    if (uri.Scheme != Uri.UriSchemeHttp &&
        uri.Scheme != Uri.UriSchemeHttps)
    {
        continue;
    }

    if (!uri.AbsolutePath.StartsWith("/wiki/", StringComparison.OrdinalIgnoreCase))
    {
        continue;
    }

    UriBuilder builder = new UriBuilder(uri)
    {
        Fragment = ""
    };

    links.Add(builder.Uri.AbsoluteUri);
}

Console.WriteLine($"Found {links.Count} unique Wikipedia page links.");
Console.WriteLine("Starting parallel downloads...");

using SemaphoreSlim semaphore = new SemaphoreSlim(8);

int successfulDownloads = 0;
int failedDownloads = 0;

List<Task> downloadTasks = new List<Task>();

foreach (string link in links)
{
    downloadTasks.Add(DownloadPageAsync(link));
}

await Task.WhenAll(downloadTasks);

Console.WriteLine();
Console.WriteLine("Download completed.");
Console.WriteLine($"Successfully downloaded: {successfulDownloads}");
Console.WriteLine($"Failed downloads: {failedDownloads}");
Console.WriteLine($"Files are saved in: {outputDirectory}");


async Task DownloadPageAsync(string url)
{
    await semaphore.WaitAsync();

    try
    {
        Uri uri = new Uri(url);

        string fileName = GetFileName(uri);
        string filePath = Path.Combine(outputDirectory, fileName);

        byte[] content = await httpClient.GetByteArrayAsync(url);

        await File.WriteAllBytesAsync(filePath, content);

        Interlocked.Increment(ref successfulDownloads);

        Console.WriteLine($"Downloaded: {fileName}");
    }
    catch (Exception ex)
    {
        Interlocked.Increment(ref failedDownloads);

        Console.WriteLine($"Failed: {url}");
        Console.WriteLine($"Reason: {ex.Message}");
    }
    finally
    {
        semaphore.Release();
    }
}


string GetFileName(Uri uri) {

    string fileName = uri.Host + uri.AbsolutePath;

    fileName = fileName.Replace('/', '_');

    if (!string.IsNullOrEmpty(uri.Query))
    {
        fileName += "_" + uri.Query.TrimStart('?');
    }

    foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
    {
        fileName = fileName.Replace(invalidCharacter, '_');
    }

    fileName = fileName
        .Replace(':', '_')
        .Replace('?', '_')
        .Replace('&', '_')
        .Replace('=', '_');

    return fileName + ".html";
}