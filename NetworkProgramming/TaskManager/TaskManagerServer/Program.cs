using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

const int port = 5000;

string dataFile = Path.Combine(
    Directory.GetCurrentDirectory(),
    "tasks.json"
);

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

List<TaskItem> tasks = LoadTasks();
object tasksLock = new object();

var clients = new ConcurrentDictionary<TcpClient, ClientConnection>();

TcpListener listener = new TcpListener(IPAddress.Any, port);
listener.Start();

Console.WriteLine("       TASK MANAGER SERVER");
Console.WriteLine($"Server is listening on port {port}.");
Console.WriteLine($"Loaded tasks: {tasks.Count}");
Console.WriteLine("Waiting for clients...");
Console.WriteLine();

while (true)
{
    TcpClient tcpClient = await listener.AcceptTcpClientAsync();

    Console.WriteLine($"Client connected: {tcpClient.Client.RemoteEndPoint}");

    _ = HandleClientAsync(tcpClient);
}


async Task HandleClientAsync(TcpClient tcpClient)
{
    ClientConnection? connection = null;

    try
    {
        NetworkStream stream = tcpClient.GetStream();

        using StreamReader reader = new StreamReader(
            stream,
            Encoding.UTF8,
            leaveOpen: true
        );

        StreamWriter writer = new StreamWriter(
            stream,
            Encoding.UTF8,
            leaveOpen: true
        )
        {
            AutoFlush = true
        };

        connection = new ClientConnection(writer);
        clients.TryAdd(tcpClient, connection);

        List<TaskItem> currentTasks;

        lock (tasksLock)
        {
            currentTasks = tasks
                .Select(CloneTask)
                .ToList();
        }

        await connection.SendAsync(new ServerMessage
        {
            Type = "snapshot",
            Tasks = currentTasks
        });

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            try
            {
                ClientRequest? request =
                    JsonSerializer.Deserialize<ClientRequest>(
                        line,
                        jsonOptions
                    );

                if (request == null)
                {
                    continue;
                }

                await ProcessRequestAsync(request);
            }
            catch (JsonException)
            {
                await connection.SendAsync(new ServerMessage
                {
                    Type = "error",
                    Text = "Invalid request format."
                });
            }
        }
    }
    catch (IOException)
    {
    }
    catch (SocketException)
    {
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Client error: {ex.Message}");
    }
    finally
    {
        clients.TryRemove(tcpClient, out _);

        connection?.Dispose();
        tcpClient.Close();

        Console.WriteLine("Client disconnected.");
    }
}


async Task ProcessRequestAsync(ClientRequest request)
{
    ServerMessage? message = null;

    lock (tasksLock)
    {
        switch (request.Type)
        {
            case "create":
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    message = new ServerMessage
                    {
                        Type = "error",
                        Text = "Task title cannot be empty."
                    };

                    break;
                }

                int newId = tasks.Count == 0
                    ? 1
                    : tasks.Max(t => t.Id) + 1;

                TaskItem newTask = new TaskItem
                {
                    Id = newId,
                    Title = request.Title.Trim(),
                    Description = request.Description?.Trim() ?? "",
                    Status = "Нове"
                };

                tasks.Add(newTask);
                SaveTasks();

                message = new ServerMessage
                {
                    Type = "created",
                    Text = $"New task created: [{newTask.Id}] {newTask.Title}",
                    Tasks = tasks.Select(CloneTask).ToList()
                };

                break;
            }

            case "update":
            {
                TaskItem? task = tasks.FirstOrDefault(
                    t => t.Id == request.TaskId
                );

                if (task == null)
                {
                    message = ErrorMessage("Task not found.");
                    break;
                }

                if (!string.IsNullOrWhiteSpace(request.Title))
                {
                    task.Title = request.Title.Trim();
                }

                if (request.Description != null)
                {
                    task.Description = request.Description.Trim();
                }

                SaveTasks();

                message = new ServerMessage
                {
                    Type = "updated",
                    Text = $"Task #{task.Id} was updated.",
                    Tasks = tasks.Select(CloneTask).ToList()
                };

                break;
            }

            case "status":
            {
                TaskItem? task = tasks.FirstOrDefault(
                    t => t.Id == request.TaskId
                );

                string[] allowedStatuses =
                {
                    "Нове",
                    "У роботі",
                    "Виконано"
                };

                if (task == null)
                {
                    message = ErrorMessage("Task not found.");
                    break;
                }

                if (request.Status == null ||
                    !allowedStatuses.Contains(request.Status))
                {
                    message = ErrorMessage("Invalid task status.");
                    break;
                }

                task.Status = request.Status;
                SaveTasks();

                message = new ServerMessage
                {
                    Type = "updated",
                    Text = $"Task #{task.Id} status changed to: {task.Status}",
                    Tasks = tasks.Select(CloneTask).ToList()
                };

                break;
            }

            case "delete":
            {
                TaskItem? task = tasks.FirstOrDefault(
                    t => t.Id == request.TaskId
                );

                if (task == null)
                {
                    message = ErrorMessage("Task not found.");
                    break;
                }

                string deletedTitle = task.Title;

                tasks.Remove(task);
                SaveTasks();

                message = new ServerMessage
                {
                    Type = "deleted",
                    Text = $"Task #{request.TaskId} ({deletedTitle}) was deleted.",
                    Tasks = tasks.Select(CloneTask).ToList()
                };

                break;
            }

            default:
                message = ErrorMessage("Unknown command.");
                break;
        }
    }

    if (message != null)
    {
        await BroadcastAsync(message);
    }
}


async Task BroadcastAsync(ServerMessage message)
{
    string json = JsonSerializer.Serialize(message);

    List<Task> sendTasks = new List<Task>();

    foreach (var pair in clients)
    {
        sendTasks.Add(SendSafelyAsync(pair.Key, pair.Value, json));
    }

    await Task.WhenAll(sendTasks);
}


async Task SendSafelyAsync(
    TcpClient tcpClient,
    ClientConnection connection,
    string json)
{
    try
    {
        await connection.SendJsonAsync(json);
    }
    catch
    {
        clients.TryRemove(tcpClient, out _);
        connection.Dispose();
        tcpClient.Close();
    }
}


ServerMessage ErrorMessage(string text)
{
    return new ServerMessage
    {
        Type = "error",
        Text = text
    };
}


List<TaskItem> LoadTasks()
{
    if (!File.Exists(dataFile))
    {
        return new List<TaskItem>();
    }

    try
    {
        string json = File.ReadAllText(dataFile);

        return JsonSerializer.Deserialize<List<TaskItem>>(
            json,
            jsonOptions
        ) ?? new List<TaskItem>();
    }
    catch
    {
        Console.WriteLine("Could not read tasks.json. Starting with an empty list.");
        return new List<TaskItem>();
    }
}


void SaveTasks()
{
    string json = JsonSerializer.Serialize(
        tasks,
        new JsonSerializerOptions { WriteIndented = true }
    );

    File.WriteAllText(dataFile, json);
}


TaskItem CloneTask(TaskItem task)
{
    return new TaskItem
    {
        Id = task.Id,
        Title = task.Title,
        Description = task.Description,
        Status = task.Status
    };
}


class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "Нове";
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


class ClientConnection : IDisposable
{
    private readonly StreamWriter writer;
    private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);

    public ClientConnection(StreamWriter writer)
    {
        this.writer = writer;
    }

    public async Task SendAsync(ServerMessage message)
    {
        string json = JsonSerializer.Serialize(message);
        await SendJsonAsync(json);
    }

    public async Task SendJsonAsync(string json)
    {
        await sendLock.WaitAsync();

        try
        {
            await writer.WriteLineAsync(json);
        }
        finally
        {
            sendLock.Release();
        }
    }

    public void Dispose()
    {
        sendLock.Dispose();
        writer.Dispose();
    }
}
