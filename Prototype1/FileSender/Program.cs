using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FileSender
{
    /// <summary>
    /// Using the pattern as found here: http://social.technet.microsoft.com/wiki/contents/articles/30957.c-windows-service-in-console-mode-for-debug-and-admin.aspx
    /// </summary>
    static class Program
    { 
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            { new FileWatcherService()};

            // In interactive mode ?
            if (Environment.UserInteractive)
            {
                // In debug mode ?
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // Simulate the services execution
                    RunInteractiveServices(ServicesToRun);
                }
                else
                {
                    try
                    {
                        bool hasCommands = false;
                        // Having an install command ?
                        if (HasCommand(args, "install") || HasCommand(args,"i"))
                        {
                            ManagedInstallerClass.InstallHelper(new String[] { typeof(Program).Assembly.Location });
                            hasCommands = true;
                        }
                        // Having an uninstall command ?
                        if (HasCommand(args, "uninstall") || HasCommand(args, "u"))
                        {
                            ManagedInstallerClass.InstallHelper(new String[] { "/u", typeof(Program).Assembly.Location });
                            hasCommands = true;
                        }
                        // Having a start command ?
                        if (HasCommand(args, "start"))
                        {
                            foreach (var service in ServicesToRun)
                            {
                                ServiceController sc = new ServiceController(service.ServiceName);
                                sc.Start();
                                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                            }
                            hasCommands = true;
                        }
                        // Having a stop command ?
                        if (HasCommand(args, "stop"))
                        {
                            foreach (var service in ServicesToRun)
                            {
                                ServiceController sc = new ServiceController(service.ServiceName);
                                sc.Stop();
                                sc.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                            }
                            hasCommands = true;
                        }
                        // If we don't have commands we print usage message
                        if (!hasCommands)
                        {
                            Console.WriteLine("Usage : {0} [command] [command ...]", Environment.GetCommandLineArgs());
                            Console.WriteLine("Commands : ");
                            Console.WriteLine(" - install or i : Install the services");
                            Console.WriteLine(" - uninstall or u : Uninstall the services");
                            Console.WriteLine(" - start : Start the services");
                            Console.WriteLine(" - stop : Stop the services");
                        }
                    }
                    catch (Exception ex)
                    {
                        var oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Error : {0}", ex.GetBaseException().Message);
                        Console.ForegroundColor = oldColor;
                    }
                }
            }
            else
            {
                // Normal service execution
                ServiceBase.Run(ServicesToRun);
            }
        }

        /// <summary>
        /// Run services in interactive mode
        /// </summary>
        static void RunInteractiveServices(ServiceBase[] servicesToRun)
        {
            Console.WriteLine();
            Console.WriteLine("Start the services in interactive mode.");
            Console.WriteLine();

            // Get the method to invoke on each service to start it
            MethodInfo onStartMethod = typeof(ServiceBase).GetMethod("OnStart", BindingFlags.Instance | BindingFlags.NonPublic);

            //Logger logger = new Logger();
            //logger.StartSimulator();
            //Console.WriteLine();
            //Console.WriteLine("Event viewer simulator starter.");

            // Start services loop
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Starting {0} ... ", service.ServiceName);
                onStartMethod.Invoke(service, new object[] { new string[] { } });
                Console.WriteLine("Started");
            }

            // Waiting the end
            Console.WriteLine();
            Console.WriteLine("Press a key to stop services et finish process...");
            Console.ReadKey();
            Console.WriteLine();

            // Get the method to invoke on each service to stop it
            MethodInfo onStopMethod = typeof(ServiceBase).GetMethod("OnStop", BindingFlags.Instance | BindingFlags.NonPublic);

            // Stop loop
            foreach (ServiceBase service in servicesToRun)
            {
                Console.Write("Stopping {0} ... ", service.ServiceName);
                onStopMethod.Invoke(service, null);
                Console.WriteLine("Stopped");
            }

            Console.WriteLine();
            Console.WriteLine("All services are stopped.");


            // Waiting a key press to not return to VS directly
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine();
                Console.Write("=== Press a key to quit ===");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Helper for check if we are a command in the command line arguments
        /// </summary>
        static bool HasCommand(String[] args, String command)
        {
            if (args == null || args.Length == 0 || String.IsNullOrWhiteSpace(command)) return false;
            return args.Any(a => String.Equals(a, command, StringComparison.OrdinalIgnoreCase));
        }

    }
}
