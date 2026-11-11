using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using MimeKit;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

Console.OutputEncoding = System.Text.Encoding.UTF8;

const string imapServer = "imap.gmail.com";
const int imapPort = 993;

string downloadsDirectory = Path.Combine(
    Directory.GetCurrentDirectory(),
    "downloads"
);

string stateFile = Path.Combine(
    Directory.GetCurrentDirectory(),
    "monitor-state.txt"
);

string downloadedUrlsFile = Path.Combine(
    Directory.GetCurrentDirectory(),
    "downloaded-urls.txt"
);

Directory.CreateDirectory(downloadsDirectory);

Console.WriteLine("         EMAIL MONITOR");

Console.Write("Gmail address: ");
string email = Console.ReadLine() ?? "";

Console.Write("App Password: ");
string password = ReadPassword().Replace(" ", "");

if (string.IsNullOrWhiteSpace(email) ||
    string.IsNullOrWhiteSpace(password))
{
    Console.WriteLine("Email and password are required.");
    return;
}

HashSet<string> downloadedUrls = new HashSet<string>(
    StringComparer.OrdinalIgnoreCase
);

if (File.Exists(downloadedUrlsFile))
{
    foreach (string url in await File.ReadAllLinesAsync(downloadedUrlsFile))
    {
        if (!string.IsNullOrWhiteSpace(url))
        {
            downloadedUrls.Add(url);
        }
    }
}

uint lastUid = 0;
uint savedUidValidity = 0;
bool hasSavedState = false;

if (File.Exists(stateFile))
{
    string[] stateParts = (await File.ReadAllTextAsync(stateFile))
        .Split('|');

    if (stateParts.Length == 2 &&
        uint.TryParse(stateParts[0], out savedUidValidity) &&
        uint.TryParse(stateParts[1], out lastUid))
    {
        hasSavedState = true;
    }
}

using HttpClient httpClient = new HttpClient();

httpClient.Timeout = Timeout.InfiniteTimeSpan;
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
    "EmailMonitor/1.0 (educational project)"
);

using CancellationTokenSource cancellation = new CancellationTokenSource();

Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cancellation.Cancel();
};

Console.WriteLine();
Console.WriteLine("Email monitoring started.");
Console.WriteLine("Checking interval: 30 seconds.");
Console.WriteLine("Press Ctrl+C to stop.");
Console.WriteLine();

try
{
    while (!cancellation.Token.IsCancellationRequested)
    {
        try
        {
            await CheckMailboxAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Monitoring error: {ex.Message}");
        }

        Console.WriteLine();
        Console.WriteLine($"Next check in 30 seconds...");
        Console.WriteLine();

        try
        {
            await Task.Delay(
                TimeSpan.FromSeconds(30),
                cancellation.Token
            );
        }
        catch (OperationCanceledException)
        {
            break;
        }
    }
}
finally
{
    Console.WriteLine();
    Console.WriteLine("Email monitoring stopped.");
}


async Task CheckMailboxAsync(CancellationToken cancellationToken)
{
    using var imapClient = new ImapClient();

    await imapClient.ConnectAsync(
        imapServer,
        imapPort,
        SecureSocketOptions.SslOnConnect,
        cancellationToken
    );

    await imapClient.AuthenticateAsync(
        email,
        password,
        cancellationToken
    );

    IMailFolder inbox = imapClient.Inbox;

    await inbox.OpenAsync(
        FolderAccess.ReadOnly,
        cancellationToken
    );

    IList<UniqueId> allUids = await inbox.SearchAsync(
        SearchQuery.All,
        cancellationToken
    );

    uint currentUidValidity = inbox.UidValidity;

    if (!hasSavedState ||
        savedUidValidity != currentUidValidity)
    {
        lastUid = allUids.Count > 0
            ? allUids.Max(uid => uid.Id)
            : 0;

        savedUidValidity = currentUidValidity;
        hasSavedState = true;

        await SaveStateAsync();

        Console.WriteLine(
            $"Initial mailbox position saved. Current messages: {allUids.Count}"
        );

        await imapClient.DisconnectAsync(true, cancellationToken);
        return;
    }

    List<UniqueId> newUids = allUids
        .Where(uid => uid.Id > lastUid)
        .OrderBy(uid => uid.Id)
        .ToList();

    if (newUids.Count == 0)
    {
        Console.WriteLine(
            $"[{DateTime.Now:T}] No new messages."
        );

        await imapClient.DisconnectAsync(true, cancellationToken);
        return;
    }

    Console.WriteLine(
        $"[{DateTime.Now:T}] Found {newUids.Count} new message(s)."
    );

    foreach (UniqueId uid in newUids)
    {
        cancellationToken.ThrowIfCancellationRequested();

        MimeMessage message = await inbox.GetMessageAsync(
            uid,
            cancellationToken
        );

        Console.WriteLine();
        Console.WriteLine("NEW EMAIL");

        Console.WriteLine($"From: {message.From}");
        Console.WriteLine($"Subject: {message.Subject}");
        Console.WriteLine($"Date: {message.Date.LocalDateTime}");

        string messageText = string.Join(
            "\n",
            new[] { message.TextBody, message.HtmlBody }
                .Where(body => !string.IsNullOrWhiteSpace(body))
        );

        List<string> links = ExtractLinks(messageText);

        if (links.Count == 0)
        {
            Console.WriteLine("No file links found.");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine($"Found {links.Count} link(s):");

            foreach (string link in links)
            {
                Console.WriteLine(link);
            }

            Console.WriteLine();

            bool allDownloadsSuccessful = true;

            foreach (string link in links)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (downloadedUrls.Contains(link))
                {
                    Console.WriteLine($"Already downloaded: {link}");
                    continue;
                }

                bool success = await DownloadFileAsync(
                    link,
                    cancellationToken
                );

                if (success)
                {
                    downloadedUrls.Add(link);

                    await File.AppendAllTextAsync(
                        downloadedUrlsFile,
                        link + Environment.NewLine,
                        cancellationToken
                    );
                }
                else
                {
                    allDownloadsSuccessful = false;
                }
            }

            if (!allDownloadsSuccessful)
            {
                Console.WriteLine(
                    "Some downloads failed. This email will be retried."
                );

                break;
            }
        }

        lastUid = uid.Id;
        await SaveStateAsync();

        Console.WriteLine();
        Console.WriteLine("Email processed.");
    }

    await imapClient.DisconnectAsync(true, cancellationToken);
}


List<string> ExtractLinks(string text)
{
    string decodedText = WebUtility.HtmlDecode(text);

    Regex linkRegex = new Regex(
        @"https?://[^\s<>""']+",
        RegexOptions.IgnoreCase
    );

    HashSet<string> links = new HashSet<string>(
        StringComparer.OrdinalIgnoreCase
    );

    foreach (Match match in linkRegex.Matches(decodedText))
    {
        string rawUrl = match.Value.TrimEnd(
            '.', ',', ';', ':', '!', '?', ')', ']', '}'
        );

        if (Uri.TryCreate(rawUrl, UriKind.Absolute, out Uri? uri) &&
            (uri.Scheme == Uri.UriSchemeHttp ||
             uri.Scheme == Uri.UriSchemeHttps))
        {
            links.Add(uri.AbsoluteUri);
        }
    }

    return links.ToList();
}


async Task<bool> DownloadFileAsync(
    string url,
    CancellationToken cancellationToken)
{
    Console.WriteLine($"Downloading: {url}");

    string? temporaryPath = null;

    try
    {
        using HttpResponseMessage response = await httpClient.GetAsync(
            url,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        long? totalBytes = response.Content.Headers.ContentLength;

        string fileName = GetFileName(
            new Uri(url),
            response.Content.Headers.ContentDisposition?.FileNameStar ??
            response.Content.Headers.ContentDisposition?.FileName,
            response.Content.Headers.ContentType?.MediaType
        );

        string destinationPath = Path.Combine(
            downloadsDirectory,
            fileName
        );

        if (File.Exists(destinationPath))
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);
            string hash = Convert.ToHexString(
                SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(url))
            )[..8];

            fileName = $"{name}_{hash}{extension}";
            destinationPath = Path.Combine(downloadsDirectory, fileName);
        }

        if (File.Exists(destinationPath))
        {
            Console.WriteLine($"File already exists: {fileName}");
            return true;
        }

        temporaryPath = destinationPath + ".download";

        await using Stream input = await response.Content.ReadAsStreamAsync(
            cancellationToken
        );

        await using FileStream output = new FileStream(
            temporaryPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        byte[] buffer = new byte[81920];

        long downloadedBytes = 0;
        int bytesRead;

        while ((bytesRead = await input.ReadAsync(
            buffer,
            cancellationToken
        )) > 0)
        {
            await output.WriteAsync(
                buffer.AsMemory(0, bytesRead),
                cancellationToken
            );

            downloadedBytes += bytesRead;

            if (totalBytes.HasValue && totalBytes.Value > 0)
            {
                double percentage =
                    downloadedBytes * 100.0 / totalBytes.Value;

                Console.Write(
                    $"\rProgress: {percentage:F1}% " +
                    $"({downloadedBytes}/{totalBytes.Value} bytes)"
                );
            }
            else
            {
                Console.Write(
                    $"\rDownloaded: {downloadedBytes} bytes"
                );
            }
        }

        Console.WriteLine();

        await output.DisposeAsync();

        File.Move(temporaryPath, destinationPath);

        temporaryPath = null;

        Console.WriteLine($"File saved: downloads/{fileName}");

        return true;
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Download failed: {ex.Message}");
        return false;
    }
    finally
    {
        if (temporaryPath != null && File.Exists(temporaryPath))
        {
            File.Delete(temporaryPath);
        }
    }
}


string GetFileName(Uri uri, string? contentDispositionName, string? mediaType)
{
    string fileName = "";

    if (!string.IsNullOrWhiteSpace(contentDispositionName))
    {
        fileName = contentDispositionName.Trim('"');
    }

    if (string.IsNullOrWhiteSpace(fileName))
    {
        fileName = Path.GetFileName(uri.LocalPath);
    }

    if (string.IsNullOrWhiteSpace(fileName))
    {
        fileName = $"download_{Guid.NewGuid():N}";
    }

    fileName = Path.GetFileName(fileName);

    if (string.IsNullOrWhiteSpace(Path.GetExtension(fileName)))
    {
        string extension = mediaType switch
        {
            "application/pdf" => ".pdf",
            "application/zip" => ".zip",
            "application/x-zip-compressed" => ".zip",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "text/plain" => ".txt",
            _ => ".bin"
        };

        fileName += extension;
    }

    foreach (char invalidCharacter in Path.GetInvalidFileNameChars())
    {
        fileName = fileName.Replace(invalidCharacter, '_');
    }

    return fileName;
}


async Task SaveStateAsync()
{
    string state = $"{savedUidValidity}|{lastUid}";

    await File.WriteAllTextAsync(stateFile, state);
}


string ReadPassword()
{
    var password = new System.Text.StringBuilder();

    while (true)
    {
        ConsoleKeyInfo key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            break;
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
        }
    }

    return password.ToString();
}
