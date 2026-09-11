using System.Net.Sockets;
using System.Text;

int port = 6000;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    port = parsedPort;
}

string logFilePath = args.Length > 1 ? args[1] : "server.log";

if (!File.Exists(logFilePath))
{
    File.Create(logFilePath).Dispose();
}

object fileLock = new();
using CancellationTokenSource cts = new();
using UdpClient udpServer = new(port);

Console.WriteLine($"UDP лог-сервер запущено на порту {port}.");
Console.WriteLine($"Файл логів: {Path.GetFullPath(logFilePath)}");
Console.WriteLine("Натисніть будь-яку клавішу, щоб зупинити сервер...");

Task keyWatcherTask = Task.Run(() =>
{
    Console.ReadKey(intercept: true);
    cts.Cancel();
    udpServer.Close();
});

while (!cts.IsCancellationRequested)
{
    UdpReceiveResult result;
    try
    {
        result = await udpServer.ReceiveAsync(cts.Token);
    }
    catch (OperationCanceledException)
    {
        break;
    }
    catch (ObjectDisposedException)
    {
        break;
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"Помилка сокета: {ex.Message}");
        continue;
    }

    try
    {
        string raw = Encoding.UTF8.GetString(result.Buffer).Trim();
        string[] parts = raw.Split('|', 3);

        if (parts.Length != 3)
        {
            Console.WriteLine($"[{result.RemoteEndPoint}] Некоректний формат повідомлення: {raw}");
            continue;
        }

        string level = parts[0].Trim().ToUpperInvariant();
        string clientName = parts[1].Trim();
        string text = parts[2].Trim();
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logLine = $"{timestamp} - {level} - {clientName} - {text}";

        PrintColored(level, logLine);

        lock (fileLock)
        {
            File.AppendAllText(logFilePath, logLine + Environment.NewLine);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка обробки повідомлення від {result.RemoteEndPoint}: {ex.Message}");
    }
}

Console.WriteLine("Сервер зупинено.");

void PrintColored(string level, string line)
{
    ConsoleColor color = level switch
    {
        "ERROR" => ConsoleColor.Red,
        "INFO" => ConsoleColor.Green,
        "DEBUG" => ConsoleColor.Cyan,
        _ => ConsoleColor.Gray
    };

    ConsoleColor original = Console.ForegroundColor;
    Console.ForegroundColor = color;
    Console.WriteLine(line);
    Console.ForegroundColor = original;
}
