using System.Reflection;
using PluginBase;

class Program
{
    static void Main()
    {
        string pluginsFolder = Path.Combine(
            AppContext.BaseDirectory,
            "Plugins");

        if (!Directory.Exists(pluginsFolder))
        {
            Directory.CreateDirectory(pluginsFolder);
        }

        string[] dllFiles = Directory.GetFiles(
            pluginsFolder,
            "*.dll");

        List<IPlugin> plugins = new List<IPlugin>();

        foreach (string dllFile in dllFiles)
        {
            try
            {
                Assembly assembly =
                    Assembly.LoadFrom(dllFile);

                Type[] types = assembly.GetTypes();

                foreach (Type type in types)
                {
                    if (typeof(IPlugin).IsAssignableFrom(type)
                        && !type.IsInterface
                        && !type.IsAbstract)
                    {
                        IPlugin? plugin =
                            Activator.CreateInstance(type)
                            as IPlugin;

                        if (plugin != null)
                        {
                            plugins.Add(plugin);

                            Console.WriteLine(
                                $"Found plugin: {plugin.Name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error loading {dllFile}: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Executing plugins...");
        Console.WriteLine();

        foreach (IPlugin plugin in plugins)
        {
            Console.WriteLine($"[{plugin.Name}]");

            plugin.Execute();

            Console.WriteLine();
        }
    }
}