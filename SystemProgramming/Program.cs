using System.Diagnostics;

class Program
{
    static void Main()
    {
        while (true)
        {
            Console.Clear();

            Console.WriteLine("===== SYSTEM PROGRAMMING =====");
            Console.WriteLine("1. Kill Safari every second");
            Console.WriteLine("2. Show running processes");
            Console.WriteLine("3. Open browser with Google search");
            Console.WriteLine("0. Exit");
            Console.Write("\nChoose an option: ");

            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    KillSafari();
                    break;

                case "2":
                    ShowProcesses();
                    break;

                case "3":
                    OpenBrowser();
                    break;

                case "0":
                    return;

                default:
                    Console.WriteLine("Invalid option.");
                    Pause();
                    break;
            }
        }
    }

    // Task 1
    static void KillSafari()
    {
        Console.Clear();

        Console.WriteLine("===== TASK 1 =====");
        Console.WriteLine("Safari will be checked every second.");
        Console.WriteLine("Press Ctrl+C to stop the program.\n");

        while (true)
        {
            Process[] processes = Process.GetProcessesByName("Safari");

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    try
                    {
                        Console.WriteLine(
                            $"Safari found. PID: {process.Id}. Stopping process...");

                        process.Kill();
                        process.WaitForExit();

                        Console.WriteLine("Safari stopped.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Could not stop Safari: {ex.Message}");
                    }
                }
            }
            else
            {
                Console.WriteLine("Safari is not running.");
            }

            Thread.Sleep(1000);
        }
    }

    // Task 2
    static void ShowProcesses()
    {
        Console.Clear();

        Console.WriteLine("===== TASK 2 =====");
        Console.WriteLine("Running processes:\n");

        Process[] processes = Process.GetProcesses();

        foreach (Process process in processes.OrderBy(p => p.ProcessName))
        {
            try
            {
                Console.WriteLine(
                    $"PID: {process.Id,-8} | Name: {process.ProcessName}");
            }
            catch
            {
                // Some system processes may not allow access to their information.
            }
        }

        Console.Write("\nEnter PID of the process to stop: ");

        if (!int.TryParse(Console.ReadLine(), out int pid))
        {
            Console.WriteLine("Invalid PID.");
            Pause();
            return;
        }

        try
        {
            Process? selectedProcess = Process.GetProcessById(pid);

            Console.WriteLine(
                $"\nSelected process: {selectedProcess.ProcessName}");

            Console.Write("Are you sure you want to stop it? (y/n): ");

            string? confirmation = Console.ReadLine();

            if (confirmation?.ToLower() == "y")
            {
                selectedProcess.Kill();
                selectedProcess.WaitForExit();

                Console.WriteLine("Process stopped successfully.");
            }
            else
            {
                Console.WriteLine("Operation cancelled.");
            }
        }
        catch (ArgumentException)
        {
            Console.WriteLine("Process with this PID was not found.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not stop process: {ex.Message}");
        }

        Pause();
    }

    // Task 3
    static void OpenBrowser()
    {
        Console.Clear();

        Console.WriteLine("===== TASK 3 =====");
        Console.WriteLine("Available browsers:\n");

        List<(string Name, string Path)> browsers = new();

        AddBrowserIfExists(
            browsers,
            "Safari",
            "/Applications/Safari.app");

        AddBrowserIfExists(
            browsers,
            "Google Chrome",
            "/Applications/Google Chrome.app");

        AddBrowserIfExists(
            browsers,
            "Firefox",
            "/Applications/Firefox.app");

        AddBrowserIfExists(
            browsers,
            "Microsoft Edge",
            "/Applications/Microsoft Edge.app");

        AddBrowserIfExists(
            browsers,
            "Brave",
            "/Applications/Brave Browser.app");

        if (browsers.Count == 0)
        {
            Console.WriteLine("No supported browsers were found.");
            Pause();
            return;
        }

        for (int i = 0; i < browsers.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {browsers[i].Name}");
        }

        Console.Write("\nChoose a browser: ");

        if (!int.TryParse(Console.ReadLine(), out int choice))
        {
            Console.WriteLine("Invalid choice.");
            Pause();
            return;
        }

        if (choice < 1 || choice > browsers.Count)
        {
            Console.WriteLine("Invalid browser number.");
            Pause();
            return;
        }

        string browserPath = browsers[choice - 1].Path;

        string url =
            "https://www.google.com/search?q=C%23";

        try
        {
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = $"-a \"{browserPath}\" \"{url}\"",
                    UseShellExecute = false
                });

            Console.WriteLine(
                $"\nOpening {browsers[choice - 1].Name}...");

            Console.WriteLine(
                "Google search for C# will be opened.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"Could not open browser: {ex.Message}");
        }

        Pause();
    }

    static void AddBrowserIfExists(
        List<(string Name, string Path)> browsers,
        string name,
        string path)
    {
        if (Directory.Exists(path))
        {
            browsers.Add((name, path));
        }
    }

    static void Pause()
    {
        Console.WriteLine("\nPress Enter to continue...");
        Console.ReadLine();
    }
}