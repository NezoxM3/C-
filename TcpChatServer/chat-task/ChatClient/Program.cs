using System.Net.Sockets;
using System.Text;

string host = args.Length > 0 ? args[0] : "127.0.0.1";
int port = args.Length > 1 && int.TryParse(args[1], out int parsedPort) ? parsedPort : 5000;

Console.WriteLine("=== TCP чат-клієнт ===");
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
List<string> knownUsers = new();
using CancellationTokenSource cts = new();

Task pollingTask = Task.Run(async () =>
{
    while (!cts.IsCancellationRequested)
    {
        try
        {
            string listResponse = await SendRequestAsync(host, port, $"LIST|{username}|{password}");
            if (listResponse.StartsWith("OK|"))
            {
                string payload = listResponse[3..];
                lock (consoleLock)
                {
                    knownUsers = payload == "EMPTY" ? new List<string>() : payload.Split(',').ToList();
                }
            }

            string inboxResponse = await SendRequestAsync(host, port, $"INBOX|{username}|{password}");
            if (inboxResponse.StartsWith("OK|"))
            {
                string payload = inboxResponse[3..];
                if (payload != "EMPTY")
                {
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
            }
        }
        catch (Exception ex)
        {
            lock (consoleLock)
            {
                Console.WriteLine($"[Помилка опитування сервера: {ex.Message}]");
            }
        }

        try
        {
            await Task.Delay(3000, cts.Token);
        }
        catch (TaskCanceledException)
        {
        
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
        lock (consoleLock)
        {
            Console.WriteLine(knownUsers.Count > 0
                ? "Користувачі: " + string.Join(", ", knownUsers)
                : "Інших користувачів поки немає (оновлюється кожні 3с).");
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
