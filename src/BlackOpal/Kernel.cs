﻿/* [===== MAIN KERNEL CLASS =====] */
// Note: This kernel currently requires 103 MB of RAM to boot when using GZIP compression, but can run on as little 3.81MB (VMSVGA) / 12.62MB (VGA) / 512k (CMD) AFTER booting.
// If GZIP isn't being used, 100 MB is the minimum required to boot.
// I believe this is because the entire kernel is getting decompressed into RAM, but this might not be the case.
// After booting, the OS uses around 300-350K in console mode, and about 3.81MB (VMSVGA) / 12.62MB (VGA) in GUI mode.
// Using GZIP compresses the ISO image, at the expense of higher memory requirements

/* DIRECTIVES */
using Cosmos.System.Network.Config;
using Cosmos.System.Graphics;
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

        public const string OSContributors = "Scarlet";
        public const string OSVersion = "0.0.3";
        public const string OSAuthor = "memescoep";
        public const string OSName = "Black Opal";
        public const string OSDate = "2-23-2024";
        public static SVGAIITerminal.SVGAIITerminal Terminal;
        public static TextScreenBase TextScreen;
        public static DateTime KernelStartTime;
        public static Color TerminalColor = Color.Green;
        public static float TotalInstalledRAM = 0f;
        public static float UsedRAM = 0f;
        public string CMDPrompt = ">> ";
        public string Username = "root";
        public string Hostname = "BlackOpal";
        public Sys.FileSystem.CosmosVFS fs;
        private BtfFontFace TerminalFont;

        /* FUNCTIONS */
        // This function gets called immediately upon kernel startup. It'll be used to initialize a VGA screen, network driver, and IDE controller
        /*protected override void OnBoot()
        {
            //base.OnBoot();
            TextScreen = GetTextScreen();
            //Sys.Global.Init(TextScreen, false, true, true, true);
            ConsoleFunctions.PrintLogMSG($"VGA screen initialized.\n\r", ConsoleFunctions.LogType.INFO);

            // Get the start time of the kernel
            KernelStartTime = DateTime.Now;

            // Set the cursor's height to be a full block
            //Console.CursorSize = 100;

            // Print a boot message
            ConsoleFunctions.PrintLogMSG($"Kernel started at {KernelStartTime.ToString()}\n\r", ConsoleFunctions.LogType.INFO);
        }*/

        // Perform initialization and configuration
        protected override void BeforeRun()
        {
            try
            {
                // Get the start time of the kernel
                KernelStartTime = DateTime.Now;

                // Print a boot message
                ConsoleFunctions.PrintLogMSG($"Kernel started at {KernelStartTime.ToString()}\n\r", ConsoleFunctions.LogType.INFO);

                // Show a boot screen
                ConsoleFunctions.PrintLogMSG("Initializing INIT canvas...\n\r", ConsoleFunctions.LogType.INFO);
                /*INITCanvas = new VBECanvas();
                INITCanvas.Mode = new Mode(UserInterface.ScreenWidth, UserInterface.ScreenHeight, ColorDepth.ColorDepth32);
                INITCanvas.DrawString("Loading, please wait...", PCScreenFont.Default, System.Drawing.Color.White, (UserInterface.ScreenWidth / 2) - 92, UserInterface.ScreenHeight / 2);
                INITCanvas.Display();*/

                // Initialize the terminal
                ConsoleFunctions.PrintLogMSG("Loading terminal BTF font...\n\r", ConsoleFunctions.LogType.INFO);
                TerminalFont = new BtfFontFace(TTFFont, 16);

                ConsoleFunctions.PrintLogMSG("Initializing terminal...\n\r", ConsoleFunctions.LogType.INFO);
                Terminal = new SVGAIITerminal.SVGAIITerminal(UserInterface.ScreenWidth, UserInterface.ScreenHeight, TerminalFont);

                ConsoleFunctions.PrintLogMSG("Configuring terminal...\n\r", ConsoleFunctions.LogType.INFO);
                Terminal.CursorShape = SVGAIITerminal.CursorShape.Block;
                Terminal.ForegroundColor = Color.Yellow;
                Terminal.SetCursorPosition(0, 0);

                // Commented out because it causes crashes (I probalby fucked it up lmao)
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

                // Play a startup chime
                for (uint i = 200; i <= 1000; i += 100)
                    PCSpeaker.Beep(i, 25);
            }
            catch (Exception ex)
            {
                Terminal.ForegroundColor = Color.White;
                Terminal.Clear();
                ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\rThe system has been halted.\n\r", ConsoleFunctions.LogType.FATAL);
                PCSpeaker.Beep(500, 250);

                while (true)
                    CPU.Halt();
            }
        }

        // This function is called infinitely, and is run after "BeforeRun"
        // This will be known as the main loop
        protected override void Run()
        {
            try
            {
                // Print the prompt
                Terminal.ForegroundColor = Color.Magenta;
                Terminal.Write($"{Username}@{Hostname}");
                Terminal.ForegroundColor = Color.Green;
                Terminal.Write($"[{Directory.GetCurrentDirectory()}] {CMDPrompt}");
                Terminal.ForegroundColor = Color.White;

                // Get user input
                var input = Terminal.ReadLine();

                // Handle a command
                var arglist = input.Split(' ');
                HandleCommand(arglist[0], arglist);
            }
            catch (Exception ex)
            {
                ConsoleFunctions.PrintLogMSG($"{ex.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
            }
        }

        // Handle a command
        private void HandleCommand(string command, string[] arguments)
        {
            // Trim all whitespace from the start of the command
            command = command.TrimStart();

            switch (command)
            {
                // ** GRAPHICS **
                case "gui":
                    ConsoleFunctions.PrintLogMSG("Initializing GUI...\n\r", ConsoleFunctions.LogType.INFO);

                    UserInterface gui = new();
                    gui.Init();
                    break;

                case "videomodes":
                    var cv = FullScreenCanvas.GetFullScreenCanvas();
                    foreach (var mode in cv.AvailableModes)
                    {
                        ConsoleFunctions.PrintLogMSG($"{mode.Width.ToString()}x{mode.Height.ToString()}@32\n\r", ConsoleFunctions.LogType.NONE);
                    }

                    cv.Disable();
                    break;



                // ** CONSOLE **
                case "clear":
                case "cls":
                    Terminal.Clear();
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
                // Make a file
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
                // Print what the user put as an argument
                case "echo":
                    if (arguments.Length <= 1 || string.IsNullOrWhiteSpace(arguments[1]))
                    {
                        break;
                    }

                    for (int i = 1; i < arguments.Length; i++)
                    {
                        Terminal.Write($"{arguments[i]} ");
                    }

                    Terminal.WriteLine();
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
            // to run out of memory, which leads to crashes. Real OSes such as Windows solve this by using both a garbace collector and "swap".
            // Swap is a partition or file that acts as extra (slower) RAM, and is stored on the hard disk. I haven't implemented
            // this yet because I'm focusing on getting the core functionality implemented first, and I believe it's pretty complex to implement.
            // Cosmos also doesn't have stable filesystem support as of right now (11-3-23).
            Heap.Collect();
        }
    }
}

