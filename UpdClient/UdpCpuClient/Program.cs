using System.Net.Sockets;
using System.Text;

string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out int parsedPort) ? parsedPort : 5000;

using UdpClient udpClient = new();
udpClient.Connect(host, port);

Console.WriteLine($"UDP клієнт. Сервер: {host}:{port}");
Console.WriteLine("Опитування кожні 2 секунди... (Ctrl+C — зупинити)");

byte[] requestBytes = Encoding.UTF8.GetBytes("cpu");

while (true)
{
    try
    {
        await udpClient.SendAsync(requestBytes, requestBytes.Length);

        UdpReceiveResult result = await udpClient.ReceiveAsync();
        string response = Encoding.UTF8.GetString(result.Buffer);

        Console.WriteLine($"Поточне навантаження CPU: {response}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка зв'язку з сервером: {ex.Message}");
    }

    await Task.Delay(2000);
}
