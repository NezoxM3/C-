using System.Net;
using System.Net.Sockets;
using System.Text;

string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out int parsedPort) ? parsedPort : 5000;

IPAddress multicastAddress = IPAddress.Parse("230.0.0.1");
int multicastPort = 11000;

Console.WriteLine("=== TCP чат-клієнт (multicast-сповіщення замість опитування) ===");
Console.WriteLine($"Сервер: {host}:{port}");

string username = "";
string password = "";
bool authorized = false;

while (!authorized)
{
    Console.WriteLine();
    Console.WriteLine("1 - Реєстрація");
    Console.WriteLine("2 - Авторизація");
    Console.Write("Вибір: ");
    string? choice = Console.ReadLine();

    Console.Write("Ім'я користувача: ");
    username = (Console.ReadLine() ?? "").Trim();
    Console.Write("Пароль: ");
    password = (Console.ReadLine() ?? "").Trim();

    string command = choice == "1" ? "REGISTER" : "LOGIN";
    string response = await SendRequestAsync(host, port, $"{command}|{username}|{password}");

    if (response == "OK")
    {
        authorized = true;
        Console.WriteLine(choice == "1" ? "Реєстрація успішна!" : "Авторизація успішна!");
    }
    else
    {
        Console.WriteLine($"Помилка: {response}");
    }
}

object consoleLock = new();
using CancellationTokenSource cts = new();


Task multicastListenerTask = Task.Run(async () =>
{
    using UdpClient multicastListener = new();
    multicastListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
    multicastListener.Client.Bind(new IPEndPoint(IPAddress.Any, multicastPort));
    multicastListener.JoinMulticastGroup(multicastAddress);

    while (!cts.IsCancellationRequested)
    {
        UdpReceiveResult result;
        try
        {
            result = await multicastListener.ReceiveAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (ObjectDisposedException)
        {
            break;
        }

        string notification = Encoding.UTF8.GetString(result.Buffer);
        string[] parts = notification.Split('|');

        if (parts.Length != 2 || parts[0] != "NEW_MESSAGE")
        {
            continue;
        }

        string recipient = parts[1];
        if (!recipient.Equals(username, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        string inboxResponse = await SendRequestAsync(host, port, $"INBOX|{username}|{password}");
        if (!inboxResponse.StartsWith("OK|"))
        {
            continue;
        }

        string payload = inboxResponse[3..];
        if (payload == "EMPTY")
        {
            continue;
        }

        string[] messages = payload.Split('\u001E');
        lock (consoleLock)
        {
            Console.WriteLine();
            foreach (string msg in messages)
            {
                string[] fields = msg.Split("::", 3);
                if (fields.Length == 3)
                {
                    Console.WriteLine($"[{fields[1]}] Повідомлення від {fields[0]}: {fields[2]}");
                }
            }
            Console.Write("> ");
        }
    }
});

while (true)
{
    lock (consoleLock)
    {
        Console.WriteLine();
        Console.WriteLine("1 - Список користувачів");
        Console.WriteLine("2 - Надіслати повідомлення");
        Console.WriteLine("3 - Вийти");
        Console.Write("> ");
    }

    string? choice = Console.ReadLine();

    if (choice == "1")
    {
        string listResponse = await SendRequestAsync(host, port, $"LIST|{username}|{password}");
        if (listResponse.StartsWith("OK|"))
        {
            string payload = listResponse[3..];
            lock (consoleLock)
            {
                Console.WriteLine(payload == "EMPTY"
                    ? "Інших користувачів поки немає."
                    : "Користувачі: " + payload.Replace(',', ' '));
            }
        }
        else
        {
            Console.WriteLine($"Помилка: {listResponse}");
        }
    }
    else if (choice == "2")
    {
        Console.Write("Кому (ім'я користувача): ");
        string target = (Console.ReadLine() ?? "").Trim();
        Console.Write("Текст повідомлення: ");
        string text = Console.ReadLine() ?? "";

        string response = await SendRequestAsync(host, port, $"SEND|{username}|{password}|{target}|{text}");
        Console.WriteLine(response == "OK" ? "Повідомлення надіслано." : $"Помилка: {response}");
    }
    else if (choice == "3")
    {
        cts.Cancel();
        Console.WriteLine("До побачення!");
        break;
    }
}

async Task<string> SendRequestAsync(string targetHost, int targetPort, string requestLine)
{
    using TcpClient client = new();
    await client.ConnectAsync(targetHost, targetPort);
    await using NetworkStream stream = client.GetStream();
    using StreamReader reader = new(stream, Encoding.UTF8);
    await using StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true };

    await writer.WriteLineAsync(requestLine);
    string? response = await reader.ReadLineAsync();
    return response ?? "ERROR|NO_RESPONSE";
}
