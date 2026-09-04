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

    using (client)
    await using (NetworkStream stream = client.GetStream())
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer);

            if (bytesRead == 0)
            {
                return;
            }

            string command = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {remoteEndpoint} -> {command}");

            string response = ProcessCommand(command);

            byte[] responseBytes = Encoding.UTF8.GetBytes(response);
            await stream.WriteAsync(responseBytes);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {remoteEndpoint} <- {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка під час обробки клієнта {remoteEndpoint}: {ex.Message}");
        }
    }
}

string ProcessCommand(string command)
{
    return command.Trim().ToUpperInvariant() switch
    {
        "PING" => "PONG",
        "DATETIME" => DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
        "RANDOM" => random.Next(0, 1000).ToString(),
        "UPTIME" => (DateTime.Now - serverStartTime).ToString(@"hh\:mm\:ss"),
        _ => "UNKNOWN COMMAND"
    };
}
