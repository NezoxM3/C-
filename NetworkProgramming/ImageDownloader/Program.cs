using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

const string websiteUrl = "https://sithai.choiceqr.com/";

string outputDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    "images"
);

Directory.CreateDirectory(outputDirectory);

using HttpClient httpClient = new HttpClient();

httpClient.Timeout = TimeSpan.FromSeconds(60);
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
    "ImageDownloader/1.0 (educational project)"
);

Uri baseUri = new Uri(websiteUrl);

Console.WriteLine("Downloading website HTML...");

string html;

try
{
    html = await httpClient.GetStringAsync(websiteUrl);
}
catch (Exception ex)
{
    Console.WriteLine($"Could not download website: {ex.Message}");
    return;
}

HashSet<string> imageUrls = new HashSet<string>(
    StringComparer.OrdinalIgnoreCase
);

HashSet<string> cssUrls = new HashSet<string>(
    StringComparer.OrdinalIgnoreCase
);

Regex imageTagRegex = new Regex(
    @"<(?:img|source)\b[^>]*>",
    RegexOptions.IgnoreCase
);

foreach (Match tagMatch in imageTagRegex.Matches(html))
{
    string tag = tagMatch.Value;

    Regex attributeRegex = new Regex(
        @"\b(?:src|data-src|data-original|data-lazy-src|srcset|data-srcset)\s*=\s*(?:""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^\s>]+))",
        RegexOptions.IgnoreCase
    );

    foreach (Match attributeMatch in attributeRegex.Matches(tag))
    {
        string value = WebUtility.HtmlDecode(
            attributeMatch.Groups["value"].Value
        );

        if (value.Contains(",") &&
            (tag.Contains("srcset", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (string candidate in value.Split(','))
            {
                string candidateUrl = candidate.Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];

                AddImageUrl(candidateUrl);
            }
        }
        else
        {
            AddImageUrl(value);
        }
    }
}

Regex metaRegex = new Regex(
    @"<meta\b[^>]*>",
    RegexOptions.IgnoreCase
);

foreach (Match match in metaRegex.Matches(html))
{
    string tag = match.Value;

    if (tag.Contains("og:image", StringComparison.OrdinalIgnoreCase))
    {
        string? content = GetAttribute(tag, "content");

        if (content != null)
        {
            AddImageUrl(content);
        }
    }
}

Regex cssUrlRegex = new Regex(
    @"url\(\s*['""]?(?<url>[^'"")]+)['""]?\s*\)",
    RegexOptions.IgnoreCase
);

foreach (Match match in cssUrlRegex.Matches(html))
{
    AddImageUrl(match.Groups["url"].Value);
}

Regex styleBlockRegex = new Regex(
    @"<style\b[^>]*>(?<css>.*?)</style>",
    RegexOptions.IgnoreCase | RegexOptions.Singleline
);

foreach (Match match in styleBlockRegex.Matches(html))
{
    string css = match.Groups["css"].Value;

    foreach (Match cssMatch in cssUrlRegex.Matches(css))
    {
        AddImageUrl(cssMatch.Groups["url"].Value);
    }
}

Regex linkTagRegex = new Regex(
    @"<link\b[^>]*>",
    RegexOptions.IgnoreCase
);

foreach (Match match in linkTagRegex.Matches(html))
{
    string tag = match.Value;

    string? rel = GetAttribute(tag, "rel");
    string? href = GetAttribute(tag, "href");

    if (rel != null &&
        rel.Contains("stylesheet", StringComparison.OrdinalIgnoreCase) &&
        href != null)
    {
        AddCssUrl(href);
    }

    if (rel != null &&
        rel.Contains("preload", StringComparison.OrdinalIgnoreCase) &&
        tag.Contains("image", StringComparison.OrdinalIgnoreCase) &&
        href != null)
    {
        AddImageUrl(href);
    }
}

Console.WriteLine($"Found {cssUrls.Count} CSS files.");
Console.WriteLine("Downloading CSS files...");

foreach (string cssUrl in cssUrls)
{
    try
    {
        string css = await httpClient.GetStringAsync(cssUrl);

        foreach (Match match in cssUrlRegex.Matches(css))
        {
            AddImageUrl(match.Groups["url"].Value, new Uri(cssUrl));
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not download CSS: {cssUrl}");
        Console.WriteLine(ex.Message);
    }
}

Console.WriteLine();
Console.WriteLine($"Found {imageUrls.Count} unique image URLs.");
Console.WriteLine("Starting image downloads...");

using SemaphoreSlim semaphore = new SemaphoreSlim(6);

int downloadedCount = 0;
int skippedCount = 0;
int failedCount = 0;

List<Task> downloadTasks = new List<Task>();

foreach (string imageUrl in imageUrls)
{
    downloadTasks.Add(DownloadImageAsync(imageUrl));
}

await Task.WhenAll(downloadTasks);

Console.WriteLine();
Console.WriteLine("Download completed!");
Console.WriteLine($"Downloaded: {downloadedCount}");
Console.WriteLine($"Skipped: {skippedCount}");
Console.WriteLine($"Failed: {failedCount}");
Console.WriteLine($"Folder: {outputDirectory}");


void AddImageUrl(string rawUrl, Uri? sourceUri = null)
{
    rawUrl = WebUtility.HtmlDecode(rawUrl.Trim());

    if (string.IsNullOrWhiteSpace(rawUrl) ||
        rawUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
        rawUrl.StartsWith("blob:", StringComparison.OrdinalIgnoreCase) ||
        rawUrl.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
        rawUrl.StartsWith("#"))
    {
        return;
    }

    Uri referenceUri = sourceUri ?? baseUri;

    if (!Uri.TryCreate(referenceUri, rawUrl, out Uri? uri))
    {
        return;
    }

    if (uri.Scheme != Uri.UriSchemeHttp &&
        uri.Scheme != Uri.UriSchemeHttps)
    {
        return;
    }

    UriBuilder builder = new UriBuilder(uri)
    {
        Fragment = ""
    };

    imageUrls.Add(builder.Uri.AbsoluteUri);
}


void AddCssUrl(string rawUrl)
{
    rawUrl = WebUtility.HtmlDecode(rawUrl.Trim());

    if (!Uri.TryCreate(baseUri, rawUrl, out Uri? uri))
    {
        return;
    }

    if (uri.Scheme != Uri.UriSchemeHttp &&
        uri.Scheme != Uri.UriSchemeHttps)
    {
        return;
    }

    cssUrls.Add(uri.AbsoluteUri);
}


string? GetAttribute(string tag, string attributeName)
{
    Regex regex = new Regex(
        $@"\b{Regex.Escape(attributeName)}\s*=\s*(?:""(?<value>[^""]*)""|'(?<value>[^']*)'|(?<value>[^\s>]+))",
        RegexOptions.IgnoreCase
    );

    Match match = regex.Match(tag);

    if (!match.Success)
    {
        return null;
    }

    return WebUtility.HtmlDecode(match.Groups["value"].Value);
}


async Task DownloadImageAsync(string url)
{
    await semaphore.WaitAsync();

    try
    {
        Uri uri = new Uri(url);

        using HttpResponseMessage response =
            await httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        string? mediaType = response.Content.Headers.ContentType?.MediaType;

        if (mediaType == null ||
            !mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            Interlocked.Increment(ref skippedCount);
            return;
        }

        byte[] imageData = await response.Content.ReadAsByteArrayAsync();

        if (imageData.Length == 0)
        {
            Interlocked.Increment(ref skippedCount);
            return;
        }

        string fileName = GetImageFileName(uri, mediaType);
        string filePath = Path.Combine(outputDirectory, fileName);

        if (File.Exists(filePath))
        {
            Interlocked.Increment(ref skippedCount);
            return;
        }

        await File.WriteAllBytesAsync(filePath, imageData);

        Interlocked.Increment(ref downloadedCount);

        Console.WriteLine($"Downloaded: {fileName}");
    }
    catch (Exception ex)
    {
        Interlocked.Increment(ref failedCount);

        Console.WriteLine($"Failed: {url}");
        Console.WriteLine($"Reason: {ex.Message}");
    }
    finally
    {
        semaphore.Release();
    }
}


string GetImageFileName(Uri uri, string mediaType)
{
    string originalName = Path.GetFileName(uri.AbsolutePath);

    if (string.IsNullOrWhiteSpace(originalName))
    {
        originalName = "image";
    }

    originalName = Uri.UnescapeDataString(originalName);

    string extension = Path.GetExtension(originalName);

    if (string.IsNullOrWhiteSpace(extension))
    {
        extension = mediaType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            "image/webp" => ".webp",
            "image/svg+xml" => ".svg",
            "image/avif" => ".avif",
            "image/bmp" => ".bmp",
            "image/x-icon" => ".ico",
            _ => ".img"
        };

        originalName += extension;
    }

    string nameWithoutExtension =
        Path.GetFileNameWithoutExtension(originalName);

    byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(uri.AbsoluteUri));

    string shortHash = Convert.ToHexString(hash)[..10];

    string safeName = $"{nameWithoutExtension}_{shortHash}{extension}";

    foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
    {
        safeName = safeName.Replace(invalidCharacter, '_');
    }

    return safeName;
}