using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;


int port = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    port = parsedPort;
}

// username -> password hash
ConcurrentDictionary<string, string> users = new(StringComparer.OrdinalIgnoreCase);
// username -> черга недоставлених повідомлень
ConcurrentDictionary<string, ConcurrentQueue<ChatMessage>> inboxes = new(StringComparer.OrdinalIgnoreCase);

TcpListener listener = new(IPAddress.Any, port);
listener.Start();

Console.WriteLine($"Чат-сервер запущено на порту {port}.");
Console.WriteLine("Очікування підключень клієнтів... (Ctrl+C — зупинити)");

while (true)
{
    TcpClient client = await listener.AcceptTcpClientAsync();
    _ = HandleClientAsync(client);
}

async Task HandleClientAsync(TcpClient client)
{
    string remote = client.Client.RemoteEndPoint?.ToString() ?? "невідомо";

    using (client)
    await using (NetworkStream stream = client.GetStream())
    using (StreamReader reader = new(stream, Encoding.UTF8))
    await using (StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true })
    {
        try
        {
            string? line = await reader.ReadLineAsync();
            if (line is null)
            {
                return;
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {remote} -> {line}");
            string response = HandleRequest(line);
            await writer.WriteLineAsync(response);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {remote} <- {response}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка обробки клієнта {remote}: {ex.Message}");
        }
    }
}

string HandleRequest(string line)
{
    string[] parts = line.Split('|');
    if (parts.Length == 0)
    {
        return "ERROR|BAD_REQUEST";
    }

    string command = parts[0].ToUpperInvariant();

    return command switch
    {
        "REGISTER" when parts.Length == 3 => Register(parts[1], parts[2]),
        "LOGIN" when parts.Length == 3 => Login(parts[1], parts[2]),
        "LIST" when parts.Length == 3 => ListUsers(parts[1], parts[2]),
        "SEND" when parts.Length >= 5 => SendMessage(parts[1], parts[2], parts[3], string.Join('|', parts[4..])),
        "INBOX" when parts.Length == 3 => GetInbox(parts[1], parts[2]),
        _ => "ERROR|BAD_REQUEST"
    };
}

string Register(string username, string password)
{
    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
    {
        return "ERROR|INVALID_INPUT";
    }

    bool added = users.TryAdd(username, HashPassword(password));
    if (!added)
    {
        return "ERROR|USER_EXISTS";
    }

    inboxes.TryAdd(username, new ConcurrentQueue<ChatMessage>());
    return "OK";
}

string Login(string username, string password)
{
    if (!users.TryGetValue(username, out string? storedHash))
    {
        return "ERROR|INVALID_CREDENTIALS";
    }

    return storedHash == HashPassword(password) ? "OK" : "ERROR|INVALID_CREDENTIALS";
}

string ListUsers(string username, string password)
{
    if (!IsAuthorized(username, password))
    {
        return "ERROR|INVALID_CREDENTIALS";
    }

    var others = users.Keys.Where(u => !u.Equals(username, StringComparison.OrdinalIgnoreCase)).ToList();
    string payload = others.Count > 0 ? string.Join(',', others) : "EMPTY";
    return $"OK|{payload}";
}

string SendMessage(string username, string password, string target, string text)
{
    if (!IsAuthorized(username, password))
    {
        return "ERROR|INVALID_CREDENTIALS";
    }

    if (!users.ContainsKey(target))
    {
        return "ERROR|USER_NOT_FOUND";
    }

    if (string.IsNullOrWhiteSpace(text))
    {
        return "ERROR|EMPTY_MESSAGE";
    }

    ConcurrentQueue<ChatMessage> queue = inboxes.GetOrAdd(target, _ => new ConcurrentQueue<ChatMessage>());
    queue.Enqueue(new ChatMessage(username, text, DateTime.Now));
    return "OK";
}

string GetInbox(string username, string password)
{
    if (!IsAuthorized(username, password))
    {
        return "ERROR|INVALID_CREDENTIALS";
    }

    ConcurrentQueue<ChatMessage> queue = inboxes.GetOrAdd(username, _ => new ConcurrentQueue<ChatMessage>());
    List<string> messages = new();

    while (queue.TryDequeue(out ChatMessage? msg))
    {
        messages.Add($"{msg.From}::{msg.SentAt:HH:mm:ss}::{msg.Text}");
    }

    string payload = messages.Count > 0 ? string.Join('\u001E', messages) : "EMPTY";
    return $"OK|{payload}";
}

bool IsAuthorized(string username, string password) =>
    users.TryGetValue(username, out string? hash) && hash == HashPassword(password);

string HashPassword(string password) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

record ChatMessage(string From, string Text, DateTime SentAt);
