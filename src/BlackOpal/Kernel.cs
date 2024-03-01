/* [===== MAIN KERNEL CLASS =====] */
// Note: This kernel currently requires 103 MB of RAM to boot when using GZIP compression, but can run on as little 3.81MB (VMSVGA) / 12.62MB (VGA) / 512k (CMD) AFTER booting.
// If GZIP isn't being used, 100 MB is the minimum required to boot.
// I believe this is because the entire kernel is getting decompressed into RAM, but this might not be the case.
// After booting, the OS uses around 300-350K in console mode, and about 3.81MB (VMSVGA) / 12.62MB (VGA) in GUI mode.
// Using GZIP compresses the ISO image, at the expense of higher memory requirements

/* DIRECTIVES */
using Cosmos.System.FileSystem.Listing;
using Cosmos.System.Network.Config;
using Cosmos.Core.Memory;
using Cosmos.Core;
using Cosmos.HAL;
using IL2CPU.API.Attribs;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.IO;
using System;
using IO.Networking;
using IO.CMD;
using GUI;
using BlackOpal.IO.Filesystem;
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
        public static UserInterface UI = new UserInterface();
        public static HTerminal HTerminal = new HTerminal(Terminal);
        public static DateTime KernelStartTime;
        public static Color TerminalColor = Color.Green;
        public static string HydrixLibVersion = HTerminal.GetHydrixLibVersion();
        public static float TotalInstalledRAM = 0f;
        public static float UsedRAM = 0f;
        public string CMDPrompt = ">>";
        public string Username = "root";
        public string Hostname = "BlackOpal";
        public Sys.FileSystem.CosmosVFS FS; 

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
                /*for (uint i = 512; i < 32768; i += 512)
                {
                    ConsoleFunctions.PrintLogMSG($"Zeroing memory block {CPU.GetEndOfKernel() + i} -> {CPU.GetEndOfKernel() + i + 512}...\n\r", ConsoleFunctions.LogType.INFO);
                    CPU.ZeroFill(CPU.GetEndOfKernel() + i, 512);
                }*/

                // Initialize ACPI
                ConsoleFunctions.PrintLogMSG($"Initializing ACPI...\n\r", ConsoleFunctions.LogType.INFO);
                ACPI.Start();

                // Set the keyboard layout (this may help with some keyboards acting funky)
                ConsoleFunctions.PrintLogMSG($"Setting keyboard layout...\n\r", ConsoleFunctions.LogType.INFO);
                Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.USStandardLayout());

                // Get the total amount of installed RAM in the computer
                TotalInstalledRAM = CPU.GetAmountOfRAM() * 1024f;

                // Create and register the virtual filesystem object
                ConsoleFunctions.PrintLogMSG("Creating and registering the virtual filesystem...\n\r", ConsoleFunctions.LogType.INFO);
                
                try
                {
                    FS = new();
                    Sys.FileSystem.VFS.VFSManager.RegisterVFS(FS);

                    // Set the current working directory (if we can)
                    if (FS.Disks.Count > 0)
                    {
                        ConsoleFunctions.PrintLogMSG("Setting the current working directory...\n\r", ConsoleFunctions.LogType.INFO);
                        foreach (var Disk in FS.Disks)
                        {
                            if (Disk.Partitions.Count > 0)
                            {
                                if (Disk.Partitions[0].HasFileSystem && String.IsNullOrEmpty(Disk.Partitions[0].RootPath) == false)
                                {
                                    Directory.SetCurrentDirectory(Disk.Partitions[0].RootPath);
                                    ConsoleFunctions.PrintLogMSG($"Working directory is: \"{Disk.Partitions[0].RootPath}\"\n\r", ConsoleFunctions.LogType.INFO);
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
                catch(Exception EX)
                {
                    ConsoleFunctions.PrintLogMSG($"Disk init error: {EX.Message}\n\n\rPress any key to continue.", ConsoleFunctions.LogType.ERROR);
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
            catch (Exception EX)
            {
                KernelPanic.Panic(EX.Message, EX.HResult.ToString());
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
                var Input = Terminal.ReadLine();

                // Handle a command
                var ArgList = Input.Split(' ');

                HandleCommand(ArgList[0], ArgList);
            }
            catch (Exception EX)
            {
                KernelPanic.Panic(EX.Message, EX.HResult.ToString());
            }
        }

        // Handle a command
        private void HandleCommand(string Command, string[] Arguments)
        {
            // Trim all whitespace from the start of the command
            Command = Command.TrimStart();

            // Handle the command
            switch (Command)
            {
                // ** GRAPHICS **
                case "gui":
                    ConsoleFunctions.PrintLogMSG("Initializing GUI...\n\r", ConsoleFunctions.LogType.INFO);                    
                    UI.Init();
                    break;

                // ** CONSOLE **
                case "clear":
                case "cls":
                    Terminal.Clear();
                    break;

                case "help":
                    // Check if there are any arguments
                    if (Arguments.Length == 1)
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
                        Help.ShowHelp(Arguments[1]);
                    }
                    break;


                // ** NETWORK **
                // Use NTP to set the current date/time
                case "ntp":
                    try
                    {
                        Terminal.WriteLine(NTPClient.GetNetworkTime().ToString());
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
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

                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"An IP address must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (Network.IsIPv4AddressValid(Arguments[1]) == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"\"{Arguments[1]}\" is not a valid IP address.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    Terminal.WriteLine($"Pinging {Arguments[1]}:");
                    int SuccessCounter = 0;

                    for (int i = 0; i < 4; i++)
                    {
                        float PingTime = Network.ICMPPing(Network.StringToAddress(Arguments[1]));

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

                    foreach (var Config in NetworkConfiguration.NetworkConfigs)
                    {
                        Terminal.WriteLine($"[== {Config.Device.NameID} ==]\n" +
                                    $"IP: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress.ToString()}\n" +
                                    $"SUBNET: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.SubnetMask.ToString()}\n" +
                                    $"DEFAULT GATEWAY: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.DefaultGateway.ToString()}\n" +
                                    $"MAC: {NetworkConfiguration.CurrentNetworkConfig.Device.MACAddress.ToString()}\n" +
                                    $"DEVICE NAME: {NetworkConfiguration.CurrentNetworkConfig.Device.Name.ToString()}\n");
                    }

                    break;

                // Initialize the NIC
                case "netinit":
                    if (Arguments.Length >= 1)
                    {
                        if (Arguments[1] == "dhcp")
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
                    var NewFileName = "";

                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    NewFileName = PathUtils.GetValidPath(PathUtils.ListToPath(Arguments.ToList(), true));

                    if (Path.GetFileName(NewFileName).Length > 8)
                    {
                        ConsoleFunctions.PrintLogMSG($"The filename cannot be greater than 8 characters.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    if (File.Exists(NewFileName) == true)
                    {
                        ConsoleFunctions.PrintLogMSG($"The file \"{NewFileName}\" already exists.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    NewFileName = Path.GetFullPath(NewFileName);
                    FS.CreateFile(NewFileName);

                    break;

                // Make a directory
                case "mkdir":
                    var NewDirName = "";

                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    NewDirName = PathUtils.GetValidPath(PathUtils.ListToPath(Arguments.ToList(), true));

                    if (Directory.Exists(NewDirName) == true)
                    {
                        ConsoleFunctions.PrintLogMSG($"The directory \"{NewDirName}\" already exists.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    NewDirName = Path.GetFullPath(NewDirName);
                    FS.CreateDirectory(NewDirName);

                    break;

                // Delete a file or directory
                case "rm":
                    bool DeleteDir = false;
                    int PathArgIndex = 1;
                    var NewPathName = "";

                    foreach(var Arg in Arguments)
                    {
                        if (Arg == "-rf")
                        {
                            DeleteDir = true;
                            var ArgsList = Arguments.ToList();

                            ArgsList.RemoveAt(1);
                            Arguments = ArgsList.ToArray();
                            break;
                        }
                    }

                    if (Arguments.Length <= PathArgIndex || string.IsNullOrWhiteSpace(Arguments[PathArgIndex]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A file or directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    NewPathName = PathUtils.GetValidPath(PathUtils.ListToPath(Arguments.ToList(), true));
                    NewPathName = Path.GetFullPath(NewPathName);

                    if (File.Exists(NewPathName))
                    {
                        File.Delete(NewPathName);
                    }
                    else if (Directory.Exists(NewPathName) && DeleteDir)
                    {
                        Directory.Delete(NewPathName);
                    }
                    else
                    {
                        ConsoleFunctions.PrintLogMSG($"The item \"{NewPathName}\" doesn't exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Get information about each installed (and functioning) disk
                case "diskinfo":
                    foreach(var Disk in FS.Disks)
                    {
                        Terminal.WriteLine($"[== DISK #{FS.Disks.IndexOf(Disk)} ==]");
                        Disk.DisplayInformation();
                        Terminal.WriteLine($"Root path: {Disk.Partitions[0].RootPath}\n");
                    }

                    Terminal.WriteLine();
                    break;

                // Change the current working directory
                case "cd":
                    var Dir = "0:\\";

                    // Check to make sure arguments exist
                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    // Get a valid path from the above path
                    Dir = PathUtils.GetValidPath(PathUtils.ListToPath(Arguments.ToList(), true));

                    // Make sure the valid directory exists
                    if (Directory.Exists(Dir) == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"The directory \"{Dir}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    // Change to the new directory
                    Dir = Path.GetFullPath(Dir);
                    Directory.SetCurrentDirectory(Dir);
                    break;

                // Get a directory listing
                case "dir":
                case "ls":
                    string SearchDirectory = Directory.GetCurrentDirectory();

                    if (Arguments.Length >= 2)
                    {
                        SearchDirectory = Path.GetFullPath(Arguments[1]);
                    }

                    if (Directory.Exists(SearchDirectory) == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"The directory \"{SearchDirectory}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    var EntryList = new List<DirectoryEntry>();
                    int LongestItemLength = 0;

                    foreach (var Item in FS.GetDirectoryListing(SearchDirectory))
                    {
                        if (File.Exists(Item.mFullPath) && Item.mName.Length > LongestItemLength)
                        {
                            LongestItemLength = Item.mName.Length;
                        }
                            
                        EntryList.Add(Item);                 
                    }

                    foreach (var Entry in EntryList)
                    {
                        if (File.Exists(Entry.mFullPath))
                        {
                            HTerminal.ColoredWrite("[FILE] ", Color.GoogleGreen);
                            Terminal.Write(Entry.mName);
                            HTerminal.ColoredWrite($" ---{new string('-', LongestItemLength - Entry.mName.Length)} ", Color.UltraViolet);
                            HTerminal.ColoredWriteLine($"{Entry.mSize / 1024f} KB", Color.Magenta);
                        }

                        else if (Directory.Exists(Entry.mFullPath))
                        {
                            HTerminal.ColoredWrite("[DIR] ", Color.GoogleBlue);
                            Terminal.WriteLine(Entry.mName);
                        }
                    }

                    Terminal.WriteLine();
                    break;

                // Get a valid path from
                case "validpath":
                    Terminal.WriteLine($"\"{PathUtils.GetValidPath(Arguments[1])}\"");
                    break;

                // Print a file's contents to the console
                case "cat":
                    try
                    {
                        var FilePath = Path.GetFullPath(PathUtils.GetValidPath(Arguments[1]));

                        if (FilePath.Length <= 1 || string.IsNullOrWhiteSpace(FilePath))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        }

                        else if (File.Exists(Path.GetFullPath(FilePath)) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{FilePath}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        }

                        else
                        {
                            Terminal.WriteLine($"{File.ReadAllText(FilePath)}\n");
                        }
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Print a file's contents to the console in hex
                case "hex":
                    try
                    {
                        var FilePath = Path.GetFullPath(PathUtils.GetValidPath(Arguments[1]));

                        if (FilePath.Length <= 1 || string.IsNullOrWhiteSpace(FilePath))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        }

                        else if (File.Exists(Path.GetFullPath(FilePath)) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{FilePath}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        }

                        else
                        {
                            // Temporary variables
                            string ByteHex = "00";
                            byte[] Content = File.ReadAllBytes(Path.GetFullPath(FilePath));
                            int LineNumber = 1;

                            // Print the first line number
                            HTerminal.ColoredWrite($"{LineNumber}: ", Color.GoogleGreen);

                            // Loop through each byte in the file
                            for (long ByteIndex = 0; ByteIndex < Content.Length; ByteIndex++)
                            {
                                // Convert the byte to hex
                                ByteHex = $"{Convert.ToInt32(Content[ByteIndex]):X}";

                                // Add a leading zero if the byte is <= 15 (this is for formatting purposes)
                                if (Convert.ToInt32(ByteHex, 16) <= 15)
                                {
                                    ByteHex = ByteHex.Insert(0, "0");
                                }

                                // Print the hex byte
                                Terminal.Write($"{ByteHex} ");

                                // If more than 12 characters have been printed on the current line, start a new line
                                if ((ByteIndex + 1) % 12 == 0 && ByteIndex > 0 && Content.Length != 12)
                                {
                                    LineNumber++;
                                    Terminal.WriteLine();
                                    HTerminal.ColoredWrite($"{LineNumber}: ", Color.GoogleGreen);

                                    // Collect garbage so the OS doesn't crash
                                    Heap.Collect();
                                }
                            }

                            Terminal.WriteLine("\n\r");
                        }
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;



                // ** POWER **
                // Shut down / restart the computer
                case "power":
                    if (Arguments.Length <= 1)
                    {
                        ConsoleFunctions.PrintLogMSG("Please specify a power operation.\n\r  1. -s = shutdown\n\r  2. -r = reboot\n\r  3. -as = ACPI shutdown\n\r  4. -ar = ACPI reboot\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    switch (Arguments[1])
                    {
                        case "-s":
                            ConsoleFunctions.PrintLogMSG("Shutting down...\n\n\r", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(250);
                            Sys.Power.Shutdown();
                            break;

                        case "-r":
                            ConsoleFunctions.PrintLogMSG("Rebooting...\n\n\r", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(250);
                            Sys.Power.Reboot();
                            break;

                        case "-as":
                            ConsoleFunctions.PrintLogMSG("Shutting down (ACPI)...\n\n\r", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(250);
                            ACPI.Shutdown();
                            break;

                        case "-ar":
                            ConsoleFunctions.PrintLogMSG("Rebooting (ACPI)...\n\n\r", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(250);
                            ACPI.Reboot();
                            break;

                        default:
                            ConsoleFunctions.PrintLogMSG("Invalid power operation.\n\r  1. -s = shutdown\n\r  2. -r = reboot\n\r  3. -as = ACPI shutdown\n\r  4. -ar = ACPI reboot\n\n\r", ConsoleFunctions.LogType.ERROR);
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
                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        break;
                    }

                    for (int i = 1; i < Arguments.Length; i++)
                    {
                        Terminal.Write($"{Arguments[i]} ");
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
                    ConsoleFunctions.PrintLogMSG($"Invalid command: \"{Command}\"\n\n\r", ConsoleFunctions.LogType.ERROR);
                    break;
            }

            // Collect any garbage that we created. This helps prevent memory leaks, which can cause the computer
            // to run out of memory, and crash. Real OSes such as Windows solve this by using both a garbace collector and "swap" memory.
            // Swap is a partition or file that acts as extra (slower) RAM, and is stored on the hard disk. I haven't implemented
            // this yet because I'm focusing on getting the core functionality implemented first, and I believe it's fairly complex.
            // Cosmos also doesn't have stable filesystem support as of right now (11-3-23).
            Heap.Collect();
        }
    }
}

