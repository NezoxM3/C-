using System.Net.Sockets;
using System.Text;

string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out int parsedPort) ? parsedPort : 5000;

Console.WriteLine($"TCP-клієнт. Сервер: {host}:{port}");
Console.WriteLine("Доступні команди: PING, DATETIME, RANDOM, UPTIME, EXIT");

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    string command = input.Trim();

    if (command.Equals("EXIT", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Завершення роботи клієнта.");
        break;
    }

    try
    {
        string response = await SendCommandAsync(host, port, command);
        Console.WriteLine($"Відповідь сервера: {response}");
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"Не вдалося підключитися до сервера: {ex.Message}");
    }
}

async Task<string> SendCommandAsync(string host, int port, string command)
{
    using TcpClient client = new();
    await client.ConnectAsync(host, port);

    await using NetworkStream stream = client.GetStream();

    byte[] requestBytes = Encoding.UTF8.GetBytes(command);
    await stream.WriteAsync(requestBytes);

    byte[] buffer = new byte[1024];
    int bytesRead = await stream.ReadAsync(buffer);

    return Encoding.UTF8.GetString(buffer, 0, bytesRead);
}
