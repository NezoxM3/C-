using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

int port = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    port = parsedPort;
}

DateTime serverStartTime = DateTime.Now;
Random random = new();

TcpListener listener = new(IPAddress.Any, port);
listener.Start();

Console.WriteLine($"Сервер запущено на порту {port}.");
Console.WriteLine("Очікування підключень клієнтів... (Ctrl+C — зупинити)");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    _ = HandleClientAsync(client);
}

async Task HandleClientAsync(TcpClient client)
{
    string remoteEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "невідомо";
    Console.WriteLine($"Клієнт підключився: {remoteEndpoint}");

    using (client)
    await using (NetworkStream stream = client.GetStream())
    {
        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead;
            try
            {
                bytesRead = await stream.ReadAsync(buffer);
            }
            catch (IOException)
            {
                break;
            }

            if (bytesRead == 0)
            {
                break;
            }

            string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {remoteEndpoint} -> {command}");

            string response = ProcessCommand(command);

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseBytes);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {remoteEndpoint} <- {response}");
        }
    }

    Console.WriteLine($"Клієнт відключився: {remoteEndpoint}");
}

string ProcessCommand(string rawCommand)
{
    string[] parts = rawCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0)
    {
        return "UNKNOWN COMMAND";
    }

    string name = parts[0].ToUpperInvariant();

    switch (name)
    {
        case "PING":
            return "PONG";

        case "DATETIME":
            return DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

        case "RANDOM":
            return random.Next(0, 1000).ToString();

        case "UPTIME":
            return (DateTime.Now - serverStartTime).ToString(@"hh\:mm\:ss");

        case "ADD" when parts.Length == 3
            && TryParseNumber(parts[1], out double addA)
            && TryParseNumber(parts[2], out double addB):
            return FormatNumber(addA + addB);

        case "SUBTRACT" when parts.Length == 3
            && TryParseNumber(parts[1], out double subA)
            && TryParseNumber(parts[2], out double subB):
            return FormatNumber(subA - subB);

        case "SQUARE" when parts.Length == 2
            && TryParseNumber(parts[1], out double sqA):
            return FormatNumber(sqA * sqA);

        default:
            return "UNKNOWN COMMAND";
    }
}

bool TryParseNumber(string text, out double value) =>
    double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

string FormatNumber(double value) =>
    value == Math.Floor(value)
        ? ((long)value).ToString(CultureInfo.InvariantCulture)
        : value.ToString("0.####", CultureInfo.InvariantCulture);
