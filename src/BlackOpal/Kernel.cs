/* [===== MAIN KERNEL CLASS =====] */
// Note: This kernel currently requires 103 MB of RAM to boot when using GZIP compression, but can run on as little 3.81MB (VMSVGA) / 12.62MB (VGA) / 512k (CMD) AFTER booting.
// If GZIP isn't being used, 100 MB is the minimum required to boot.
// I believe this is because the entire kernel is getting decompressed into RAM, but this might not be the case.
// After booting, the OS uses around 300-350K in console mode, and about 3.81MB (VMSVGA) / 12.62MB (VGA) in GUI mode.
// Using GZIP compresses the ISO image, at the expense of higher memory requirements

/* DIRECTIVES */
using Cosmos.System.Network.Config;
using Cosmos.Core.Memory;
using Cosmos.Core;
using Cosmos.HAL;
using IL2CPU.API.Attribs;
using System.Threading;
using System.IO;
using System;
using IO.Networking;
using IO.CMD;
using GUI;
using BlackOpal.Calculations;
using SVGAIITerminal.TextKit;
using PrismAPI.Graphics;
using HydrixLIB;
using Sys = Cosmos.System;

/* NAMESPACES */
namespace BlackOpal
{
    /* CLASSES */
    public class Kernel : Sys.Kernel
    {
        /* WARNING SUPRESSION */
        #pragma warning disable CA1416 // Validate platform compatibility

        /* VARIABLES */
        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.Fonts.Terminal.btf")]
        static byte[] TTFFont;

        public const string OSContributors = "Scarlet & Azure";
        public const string OSVersion = "0.0.3";
        public const string OSAuthor = "memescoep";
        public const string OSName = "Black Opal";
        public const string OSDate = "2-26-2024";
        public static SVGAIITerminal.SVGAIITerminal Terminal = new SVGAIITerminal.SVGAIITerminal(UserInterface.ScreenWidth, UserInterface.ScreenHeight, new BtfFontFace(TTFFont, 16));
        public static TextScreenBase TextScreen;
        public static HTerminal HTerminal = new HTerminal(Terminal);
        public static DateTime KernelStartTime;
        public static Color TerminalColor = Color.Green;
        public static string HydrixLibVersion = HTerminal.GetHydrixLibVersion();
        public static float TotalInstalledRAM = 0f;
        public static float UsedRAM = 0f;
        public string CMDPrompt = ">>";
        public string Username = "root";
        public string Hostname = "BlackOpal";
        public Sys.FileSystem.CosmosVFS fs; 

        /* FUNCTIONS */
        // Perform initialization and configuration
        protected override void BeforeRun()
        {
            try
            {
                // Get the start time of the kernel
                KernelStartTime = DateTime.Now;

                // Initialize the terminal
                ConsoleFunctions.PrintLogMSG("Configuring terminal...\n\r", ConsoleFunctions.LogType.INFO);
                Terminal.CursorShape = SVGAIITerminal.CursorShape.Block;
                Terminal.ForegroundColor = TerminalColor;
                Terminal.SetCursorPosition(0, 0);
                Terminal.Clear();

                // Print a boot message
                ConsoleFunctions.PrintLogMSG($"Kernel started at {KernelStartTime.ToString()}\n\r", ConsoleFunctions.LogType.INFO);
                ConsoleFunctions.PrintLogMSG($"HydrixLIB Version: {HydrixLibVersion}\n\r", ConsoleFunctions.LogType.INFO);

                // Commented out because it causes crashes (I probably fucked it up lmao)
                // Zero memory so the system starts in a known state
                /*for (uint i = 512; i < RAT.RamSize; i += 512)
                {
                    ConsoleFunctions.PrintLogMSG($"Zeroing memory block {CPU.GetEndOfKernel() + i} -> {CPU.GetEndOfKernel() + i + 512}...\n\r", ConsoleFunctions.LogType.INFO);
                    CPU.ZeroFill(CPU.GetEndOfKernel() + i, 512);
                }*/

                // Set the keyboard layout (this may help with some keyboards acting funky)
                ConsoleFunctions.PrintLogMSG($"Setting keyboard layout...\n\r", ConsoleFunctions.LogType.INFO);
                Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.USStandardLayout());

                // Get the total amount of installed RAM in the computer
                TotalInstalledRAM = CPU.GetAmountOfRAM() * 1024f;

                // Create and register the virtual filesystem object
                ConsoleFunctions.PrintLogMSG("Creating and registering the virtual filesystem...\n\r", ConsoleFunctions.LogType.INFO);
                
                try
                {
                    fs = new();
                    Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);

                    // Set the current working directory (if we can)
                    if (fs.Disks.Count > 0)
                    {
                        ConsoleFunctions.PrintLogMSG("Setting the current working directory...\n\r", ConsoleFunctions.LogType.INFO);
                        foreach (var disk in fs.Disks)
                        {
                            if (disk.Partitions.Count > 0)
                            {
                                if (disk.Partitions[0].HasFileSystem && String.IsNullOrEmpty(disk.Partitions[0].RootPath) == false)
                                {
                                    Directory.SetCurrentDirectory(disk.Partitions[0].RootPath);
                                    ConsoleFunctions.PrintLogMSG($"Working directory is: \"{disk.Partitions[0].RootPath}\"\n\r", ConsoleFunctions.LogType.INFO);
                                    break;
                                }
                            }

                            else
                            {
                                ConsoleFunctions.PrintLogMSG("The main disk doesn't have a root path.\n\r", ConsoleFunctions.LogType.WARNING);
                            }
                        }
                    }
                    else
                    {
                        ConsoleFunctions.PrintLogMSG("No functioning FAT32/UDF formatted disks are installed, You won't be able to save anything.\n\n\rPress any key to continue.", ConsoleFunctions.LogType.WARNING);
                        Terminal.ReadKey();
                    }
                }
                catch(Exception ex)
                {
                    ConsoleFunctions.PrintLogMSG($"Disk init error: {ex.Message}\n\n\rPress any key to continue.", ConsoleFunctions.LogType.ERROR);
                    Terminal.ReadKey();
                }

                // Collect any garbage that we created
                ConsoleFunctions.PrintLogMSG("Calling the garbage collector...\n\r", ConsoleFunctions.LogType.INFO);
                Heap.Collect();

                // Clear the console
                Terminal.Clear();

                // Print the OS name and version
                ConsoleFunctions.PrintLogMSG($"{OSName} {OSVersion} - {OSDate}\n\r" +
                    $"By: {OSAuthor}, with help from {OSContributors}.\n\r" +
                    $"Kernel started at {KernelStartTime.ToString()} (BTR took {(DateTime.Now.Second - KernelStartTime.Second).ToString()} seconds).\n\r" +
                    $"Terminal size: {Terminal.Width}x{Terminal.Height} ({UserInterface.ScreenWidth}x{UserInterface.ScreenHeight})\n\n\r", ConsoleFunctions.LogType.NONE);
            }
            catch (Exception ex)
            {
                KernelPanic.Panic(ex.Message, ex.HResult.ToString());
            }
        }

        // This function is called infinitely, and is run after "BeforeRun"
        // This will be known as the main loop
        protected override void Run()
        {
            try
            {
                // Print the prompt                
                HTerminal.ColoredWrite($"{Username}", Color.Magenta);
                HTerminal.ColoredWrite("@", Color.White);
                HTerminal.ColoredWrite($"{Hostname}", Color.Cyan);
                HTerminal.ColoredWrite($" ({Directory.GetCurrentDirectory()}) ", Color.Green);
                HTerminal.ColoredWrite($"{CMDPrompt} ", Color.SuperOrange);
                Terminal.ForegroundColor = Color.StackOverflowWhite;

                // Get user input
                var input = Terminal.ReadLine();

                // Handle a command
                var arglist = input.Split(' ');

                HandleCommand(arglist[0], arglist);
            }
            catch (Exception EX)
            {
                KernelPanic.Panic(EX.Message, EX.HResult.ToString());
            }
        }

        // Handle a command
        private void HandleCommand(string command, string[] arguments)
        {
            // Trim all whitespace from the start of the command
            command = command.TrimStart();

            // Handle the command
            switch (command)
            {
                // ** GRAPHICS **
                case "gui":
                    ConsoleFunctions.PrintLogMSG("Initializing GUI...\n\r", ConsoleFunctions.LogType.INFO);

                    UserInterface gui = new();
                    gui.Init();
                    break;

                // ** CONSOLE **
                case "clear":
                case "cls":
                    Terminal.Clear();
                    break;

                case "help":
                    //check if there are any arguments
                    if (arguments.Length == 1)
                    {
                        /// #############################
                        /// Azureian: Please try to keep each header in the help message the same length, it looks better, thanks! (Optionally, you can make it divisible by 2 and add/remove '-' to both sides)
                        /// Current length: 8 characters
                        /// #############################
                        HTerminal.ColoredWrite("Black Opal Help\n", Color.Green);
                        HTerminal.ColoredWrite("Commands:\n", Color.Green);

                        // Write header Graphics
                        HTerminal.ColoredWriteLine("  ------------------------------GRAPHICS------------------------------", Color.GoogleBlue);
                        HTerminal.ColoredWrite("  gui - Start the GUI\n", Color.GoogleBlue);
                        HTerminal.ColoredWriteLine("  --------------------------------------------------------------------", Color.GoogleBlue);
                        Terminal.WriteLine();

                        HTerminal.ColoredWriteLine("  ------------------------------TERMINAL------------------------------", Color.GoogleGreen);
                        HTerminal.ColoredWrite("  clear/cls - Clear the console\n", Color.GoogleGreen);
                        HTerminal.ColoredWrite("  help - Display this help message\n", Color.GoogleGreen);
                        HTerminal.ColoredWrite("  echo - Print a message to the console\n", Color.GoogleGreen);
                        HTerminal.ColoredWrite("  power - Shut down or restart the computer\n", Color.GoogleGreen);
                        HTerminal.ColoredWrite("  sysinfo - Get system information\n", Color.GoogleGreen);
                        HTerminal.ColoredWrite("  raminfo - Get RAM information\n", Color.GoogleGreen);
                        HTerminal.ColoredWrite("  diskinfo - Get disk information\n", Color.GoogleGreen);
                        HTerminal.ColoredWriteLine("  --------------------------------------------------------------------", Color.GoogleGreen);
                        Terminal.WriteLine();

                        HTerminal.ColoredWriteLine("  ------------------------------INTERNET------------------------------", Color.GoogleYellow);
                        HTerminal.ColoredWrite("  netinfo - Get network information\n", Color.GoogleYellow);
                        HTerminal.ColoredWrite("  netinit - Initialize the NIC\n", Color.GoogleYellow);
                        HTerminal.ColoredWrite("  nicinfo - Get NIC information\n", Color.GoogleYellow);
                        HTerminal.ColoredWrite("  ping - Ping a device on the network\n", Color.GoogleYellow);
                        HTerminal.ColoredWrite("  ntp - Get the current date/time from an NTP server\n", Color.GoogleYellow);
                        HTerminal.ColoredWriteLine("  --------------------------------------------------------------------", Color.GoogleYellow);
                        Terminal.WriteLine();

                        HTerminal.ColoredWriteLine("  ------------------------------FILEMGMT------------------------------", Color.GoogleRed); //File Management
                        HTerminal.ColoredWrite("  mkf - Make a file\n", Color.GoogleRed);
                        HTerminal.ColoredWrite("  mkdir - Make a directory\n", Color.GoogleRed);
                        HTerminal.ColoredWrite("  rm - Delete a file or directory\n", Color.GoogleRed);
                        HTerminal.ColoredWrite("  cd - Change the current working directory\n", Color.GoogleRed);
                        HTerminal.ColoredWrite("  dir/ls - Get a directory listing\n", Color.GoogleRed);
                        HTerminal.ColoredWrite("  cat - Print a file's contents to the console\n", Color.GoogleRed);
                        HTerminal.ColoredWriteLine("  --------------------------------------------------------------------", Color.GoogleRed);
                        Terminal.WriteLine();

                        HTerminal.ColoredWriteLine("  ------------------------------DEBUGCMD------------------------------", Color.Magenta);
                        HTerminal.ColoredWrite("  panic - Force a kernel panic for debugging\n", Color.Magenta);
                        HTerminal.ColoredWriteLine("  --------------------------------------------------------------------", Color.Magenta);
                        Terminal.WriteLine();

                        HTerminal.ColoredWrite("Type 'help <command>' for more information on a specific command.\n\n", Color.Green);
                    }
                    else
                    {
                        // Check if the argument is a valid command
                        if (arguments[1] == "clear" || arguments[1] == "cls")
                        {
                            HTerminal.ColoredWrite("clear/cls - Clear the console\n", Color.Yellow);
                        }
                        else if (arguments[1] == "help")
                        {
                            HTerminal.ColoredWrite("help - Display this help message\n", Color.Yellow);
                        }
                        else if (arguments[1] == "echo")
                        {
                            HTerminal.ColoredWrite("echo - Print a message to the console\n", Color.Yellow);
                        }
                        else if (arguments[1] == "power")
                        {
                            HTerminal.ColoredWrite("power - Shut down or restart the computer\n", Color.Yellow);
                        }
                        else if (arguments[1] == "sysinfo")
                        {
                            HTerminal.ColoredWrite("sysinfo - Get system information\n", Color.Yellow);
                        }
                        else if (arguments[1] == "raminfo")
                        {
                            HTerminal.ColoredWrite("raminfo - Get RAM information\n", Color.Yellow);
                        }
                        else if (arguments[1] == "diskinfo")
                        {
                            HTerminal.ColoredWrite("diskinfo - Get disk information\n", Color.Yellow);
                        }
                        else if (arguments[1] == "netinfo")
                        {
                            HTerminal.ColoredWrite("netinfo - Get network information\n", Color.Yellow);
                        }
                        else if (arguments[1] == "netinit")
                        {
                            HTerminal.ColoredWrite("netinit - Initialize the NIC\n", Color.Yellow);
                        }
                        else if (arguments[1] == "nicinfo")
                        {
                            HTerminal.ColoredWrite("nicinfo - Get NIC information\n", Color.Yellow);
                        }
                        else if (arguments[1] == "ping")
                        {
                            HTerminal.ColoredWrite("ping - Ping a device on the network\n", Color.Yellow);
                        }
                        else if (arguments[1] == "ntp")
                        {
                            HTerminal.ColoredWrite("ntp - Get the current date/time from an NTP server\n", Color.Yellow);
                        }
                        else if (arguments[1] == "mkf")
                        {
                            HTerminal.ColoredWrite("mkf - Make a file\n", Color.Yellow);
                        }
                        else if (arguments[1] == "mkdir")
                        {
                            HTerminal.ColoredWrite("mkdir - Make a directory\n", Color.Yellow);
                        }
                        else if (arguments[1] == "rm")
                        {
                            HTerminal.ColoredWrite("rm - Delete a file or directory\n", Color.Yellow);
                        }
                        else if (arguments[1] == "cd")
                        {
                            HTerminal.ColoredWrite("cd - Change the current working directory\n", Color.Yellow);
                        }
                        else if (arguments[1] == "dir" || arguments[1] == "ls")
                        {
                            HTerminal.ColoredWrite("dir/ls - Get a directory listing\n", Color.Yellow);
                        }
                        else if (arguments[1] == "cat")
                        {
                            HTerminal.ColoredWrite("cat - Print a file's contents to the console\n", Color.Yellow);
                        }
                        else
                        {
                            HTerminal.ColoredWrite($"Invalid command: \"{arguments[1]}\"\n", Color.Red);
                        }
                    }
                    break;


                // ** NETWORK **
                // Use NTP to set the current date/time
                case "ntp":
                    try
                    {
                        Terminal.WriteLine(NTPClient.GetNetworkTime().ToString());
                    }
                    catch (Exception ex)
                    {
                        ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Get NIC information
                case "nicinfo":
                    if (Network.IsConfigured() == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"A NIC must be properly configured before networking can be used.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    foreach(var NIC in NetworkDevice.Devices)
                    {
                        Terminal.WriteLine($"[== {NIC.Name} ({NIC.NameID}) ==]\nMAC: {NIC.MACAddress.ToString()}\nIs ready: {NIC.Ready.ToString()}\n");
                    }

                    break;

                // Ping a device on a network using it's IPv4 address
                case "ping":
                    if (Network.IsConfigured() == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"A NIC must be properly configured before networking can be used.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"An IP address must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (Network.IsIPv4AddressValid(arguments[1]) == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"\"{arguments[1]}\" is not a valid IP address.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    Terminal.WriteLine($"Pinging {arguments[1]}:");
                    int SuccessCounter = 0;

                    for (int i = 0; i < 4; i++)
                    {
                        float PingTime = Network.ICMPPing(Network.StringToAddress(arguments[1]));

                        if(PingTime >= 0)
                        {
                            SuccessCounter++;
                            Terminal.WriteLine($"\t[{i + 1}] Ping succeeded in {PingTime} milliseconds.");
                            Thread.Sleep(250);
                        }

                        else
                        {
                            Terminal.WriteLine($"\tPing failed.");
                        }
                    }

                    Terminal.WriteLine($"{SuccessCounter}/4 pings succeeded. ({(float)(SuccessCounter / 4f) * 100f}%)\n");

                    break;

                // Print network information
                case "netinfo":
                    if (Network.IsConfigured() == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"A NIC must be properly configured before networking can be used.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    foreach (var config in NetworkConfiguration.NetworkConfigs)
                    {
                        Terminal.WriteLine($"[== {config.Device.NameID} ==]\n" +
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
                            ConsoleFunctions.PrintLogMSG("Attempting to obtain an IPv4 address via DHCP...\n\r", ConsoleFunctions.LogType.INFO);

                            if (Network.DHCPAutoconfig() == false)
                            {
                                ConsoleFunctions.PrintLogMSG("DHCP autoconfiguration failed.\n\r", ConsoleFunctions.LogType.ERROR);
                            }

                            // If DHCP worked, print the configuration information
                            if (Network.IsConfigured())
                            {
                                Terminal.WriteLine($"[== NET CONFIG ==]\n" +
                                    $"IP: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress.ToString()}\n" +
                                    $"SUBNET: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.SubnetMask.ToString()}\n" +
                                    $"DEFAULT GATEWAY: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.DefaultGateway.ToString()}\n" +
                                    $"MAC: {NetworkConfiguration.CurrentNetworkConfig.Device.MACAddress.ToString()}\n" +
                                    $"DEVICE NAME: {NetworkConfiguration.CurrentNetworkConfig.Device.Name.ToString()}\n" +
                                    $"ID: {NetworkConfiguration.CurrentNetworkConfig.Device.NameID.ToString()}\n");
                            }
                            else
                            {
                                ConsoleFunctions.PrintLogMSG("DHCP autoconfiguration failed: The device is not ready or an invalid IP address was assigned.\n\n\r", ConsoleFunctions.LogType.ERROR);
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
                        ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
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
                        ConsoleFunctions.PrintLogMSG($"The file \"{newFileName}\" already exists.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    newFileName = Path.GetFullPath(newFileName);
                    fs.CreateFile(newFileName);

                    break;

                // Make a directory
                case "mkdir":
                    var newDirName = "";

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
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
                        ConsoleFunctions.PrintLogMSG($"The directory \"{newDirName}\" already exists.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    newFileName = Path.GetFullPath(newDirName);
                    fs.CreateDirectory(newDirName);

                    break;

                // Delete a file or directory
                case "rm":
                    int PathArgIndex = 1;
                    var newPathName = "";
                    bool deleteDir = false;

                    foreach(var arg in arguments)
                    {
                        if (arg == "-rf")
                        {
                            PathArgIndex += 1;
                            deleteDir = true;
                            break;
                        }
                    }

                    if (arguments.Length <= PathArgIndex || string.IsNullOrWhiteSpace(arguments[PathArgIndex]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A file or directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (arguments[PathArgIndex].StartsWith("\""))
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
                        newPathName = arguments[PathArgIndex];
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
                        ConsoleFunctions.PrintLogMSG($"The item \"{newPathName}\" doesn't exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Get information about each installed (and functioning) disk
                case "diskinfo":
                    foreach(var disk in fs.Disks)
                    {
                        Terminal.WriteLine($"[== DISK #{fs.Disks.IndexOf(disk)} ==]");
                        disk.DisplayInformation();
                        Terminal.WriteLine($"Root path: {disk.Partitions[0].RootPath}\n");
                    }

                    Terminal.WriteLine();
                    break;

                // Change the current working directory
                case "cd":
                    var dir = "";

                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
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
                        ConsoleFunctions.PrintLogMSG($"The directory \"{dir}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    dir = Path.GetFullPath(dir);
                    Directory.SetCurrentDirectory(dir);

                    break;

                // Get a directory listing
                case "dir":
                case "ls":
                    foreach (var item in fs.GetDirectoryListing(Directory.GetCurrentDirectory()))
                    {
                        if (File.Exists(item.mFullPath))
                            Terminal.WriteLine($"[FILE] {item.mName}");

                        else if (Directory.Exists(item.mFullPath))
                            Terminal.WriteLine($"[DIR] {item.mName}");
                    }

                    Terminal.WriteLine();
                    break;

                // Print a file's contents to the console
                case "cat":
                    try
                    {
                        if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        }

                        else if (File.Exists(Path.GetFullPath(arguments[1])) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{arguments[1]}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        }

                        else
                        {
                            var contents = File.ReadAllText(Path.GetFullPath(arguments[1]));
                            Terminal.WriteLine($"{contents}\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;



                // ** POWER **
                // Shut down / restart the computer
                case "power":
                    if (arguments.Length <= 1)
                    {
                        ConsoleFunctions.PrintLogMSG("Please specify a power operation.\n\r  1. -s = shutdown\n\r  2. -r = reboot\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    switch (arguments[1])
                    {
                        case "-s":
                            ConsoleFunctions.PrintLogMSG("Shutting down...", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(1000);
                            Sys.Power.Shutdown();
                            break;

                        case "-r":
                            ConsoleFunctions.PrintLogMSG("Rebooting...", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(1000);
                            Sys.Power.Reboot();
                            break;

                        default:
                            ConsoleFunctions.PrintLogMSG("Invalid power operation.\n\r  1. -s = shutdown\n\r  2. -r = reboot\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                    }
                    
                    break;

                // ** SYSTEM **
                // Get RAM information
                case "raminfo":
                    UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                    Terminal.WriteLine($"[== RAM INFORMATION ==]\nRAM: {TotalInstalledRAM} KB\n" +
                        $"USED: {UsedRAM}/{TotalInstalledRAM} KB ({MathHelpers.TruncateToDecimalPlace((UsedRAM / TotalInstalledRAM) * 100f, 4)}%)\n");
                    break;

                // Get system information
                case "sysinfo":
                    UsedRAM = GCImplementation.GetUsedRAM() / 1024;
                    Terminal.WriteLine($"[== SYSTEM INFORMATION ==]\nCPU: {CPU.GetCPUBrandString()}\n" +
                        $"CPU Uptime: {CPU.GetCPUUptime()}\n" +
                        $"CPU Vendor: {CPU.GetCPUVendorName()}\n" +
                        $"RAM: {Kernel.TotalInstalledRAM} KB\n" +
                        $"USED RAM: {UsedRAM}/{TotalInstalledRAM} KB ({MathHelpers.TruncateToDecimalPlace((UsedRAM / TotalInstalledRAM) * 100f, 4)}%)\n" +
                        $"System uptime: {DateTime.Now - KernelStartTime}\n");
                    break;



                // ** EXTRA **
                // Print what the user entered as an argument
                case "echo":
                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        break;
                    }

                    for (int i = 1; i < arguments.Length; i++)
                    {
                        Terminal.Write($"{arguments[i]} ");
                    }

                    Terminal.WriteLine("\n\r");
                    break;

                // Force a debug kernel panic
                case "panic":
                    KernelPanic.Panic("The user forced a kernel panic for debugging purposes.", "-1");
                    break;

                // Empty command
                case "":
                    break;

                // Invalid command
                default:
                    ConsoleFunctions.PrintLogMSG($"Invalid command: \"{command}\"\n\n\r", ConsoleFunctions.LogType.ERROR);
                    break;
            }

            // Collect any garbage that we created. This helps prevent memory leaks, which can cause the computer
            // to run out of memory, and crash. Real OSes such as Windows solve this by using both a garbace collector and "swap" memory.
            // Swap is a partition or file that acts as extra (slower) RAM, and is stored on the hard disk. I haven't implemented
            // this yet because I'm focusing on getting the core functionality implemented first, and I believe it's pretty complex to implement.
            // Cosmos also doesn't have stable filesystem support as of right now (11-3-23).
            Heap.Collect();
        }
    }
}

