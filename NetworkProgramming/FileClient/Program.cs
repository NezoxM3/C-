using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

const int udpPort = 5001;

string clientDirectory = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "../../../../ClientFiles")
);

Directory.CreateDirectory(clientDirectory);

using UdpClient udpClient = new UdpClient(AddressFamily.InterNetwork);

udpClient.Client.SetSocketOption(
    SocketOptionLevel.Socket,
    SocketOptionName.ReuseAddress,
    true
);

udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, udpPort));

var fileLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

Console.WriteLine("         FILE CLIENT");
Console.WriteLine($"UDP Port: {udpPort}");
Console.WriteLine($"Directory: {clientDirectory}");
Console.WriteLine("Waiting for server notifications...\n");

while (true)
{
    try
    {
        UdpReceiveResult result = await udpClient.ReceiveAsync();

        string message = Encoding.UTF8.GetString(result.Buffer);

        Console.WriteLine($"Received: {message}");

        string[] parts = message.Split('|');

        if (parts.Length < 2)
            continue;

        string command = parts[0];
        string fileName = parts[1];

        if (string.IsNullOrWhiteSpace(fileName) ||
            Path.GetFileName(fileName) != fileName)
        {
            continue;
        }

        SemaphoreSlim fileLock = fileLocks.GetOrAdd(
            fileName,
            _ => new SemaphoreSlim(1, 1)
        );

        _ = Task.Run(async () =>
        {
            await fileLock.WaitAsync();

            try
            {
                switch (command)
                {
                    case "NEW_FILE":
                    case "CHANGE_FILE":

                        if (parts.Length < 5 ||
                            !long.TryParse(parts[2], out long expectedSize) ||
                            !IPAddress.TryParse(parts[3], out IPAddress? serverIp) ||
                            !int.TryParse(parts[4], out int tcpPort))
                        {
                            Console.WriteLine("Invalid file notification.");
                            return;
                        }

                        await DownloadFile(
                            fileName,
                            expectedSize,
                            serverIp,
                            tcpPort
                        );

                        break;

                    case "DELETE_FILE":

                        DeleteFile(fileName);

                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Processing error: {ex.Message}");
            }
            finally
            {
                fileLock.Release();
            }
        });
    }
    catch (SocketException ex)
    {
        Console.WriteLine($"UDP receive error: {ex.Message}");
    }
}


async Task DownloadFile(
    string fileName,
    long expectedSize,
    IPAddress serverIp,
    int tcpPort)
{
    string destinationPath = Path.Combine(clientDirectory, fileName);
    string temporaryPath = destinationPath + ".download";

    try
    {
        using TcpClient tcpClient = new TcpClient(
            AddressFamily.InterNetwork
        );

        await tcpClient.ConnectAsync(serverIp, tcpPort);

        using NetworkStream stream = tcpClient.GetStream();

        using BinaryWriter writer = new BinaryWriter(
            stream,
            Encoding.UTF8,
            leaveOpen: true
        );

        using BinaryReader reader = new BinaryReader(
            stream,
            Encoding.UTF8,
            leaveOpen: true
        );

        writer.Write(fileName);
        writer.Flush();

        bool fileExists = reader.ReadBoolean();

        if (!fileExists)
        {
            Console.WriteLine($"File not found on server: {fileName}");
            return;
        }

        long fileSize = reader.ReadInt64();

        if (fileSize != expectedSize)
        {
            Console.WriteLine($"File size changed during transfer: {fileName}");
            return;
        }

        await using FileStream fileStream = new FileStream(
            temporaryPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );

        byte[] buffer = new byte[81920];
        long remainingBytes = fileSize;

        while (remainingBytes > 0)
        {
            int bytesToRead = (int)Math.Min(
                buffer.Length,
                remainingBytes
            );

            int bytesRead = await stream.ReadAsync(
                buffer.AsMemory(0, bytesToRead)
            );

            if (bytesRead == 0)
                throw new EndOfStreamException("Connection closed unexpectedly.");

            await fileStream.WriteAsync(
                buffer.AsMemory(0, bytesRead)
            );

            remainingBytes -= bytesRead;
        }

        await fileStream.FlushAsync();
        await fileStream.DisposeAsync();

        File.Move(temporaryPath, destinationPath, true);

        Console.WriteLine($"Downloaded: {fileName} ({fileSize} bytes)");
    }
    catch
    {
        if (File.Exists(temporaryPath))
            File.Delete(temporaryPath);

        throw;
    }
}


void DeleteFile(string fileName)
{
    string filePath = Path.Combine(clientDirectory, fileName);

    if (File.Exists(filePath))
    {
        File.Delete(filePath);

        Console.WriteLine($"Deleted locally: {fileName}");
    }
}
