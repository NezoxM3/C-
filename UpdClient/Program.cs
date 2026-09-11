using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Text;

int port = 5000;
if (args.Length > 0 && int.TryParse(args[0], out int parsedPort))
{
    port = parsedPort;
}

double currentCpuPercent = 0;
object cpuLock = new();
using CancellationTokenSource cts = new();

Task cpuMonitorTask = Task.Run(async () =>
{
    while (!cts.IsCancellationRequested)
    {
        double value = await GetCpuUsagePercentAsync();
        lock (cpuLock)
        {
            currentCpuPercent = value;
        }

        try
        {
            await Task.Delay(1000, cts.Token);
        }
        catch (TaskCanceledException)
        {

        }
    }
});

using UdpClient udpServer = new(port);
Console.WriteLine($"UDP сервер запущено на порту {port}.");
Console.WriteLine("Очікування повідомлень від клієнтів... (Ctrl+C — зупинити)");

while (true)
{
    UdpReceiveResult result;
    try
    {
        result = await udpServer.ReceiveAsync();
    }
    catch (ObjectDisposedException)
    {
        break;
    }

    string message = Encoding.UTF8.GetString(result.Buffer).Trim();

    double cpuNow;
    lock (cpuLock)
    {
        cpuNow = currentCpuPercent;
    }

    Console.WriteLine();
    Console.WriteLine($"Клієнт {result.RemoteEndPoint}");
    Console.WriteLine($"Запит: {message}");
    Console.WriteLine($"CPU: {cpuNow:0}%");

    byte[] responseBytes = Encoding.UTF8.GetBytes($"{cpuNow:0}%");
    await udpServer.SendAsync(responseBytes, responseBytes.Length, result.RemoteEndPoint);
}

async Task<double> GetCpuUsagePercentAsync()
{
    try
    {
        if (OperatingSystem.IsMacOS())
        {
            return await GetMacCpuUsageAsync();
        }

        if (OperatingSystem.IsLinux())
        {
            return await GetLinuxCpuUsageAsync();
        }

        return GetFallbackCpuUsage();
    }
    catch
    {
        return GetFallbackCpuUsage();
    }
}

async Task<double> GetMacCpuUsageAsync()
{
    ProcessStartInfo psi = new("top")
    {
        RedirectStandardOutput = true,
        UseShellExecute = false,
        CreateNoWindow = true
    };
    psi.ArgumentList.Add("-l");
    psi.ArgumentList.Add("1");
    psi.ArgumentList.Add("-n");
    psi.ArgumentList.Add("0");

    using Process? process = Process.Start(psi);
    if (process is null)
    {
        return GetFallbackCpuUsage();
    }

    string output = await process.StandardOutput.ReadToEndAsync();
    await process.WaitForExitAsync();

    foreach (string rawLine in output.Split('\n'))
    {
        string trimmed = rawLine.Trim();
        if (!trimmed.StartsWith("CPU usage:", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        foreach (string token in trimmed.Split(','))
        {
            if (!token.Contains("idle", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string digits = new(token.Where(c => char.IsDigit(c) || c == '.').ToArray());
            if (double.TryParse(digits, NumberStyles.Float, CultureInfo.InvariantCulture, out double idle))
            {
                return Math.Clamp(100 - idle, 0, 100);
            }
        }
    }

    return GetFallbackCpuUsage();
}

async Task<double> GetLinuxCpuUsageAsync()
{
    (long idle1, long total1) = ReadProcStat();
    await Task.Delay(200);
    (long idle2, long total2) = ReadProcStat();

    long idleDelta = idle2 - idle1;
    long totalDelta = total2 - total1;

    if (totalDelta <= 0)
    {
        return GetFallbackCpuUsage();
    }

    return Math.Clamp(100.0 * (1.0 - (double)idleDelta / totalDelta), 0, 100);
}

(long idle, long total) ReadProcStat()
{
    string line = File.ReadLines("/proc/stat").First(l => l.StartsWith("cpu "));
    long[] values = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Skip(1)
        .Select(long.Parse)
        .ToArray();

    long idle = values[3];
    long total = values.Sum();
    return (idle, total);
}

double GetFallbackCpuUsage()
{
    TimeSpan cpuTime = Process.GetCurrentProcess().TotalProcessorTime;
    return Math.Round(cpuTime.TotalMilliseconds % 100, 0);
}
