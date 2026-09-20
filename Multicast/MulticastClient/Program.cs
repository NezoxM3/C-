using System.Net;
using System.Net.Sockets;
using System.Text;

IPAddress multicastAddress = IPAddress.Parse("230.0.0.1");
int multicastPort = 11000;

using UdpClient listener = new();
listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
listener.Client.Bind(new IPEndPoint(IPAddress.Any, multicastPort));
listener.JoinMulticastGroup(multicastAddress);

Console.WriteLine($"Слухаю multicast групу {multicastAddress}:{multicastPort}...");
Console.WriteLine();

while (true)
{
    Console.WriteLine("Waiting multicast...");

    UdpReceiveResult result;
    try
    {
        result = await listener.ReceiveAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка прийому multicast: {ex.Message}");
        continue;
    }

    string notification = Encoding.UTF8.GetString(result.Buffer);
    string[] parts = notification.Split('|');

    if (parts.Length != 3 || parts[0] != "NEW_MESSAGE")
    {
        continue;
    }

    string serverIp = parts[1];
    if (!int.TryParse(parts[2], out int serverPort))
    {
        continue;
    }

    Console.WriteLine("Notification received");
    Console.WriteLine("Connecting to server...");

    try
    {
        using TcpClient client = new();
        await client.ConnectAsync(serverIp, serverPort);
        await using NetworkStream stream = client.GetStream();

        using MemoryStream buffer = new();
        await stream.CopyToAsync(buffer);
        string message = Encoding.UTF8.GetString(buffer.ToArray());

        Console.WriteLine("Message:");
        Console.WriteLine(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Помилка отримання повідомлення: {ex.Message}");
    }

    Console.WriteLine();
}
