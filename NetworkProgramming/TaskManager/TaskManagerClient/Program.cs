using System.Net.Sockets;
using System.Text;
using System.Text.Json;

const string serverIp = "127.0.0.1";
const int serverPort = 5000;

Console.OutputEncoding = Encoding.UTF8;

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

using TcpClient client = new TcpClient();

Console.WriteLine("       TASK MANAGER CLIENT");

try
{
    Console.WriteLine($"Connecting to {serverIp}:{serverPort}...");

    await client.ConnectAsync(serverIp, serverPort);

    Console.WriteLine("Connected to server!");
    Console.WriteLine();

    NetworkStream stream = client.GetStream();

    using StreamReader reader = new StreamReader(
        stream,
        Encoding.UTF8,
        leaveOpen: true
    );

    using StreamWriter writer = new StreamWriter(
        stream,
        Encoding.UTF8,
        leaveOpen: true
    )
    {
        AutoFlush = true
    };

    Task receiveTask = ReceiveMessagesAsync(reader);

    while (client.Connected)
    {
        ShowMenu();

        string choice = Console.ReadLine() ?? "";

        try
        {
            switch (choice)
            {
                case "1":
                    await CreateTaskAsync(writer);
                    break;

                case "2":
                    await UpdateTaskAsync(writer);
                    break;

                case "3":
                    await ChangeStatusAsync(writer);
                    break;

                case "4":
                    await DeleteTaskAsync(writer);
                    break;

                case "0":
                    Console.WriteLine("Disconnecting...");
                    return;

                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
        }
        catch (IOException)
        {
            Console.WriteLine("Connection to server was lost.");
            break;
        }
    }

    await receiveTask;
}
catch (SocketException)
{
    Console.WriteLine("Could not connect to the server.");
    Console.WriteLine("Make sure the server is running.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}


void ShowMenu()
{
    Console.WriteLine();
    Console.WriteLine("MENU");
    Console.WriteLine("1. Create task");
    Console.WriteLine("2. Edit task");
    Console.WriteLine("3. Change task status");
    Console.WriteLine("4. Delete task");
    Console.WriteLine("0. Exit");
    Console.Write("Choose: ");
}


async Task CreateTaskAsync(StreamWriter writer)
{
    Console.Write("Task title: ");
    string title = Console.ReadLine() ?? "";

    Console.Write("Task description: ");
    string description = Console.ReadLine() ?? "";

    await SendRequestAsync(writer, new ClientRequest
    {
        Type = "create",
        Title = title,
        Description = description
    });
}


async Task UpdateTaskAsync(StreamWriter writer)
{
    Console.Write("Task ID: ");

    if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Console.Write("New title (leave empty to keep current): ");
    string title = Console.ReadLine() ?? "";

    Console.Write("New description (leave empty to keep current): ");
    string description = Console.ReadLine() ?? "";

    await SendRequestAsync(writer, new ClientRequest
    {
        Type = "update",
        TaskId = id,
        Title = title,
        Description = string.IsNullOrEmpty(description)
            ? null
            : description
    });
}


async Task ChangeStatusAsync(StreamWriter writer)
{
    Console.Write("Task ID: ");

    if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("Choose status:");
    Console.WriteLine("1. Нове");
    Console.WriteLine("2. У роботі");
    Console.WriteLine("3. Виконано");
    Console.Write("Choose: ");

    string choice = Console.ReadLine() ?? "";

    string? status = choice switch
    {
        "1" => "Нове",
        "2" => "У роботі",
        "3" => "Виконано",
        _ => null
    };

    if (status == null)
    {
        Console.WriteLine("Invalid status.");
        return;
    }

    await SendRequestAsync(writer, new ClientRequest
    {
        Type = "status",
        TaskId = id,
        Status = status
    });
}


async Task DeleteTaskAsync(StreamWriter writer)
{
    Console.Write("Task ID: ");

    if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
    {
        Console.WriteLine("Invalid ID.");
        return;
    }

    await SendRequestAsync(writer, new ClientRequest
    {
        Type = "delete",
        TaskId = id
    });
}


async Task SendRequestAsync(StreamWriter writer, ClientRequest request)
{
    string json = JsonSerializer.Serialize(request);
    await writer.WriteLineAsync(json);
}


async Task ReceiveMessagesAsync(StreamReader reader)
{
    try
    {
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            ServerMessage? message =
                JsonSerializer.Deserialize<ServerMessage>(
                    line,
                    jsonOptions
                );

            if (message == null)
            {
                continue;
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("SERVER UPDATE");

            if (!string.IsNullOrWhiteSpace(message.Text))
            {
                Console.WriteLine(message.Text);
            }

            if (message.Tasks != null)
            {
                PrintTasks(message.Tasks);
            }

            if (message.Type == "error")
            {
                Console.WriteLine("Please check your request.");
            }

            Console.WriteLine();
        }
    }
    catch (IOException)
    {
        Console.WriteLine("Disconnected from server.");
    }
    catch (JsonException)
    {
        Console.WriteLine("Received invalid data from server.");
    }
}


void PrintTasks(List<TaskItem> tasks)
{
    Console.WriteLine();
    Console.WriteLine("CURRENT TASK LIST");

    if (tasks.Count == 0)
    {
        Console.WriteLine("There are no tasks yet.");
        return;
    }

    foreach (TaskItem task in tasks)
    {
        Console.WriteLine($"ID: {task.Id}");
        Console.WriteLine($"Title: {task.Title}");
        Console.WriteLine($"Description: {task.Description}");
        Console.WriteLine($"Status: {task.Status}");
    }
}


class ClientRequest
{
    public string Type { get; set; } = "";
    public int TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
}


class ServerMessage
{
    public string Type { get; set; } = "";
    public string? Text { get; set; }
    public List<TaskItem>? Tasks { get; set; }
}


class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Нове";
}
