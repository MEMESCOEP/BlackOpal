/* [===== MAIN KERNEL CLASS =====] */
// Note: This kernel currently requires 99 MB of RAM to boot when using GZIP compression, but can run on as little 2MB (GUI) / 512k (CMD) AFTER booting.
// If GZIP isn't being used, 64 MB is the minimum required to boot.
// I believe this is because the entire kernel is getting decompressed into RAM, but it might not be.
// After booting, the OS uses around 300-350K in console mode, and about 2.05MB in GUI mode.
// Using GZIP compresses the ISO image, at the expense of higher memory requirements

/* DIRECTIVES */
using Sys = Cosmos.System;
using Cosmos.System.Network.Config;
using Cosmos.Core.Memory;
using Cosmos.Core;
using System.Threading;
using System.IO;
using System;
using IO;
using System.Runtime.ConstrainedExecution;

/* NAMESPACES */
namespace CosmosOS_Learning
{
    /* CLASSES */
    public class Kernel : Sys.Kernel
    {
        /* WARNING SUPRESSION */
        #pragma warning disable CA1416 // Validate platform compatibility

        /* VARIABLES */
        public static DateTime KernelStartTime;
        public static float TotalInstalledRAM = 0f;
        public static float UsedRAM = 0f;
        public const string OSName = "Andrew Maney's Research Kernel";
        public const string OSVersion = "0.0.1";
        public const string CMDPrompt = ">> ";
        public Sys.FileSystem.CosmosVFS fs;

        /* FUNCTIONS */
        // This function gets called immediately upon kernel startup.
        protected override void BeforeRun()
        {
            try
            {
                // Set the cursor's height to be a full block
                Console.CursorSize = 100;

                // Get the start time of the kernel
                KernelStartTime = DateTime.Now;

                // Print a boot message
                ConsoleFunctions.PrintLogMSG($"Kernel started at {KernelStartTime.ToString()}\n", ConsoleFunctions.LogType.INFO);

                // Get the total amount of installed RAM in the computer
                TotalInstalledRAM = CPU.GetAmountOfRAM() * 1024f;

                // Create and register the virtual filesystem object
                ConsoleFunctions.PrintLogMSG("Creating and registering the virtual filesystem...\n", ConsoleFunctions.LogType.INFO);
                try
                {
                    fs = new();
                    Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);

                    // Set the current working directory (if we can)
                    if (fs.Disks.Count > 0)
                    {
                        ConsoleFunctions.PrintLogMSG("Setting the current working directory...\n", ConsoleFunctions.LogType.INFO);
                        foreach (var disk in fs.Disks)
                        {
                            if (disk.Partitions.Count > 0)
                            {
                                if (disk.Partitions[0].HasFileSystem && String.IsNullOrEmpty(disk.Partitions[0].RootPath) == false)
                                {
                                    Directory.SetCurrentDirectory(disk.Partitions[0].RootPath);
                                    ConsoleFunctions.PrintLogMSG($"Working directory is: \"{disk.Partitions[0].RootPath})\"\n", ConsoleFunctions.LogType.INFO);
                                    break;
                                }
                            }

                            else
                            {
                                ConsoleFunctions.PrintLogMSG("The main disk doesn't have a root path.", ConsoleFunctions.LogType.WARNING);
                            }
                        }
                    }
                    else
                    {
                        ConsoleFunctions.PrintLogMSG("No functioning FAT32/UDF formatted disks are installed, You won't be able to save anything.\n\nPress any key to continue.", ConsoleFunctions.LogType.WARNING);
                        Console.ReadKey();
                    }
                }
                catch(Exception ex)
                {
                    ConsoleFunctions.PrintLogMSG($"Disk init error: {ex.Message}\n\nPress any key to continue.", ConsoleFunctions.LogType.ERROR);
                    Console.ReadKey();
                }

                // Collect any garbage that we created
                ConsoleFunctions.PrintLogMSG("Calling the garbage collector...\n", ConsoleFunctions.LogType.INFO);
                Heap.Collect();

                // Clear the console
                Console.Clear();

                // Print the OS name and version
                ConsoleFunctions.PrintLogMSG($"[== {OSName.ToUpper()} ({OSVersion}) ==]\nKernel started at {KernelStartTime.ToString()}.\nTerminal size: {Console.WindowWidth}x{Console.WindowHeight}\n\n", ConsoleFunctions.LogType.NONE);
            }
            catch (Exception ex)
            {
                ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\nPress any key to reboot.", ConsoleFunctions.LogType.FATAL);
                Console.ReadKey();
                Sys.Power.Reboot();
            }
        }

        // This function is called infinitely, and is run after "BeforeRun"
        // This will be known as the main loop
        protected override void Run()
        {
            // Print the prompt and handle a command
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"({Directory.GetCurrentDirectory()}) {CMDPrompt}");
            Console.ForegroundColor = ConsoleColor.White;
            var input = Console.ReadLine().Split(' ');

            // Handle a command
            HandleCommand(input[0], input);
        }

        // Handle a command
        private void HandleCommand(string command, string[] arguments)
        {
            // Trim all whitespace from the start of the command
            command = command.TrimStart();

            switch (command)
            {
                // ** GUI **
                case "gui":
                    GUI gui = new();
                    gui.Init();
                    break;

                // ** CONSOLE **
                case "clear":
                case "cls":
                    Console.Clear();
                    break;

                // ** NETWORK **
                // Use NTP to set the current date/time
                case "ntp":
                    try
                    {
                        Console.WriteLine(NTPClient.GetNetworkTime().ToString());
                    }
                    catch (Exception ex)
                    {
                        ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Get NIC information
                case "nicinfo":
                    if (Networking.IsConfigured() == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"A NIC must be properly configured before networking can be used.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    foreach(var NIC in Cosmos.HAL.NetworkDevice.Devices)
                    {
                        Console.WriteLine($"[== {NIC.Name} ({NIC.NameID}) ==]\nMAC: {NIC.MACAddress.ToString()}\nIs ready: {NIC.Ready.ToString()}\n");
                    }

                    break;

                // Ping a device on a network using it's IPv4 address
                case "ping":
                    if (Networking.IsConfigured() == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"A NIC must be properly configured before networking can be used.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"An IP address must be specified.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (Networking.IsIPv4AddressValid(arguments[1]) == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"\"{arguments[1]}\" is not a valid IP address.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    Console.WriteLine($"Pinging {arguments[1]}:");
                    int SuccessCounter = 0;

                    for (int i = 0; i < 4; i++)
                    {
                        float PingTime = Networking.ICMPPing(Networking.StringToAddress(arguments[1]));

                        if(PingTime >= 0)
                        {
                            SuccessCounter++;
                            Console.WriteLine($"\t[{i + 1}] Ping succeeded in {PingTime} milliseconds.");
                            Thread.Sleep(250);
                        }

                        else
                        {
                            Console.WriteLine($"\tPing failed.");
                        }
                    }

                    Console.WriteLine($"{SuccessCounter}/4 pings succeeded. ({(float)(SuccessCounter / 4f) * 100f}%)\n");

                    break;

                // Prnt network information
                case "netinfo":
                    if (Networking.IsConfigured() == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"A NIC must be properly configured before networking can be used.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    foreach (var config in NetworkConfiguration.NetworkConfigs)
                    {
                        Console.WriteLine($"[== {config.Device.NameID} ==]\n" +
                                    $"IP: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress.ToString()}\n" +
                                    $"SUBNET: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.SubnetMask.ToString()}\n" +
                                    $"DEFAULT GATEWAY: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.DefaultGateway.ToString()}\n" +
                                    $"MAC: {NetworkConfiguration.CurrentNetworkConfig.Device.MACAddress.ToString()}\n" +
                                    $"DEVICE NAME: {NetworkConfiguration.CurrentNetworkConfig.Device.Name.ToString()}\n");
                    }

                    break;

                // Initialize the NIC
                case "netinit":
                    if (arguments.Length >= 1)
                    {
                        if (arguments[1] == "dhcp")
                        {
                            ConsoleFunctions.PrintLogMSG("Attempting to obtain an IPv4 address via DHCP...\n", ConsoleFunctions.LogType.INFO);
                            if (Networking.DHCPAutoconfig() == false)
                            {
                                ConsoleFunctions.PrintLogMSG("DHCP autoconfiguration failed.\n", ConsoleFunctions.LogType.ERROR);
                            }

                            // If DHCP worked, print the configuration information
                            if (Networking.IsConfigured())
                            {
                                Console.WriteLine($"[== NET CONFIG ==]\n" +
                                    $"IP: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress.ToString()}\n" +
                                    $"SUBNET: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.SubnetMask.ToString()}\n" +
                                    $"DEFAULT GATEWAY: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.DefaultGateway.ToString()}\n" +
                                    $"MAC: {NetworkConfiguration.CurrentNetworkConfig.Device.MACAddress.ToString()}\n" +
                                    $"DEVICE NAME: {NetworkConfiguration.CurrentNetworkConfig.Device.Name.ToString()}\n" +
                                    $"ID: {NetworkConfiguration.CurrentNetworkConfig.Device.NameID.ToString()}\n");
                            }
                            else
                            {
                                ConsoleFunctions.PrintLogMSG("DHCP autoconfiguration failed: The device is not ready or an invalid IP address was assigned.\n\n", ConsoleFunctions.LogType.ERROR);
                            }
                        }
                    }

                    break;

                // ** FILESYSTEM **
                // Make a file
                case "mkf":
                    var newFileName = "";

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (arguments[1].StartsWith("\""))
                    {
                        newFileName = "";

                        foreach (var part in arguments)
                        {
                            newFileName += part + " ";
                        }

                        // Get the file name inside of a the quotes
                        int pFrom = newFileName.IndexOf("\"") + 1;
                        int pTo = newFileName.LastIndexOf("\"");
                        newFileName = newFileName.Substring(pFrom, pTo - pFrom);
                        newFileName.Replace($"{command} ", "").TrimEnd();
                    }
                    else
                    {
                        newFileName = arguments[1];
                    }

                    if (File.Exists(newFileName) == true)
                    {
                        ConsoleFunctions.PrintLogMSG($"The file \"{newFileName}\" already exists.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    newFileName = Path.GetFullPath(newFileName);
                    fs.CreateFile(newFileName);

                    break;

                // Make a directory
                // Make a file
                case "mkdir":
                    var newDirName = "";

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (arguments[1].StartsWith("\""))
                    {
                        newDirName = "";

                        foreach (var part in arguments)
                        {
                            newDirName += part + " ";
                        }

                        // Get the directory name inside of a the quotes
                        int pFrom = newDirName.IndexOf("\"") + 1;
                        int pTo = newDirName.LastIndexOf("\"");
                        newDirName = newDirName.Substring(pFrom, pTo - pFrom);
                        newDirName.Replace($"{command} ", "").TrimEnd();
                    }
                    else
                    {
                        newDirName = arguments[1];
                    }

                    if (Directory.Exists(newDirName) == true)
                    {
                        ConsoleFunctions.PrintLogMSG($"The directory \"{newDirName}\" already exists.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    newFileName = Path.GetFullPath(newDirName);
                    fs.CreateDirectory(newDirName);

                    break;

                // Delete a file or directory
                case "rm":
                    var newPathName = "";
                    bool deleteDir = false;

                    foreach(var arg in arguments)
                    {
                        if (arg == "-rf")
                        {
                            deleteDir = true;
                            break;
                        }
                    }

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A file or directory name must be specified.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (arguments[1].StartsWith("\""))
                    {
                        newPathName = "";

                        foreach (var part in arguments)
                        {
                            newPathName += part + " ";
                        }

                        // Get the path inside of a the quotes
                        int pFrom = newPathName.IndexOf("\"") + 1;
                        int pTo = newPathName.LastIndexOf("\"");
                        newPathName = newPathName.Substring(pFrom, pTo - pFrom);
                        newPathName.Replace($"{command} ", "").TrimEnd();
                    }
                    else
                    {
                        newPathName = arguments[1];
                    }

                    newPathName = Path.GetFullPath(newPathName);

                    if (File.Exists(newPathName))
                    {
                        File.Delete(newPathName);
                    }
                    else if (Directory.Exists(newPathName) && deleteDir)
                    {
                        Directory.Delete(newPathName);
                    }
                    else
                    {
                        ConsoleFunctions.PrintLogMSG($"The item \"{newPathName}\" doesn't exist.\n\n", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Get information about each installed (and functioning) disk
                case "diskinfo":
                    foreach(var disk in fs.Disks)
                    {
                        Console.WriteLine($"[== DISK #{fs.Disks.IndexOf(disk)} ==]");
                        disk.DisplayInformation();
                        Console.WriteLine($"Root path: {disk.Partitions[0].RootPath}\n");
                    }

                    Console.WriteLine();
                    break;

                // Change the current working directory
                case "cd":
                    var dir = "";

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (arguments[1].StartsWith("\""))
                    {
                        dir = "";

                        foreach (var part in arguments)
                        {
                            dir += part + " ";
                        }

                        // Get the directory name inside of a the quotes
                        int pFrom = dir.IndexOf("\"") + 1;
                        int pTo = dir.LastIndexOf("\"");
                        dir = dir.Substring(pFrom, pTo - pFrom);
                        dir.Replace($"{command} ", "").TrimEnd();
                    }
                    else
                    {
                        dir = arguments[1];
                    }

                    if (Directory.Exists(dir) == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"The directory \"{dir}\" does not exist.\n\n", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    dir = Path.GetFullPath(dir);
                    Directory.SetCurrentDirectory(dir);

                    break;

                // Get a directory listing
                case "ls":
                    foreach (var item in fs.GetDirectoryListing(Directory.GetCurrentDirectory()))
                    {
                        if (File.Exists(item.mFullPath))
                            Console.WriteLine($"[FILE] {item.mName}");

                        else if (Directory.Exists(item.mFullPath))
                            Console.WriteLine($"[DIR] {item.mName}");
                    }

                    Console.WriteLine();
                    break;

                // Print a file's contents to the console
                case "cat":
                    try
                    {
                        if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n", ConsoleFunctions.LogType.ERROR);
                        }

                        else if (File.Exists(Path.GetFullPath(arguments[1])) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{arguments[1]}\" does not exist.\n\n", ConsoleFunctions.LogType.ERROR);
                        }

                        else
                        {
                            var contents = File.ReadAllText(Path.GetFullPath(arguments[1]));
                            Console.WriteLine($"{contents}\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // ** POWER **
                // Shut down the computer
                case "shutdown":
                case "poweroff":
                case "powerdown":
                case "turnoff":
                    Sys.Power.Shutdown();
                    break;

                // Reboot the computer
                case "reboot":
                case "restart":
                    Sys.Power.Reboot();
                    break;

                // ** SYSTEM **
                // Get RAM information
                case "raminfo":
                    UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                    Console.WriteLine($"[== RAM INFORMATION ==]\nRAM: {Kernel.TotalInstalledRAM} KB\n" +
                        $"USED: {UsedRAM}/{Kernel.TotalInstalledRAM} KB ({(UsedRAM / Kernel.TotalInstalledRAM) * 100}%)\n");
                    break;

                // Get system information
                case "sysinfo":
                    UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                    Console.WriteLine($"[== SYSTEM INFORMATION ==]\nCPU: {CPU.GetCPUBrandString()}\n" +
                        $"CPU Uptime: {CPU.GetCPUUptime()}\n" +
                        $"CPU Vendor: {CPU.GetCPUVendorName()}\n" +
                        $"RAM: {Kernel.TotalInstalledRAM} KB\n" +
                        $"USED RAM: {UsedRAM}/{Kernel.TotalInstalledRAM} KB ({(UsedRAM / Kernel.TotalInstalledRAM) * 100}%)\n" +
                        $"System uptime: {DateTime.Now - KernelStartTime}\n");
                    break;

                // ** EXTRA **
                // Empty command
                case "":
                    break;

                // Invalid command
                default:
                    ConsoleFunctions.PrintLogMSG($"Invalid command: \"{command}\"\n\n", ConsoleFunctions.LogType.ERROR);
                    break;
            }

            // Collect any garbage that we created. This helps prevent memory leaks, which can cause the computer
            // to run out of memory, which leads to crashes. Real OSes such as Windows solve this by using "swap", which
            // is a partition or file that acts as extra memory and is stored on the hard disk. I haven't implemented
            // this yet because I'm focusing on getting the core functionality implemented first.
            Heap.Collect();
        }
    }
}
