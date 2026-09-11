using System.Net.Sockets;
using System.Text;

string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out int parsedPort) ? parsedPort : 6000;
string clientName = args.Length > 2 ? args[2] : $"Client_{Random.Shared.Next(1, 1000)}";

string[] levels = { "DEBUG", "INFO", "ERROR" };
string[] sampleMessages =
{
    "Connected to database",
    "File not found",
    "Updating cache",
    "User logged in",
    "Cache miss",
    "Request timeout",
    "Configuration loaded",
    "Disk space low",
    "Retrying connection",
    "Task completed successfully"
};

using UdpClient udpClient = new();
udpClient.Connect(host, port);

Console.WriteLine($"Клієнт \"{clientName}\" запущено. Сервер: {host}:{port}");
Console.WriteLine("Надсилання випадкових логів кожні 2-3 секунди... (Ctrl+C — зупинити)");

Random random = Random.Shared;

while (true)
{
    string level = levels[random.Next(levels.Length)];
    string text = sampleMessages[random.Next(sampleMessages.Length)];
    string message = $"{level}|{clientName}|{text}";

    try
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await udpClient.SendAsync(data, data.Length);
        Console.WriteLine($"Надіслано: {level} - {text}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка надсилання: {ex.Message}");
    }

    int delayMs = random.Next(2000, 3001);
    await Task.Delay(delayMs);
}
