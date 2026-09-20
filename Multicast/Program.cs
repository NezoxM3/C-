using System.Net;
using System.Net.Sockets;
using System.Text;

int tcpPort = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    tcpPort = parsedPort;
}

IPAddress multicastAddress = IPAddress.Parse("230.0.0.1");
int multicastPort = 11000;

string latestMessage = "";
object messageLock = new();

string localIp = GetLocalIPAddress();


_ = Task.Run(RunTcpServerAsync);

Console.WriteLine($"TCP сервер: {localIp}:{tcpPort}");
Console.WriteLine($"Multicast група: {multicastAddress}:{multicastPort}");
Console.WriteLine();

while (true)
{
    Console.Write("Enter message: ");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    lock (messageLock)
    {
        latestMessage = input;
    }

    SendMulticastNotification(localIp, tcpPort);
    Console.WriteLine("Multicast notification sent");
    Console.WriteLine();
}

void SendMulticastNotification(string ip, int port)
{
    using UdpClient sender = new();
    string notification = $"NEW_MESSAGE|{ip}|{port}";
    byte[] data = Encoding.UTF8.GetBytes(notification);
    sender.Send(data, data.Length, new IPEndPoint(multicastAddress, multicastPort));
}

async Task RunTcpServerAsync()
{
    TcpListener listener = new(IPAddress.Any, tcpPort);
    listener.Start();

    while (true)
    {
        TcpClient client = await listener.AcceptTcpClientAsync();
        _ = HandleClientAsync(client);
    }
}

async Task HandleClientAsync(TcpClient client)
{
    using (client)
    await using (NetworkStream stream = client.GetStream())
    {
        string messageToSend;
        lock (messageLock)
        {
            messageToSend = latestMessage;
        }

        byte[] data = Encoding.UTF8.GetBytes(messageToSend);
        await stream.WriteAsync(data);
    }

    Console.WriteLine("Client connected");
}

string GetLocalIPAddress()
{
    try
    {
        using Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.Connect("8.8.8.8", 65530);
        return (socket.LocalEndPoint as IPEndPoint)?.Address.ToString() ?? "127.0.0.1";
    }
    catch
    {
        return "127.0.0.1";
    }
}
