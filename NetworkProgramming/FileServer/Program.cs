using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text;

const int tcpPort = 5000;
const int udpPort = 5001;

string serverDirectory = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "../../../../ServerFiles")
);

Directory.CreateDirectory(serverDirectory);

var networkInfo = GetNetworkInfo();

IPAddress serverIp = networkInfo.Address;
IPAddress broadcastIp = networkInfo.Broadcast;

using UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork);

udpClient.EnableBroadcast = true;
udpClient.Client.Bind(new IPEndPoint(serverIp, 0));

using TcpListener tcpListener = new TcpListener(serverIp, tcpPort);

tcpListener.Start();

using FileSystemWatcher watcher = new FileSystemWatcher(serverDirectory);

watcher.IncludeSubdirectories = false;
watcher.NotifyFilter =
    NotifyFilters.FileName |
    NotifyFilters.Size |
    NotifyFilters.LastWrite;

watcher.Created += (_, e) =>
{
    _ = Task.Run(async () =>
    {
        await Task.Delay(500);
        await SendFileNotification("NEW_FILE", e.FullPath);
    });
};

watcher.Changed += (_, e) =>
{
    _ = Task.Run(async () =>
    {
        await Task.Delay(500);
        await SendFileNotification("CHANGE_FILE", e.FullPath);
    });
};

watcher.Deleted += (_, e) =>
{
    _ = Task.Run(async () =>
    {
        string fileName = Path.GetFileName(e.FullPath);

        await SendBroadcast($"DELETE_FILE|{fileName}");
        
        Console.WriteLine($"Deleted: {fileName}");
    });
};

watcher.EnableRaisingEvents = true;

Console.WriteLine("         FILE SERVER");
Console.WriteLine($"Server IP: {serverIp}");
Console.WriteLine($"TCP Port: {tcpPort}");
Console.WriteLine($"UDP Port: {udpPort}");
Console.WriteLine($"Broadcast: {broadcastIp}");
Console.WriteLine($"Directory: {serverDirectory}");
Console.WriteLine("Server is running...\n");

while (true)
{
    TcpClient client = await tcpListener.AcceptTcpClientAsync();

    _ = Task.Run(() => HandleClientAsync(client));
}


async Task SendFileNotification(string eventType, string filePath)
{
    try
    {
        if (!File.Exists(filePath))
            return;

        FileInfo fileInfo = new FileInfo(filePath);

        string message =
            $"{eventType}|{fileInfo.Name}|{fileInfo.Length}|{serverIp}|{tcpPort}";

        await SendBroadcast(message);

        Console.WriteLine(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Notification error: {ex.Message}");
    }
}


async Task SendBroadcast(string message)
{
    try
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        await udpClient.SendAsync(
            data,
            data.Length,
            new IPEndPoint(broadcastIp, udpPort)
        );
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"UDP error: {ex.Message}");
    }
}


async Task HandleClientAsync(TcpClient client)
{
    using (client)
    {
        try
        {
            using NetworkStream stream = client.GetStream();

            using BinaryReader reader = new BinaryReader(
                stream,
                Encoding.UTF8,
                leaveOpen: true
            );

            using BinaryWriter writer = new BinaryWriter(
                stream,
                Encoding.UTF8,
                leaveOpen: true
            );

            string fileName = reader.ReadString();

            if (Path.GetFileName(fileName) != fileName)
            {
                writer.Write(false);
                return;
            }

            string filePath = Path.Combine(serverDirectory, fileName);

            if (!File.Exists(filePath))
            {
                writer.Write(false);
                return;
            }

            using FileStream fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite | FileShare.Delete
            );

            writer.Write(true);
            writer.Write(fileStream.Length);
            writer.Flush();

            await fileStream.CopyToAsync(stream);

            Console.WriteLine($"Sent file: {fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Client error: {ex.Message}");
        }
    }
}


(NetworkInterface Interface, IPAddress Address, IPAddress Broadcast)
    GetNetworkInfo()
{
    var interfaces = NetworkInterface.GetAllNetworkInterfaces()
        .Where(n =>
            n.OperationalStatus == OperationalStatus.Up &&
            n.NetworkInterfaceType != NetworkInterfaceType.Loopback
        )
        .OrderBy(n =>
            n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ? 0 :
            n.NetworkInterfaceType == NetworkInterfaceType.Ethernet ? 1 : 2
        );

    foreach (var networkInterface in interfaces)
    {
        var properties = networkInterface.GetIPProperties();

        var ipv4 = properties.UnicastAddresses.FirstOrDefault(a =>
            a.Address.AddressFamily == AddressFamily.InterNetwork &&
            a.IPv4Mask != null &&
            !IPAddress.IsLoopback(a.Address)
        );

        if (ipv4 == null)
            continue;

        byte[] addressBytes = ipv4.Address.GetAddressBytes();
        byte[] maskBytes = ipv4.IPv4Mask.GetAddressBytes();

        byte[] broadcastBytes = new byte[4];

        for (int i = 0; i < 4; i++)
        {
            broadcastBytes[i] = (byte)(
                addressBytes[i] | ~maskBytes[i]
            );
        }

        return (
            networkInterface,
            ipv4.Address,
            new IPAddress(broadcastBytes)
        );
    }

    throw new Exception("No active IPv4 network interface found.");
}
