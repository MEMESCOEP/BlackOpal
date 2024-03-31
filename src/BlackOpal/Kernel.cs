/* [===== MAIN KERNEL CLASS =====] */
// Note: This kernel currently requires 103 MB of RAM to boot when using GZIP compression, but can run on as little 3.99MB (VMSVGA) / 7.108MB (VBE) AFTER booting.
// If GZIP isn't being used, 100 MB is the minimum required to boot.
// I believe this is because the entire kernel is getting decompressed into RAM, but this might not be the case.
// After booting, the OS uses around 7MB in console mode, and about 3.99MB (VMSVGA) / 10.04MB (VBE) in GUI mode.
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
using BlackOpal.Utilities.Calculations.Checksum;
using BlackOpal.Utilities.Calculations;
using BlackOpal.Utilities.KernelUtils;
using BlackOpal.Utilities.Converters;
using BlackOpal.Utilities.Viewers;
using BlackOpal.Utilities.Interpreters;
using BlackOpal.Utilities.Installers;
using BlackOpal.Utilities.Parsers;
using BlackOpal.IO.Networking;
using BlackOpal.IO.Filesystem;
using GrapeGL.Graphics.Fonts;
using GrapeGL.Hardware.GPU;
using GrapeGL.Graphics;
using HydrixLIB;
using Sys = Cosmos.System;

/* NAMESPACES */
namespace BlackOpal
{
    /* CLASSES */
    public class Kernel : Sys.Kernel
    {
        /* VARIABLES */
        [ManifestResourceStream(ResourceName = "BlackOpal.Assets.Fonts.Terminal.btf")]
        public static byte[] TTFFont;

        public const string OSVersion = "0.0.330";
        public const string OSAuthor = "memescoep";
        public const string OSName = "Black Opal";
        public const string OSDate = "3-30-2024";
        public const bool DEBUG = false;

        public static SVGAIITerminal.SVGAIITerminal Terminal = new SVGAIITerminal.SVGAIITerminal(UserInterface.ScreenWidth, UserInterface.ScreenHeight, new BtfFontFace(TTFFont, 16));
        public static TextScreenBase TextScreen;
        public static UserInterface UI;
        public static HTerminal HTerminal = new HTerminal(Terminal);
        public static DateTime KernelStartTime;
        public static Color TerminalColor = Color.Green;
        public static string HydrixLibVersion = HTerminal.GetHydrixLibVersion();
        public static string TerminalDriver = "None";
        public static string BootMediumPath = "None";
        public static string SysConfigFile = "SysCFG.xml";
        public static float TotalInstalledRAM = 0f;
        public static float UsedRAM = 0f;
        public static bool FSExists = false;
        public static int InstallationMarker = 0x69;
        public static int BootMediumIndex = -1;
        public string CMDPrompt = ">>";
        public string Username = "UnknownUser";
        public string Hostname = "BlackOpal";
        public static Sys.FileSystem.CosmosVFS FS;
        private bool AutostartGUI = false;

        /* FUNCTIONS */
        // Initialize everything except the VGA console
        protected override void OnBoot()
        {
            Sys.Global.Init(null, true, true, true, true);
        }

        // Perform initialization and configuration
        protected override void BeforeRun()
        {
            try
            {
                // Get the start time of the kernel
                KernelStartTime = DateTime.Now;

                // Get the total amount of installed RAM in the computer
                TotalInstalledRAM = CPU.GetAmountOfRAM() * 1024f;
                ConsoleFunctions.PrintLogMSG($"Total installed RAM: {TotalInstalledRAM} KB\n\r", ConsoleFunctions.LogType.DEBUG);

                // Initialize the terminal
                Terminal.Clear();
                ConsoleFunctions.PrintLogMSG("Configuring terminal...\n\r", ConsoleFunctions.LogType.INFO);
                Terminal.CursorShape = SVGAIITerminal.CursorShape.Block;
                TerminalDriver = ((Display)Terminal.Contents).GetName();
                Terminal.SetCursorPosition(0, 0);
                ConsoleFunctions.PrintLogMSG($"Using the \"{TerminalDriver}\" driver.\n\r", ConsoleFunctions.LogType.INFO);
                Terminal.ForegroundColor = TerminalColor;
                Terminal.BackgroundColor = Color.Black;
                Terminal.Clear();

                // Print boot message(s)
                ConsoleFunctions.PrintLogMSG($"{OSName} kernel started at {KernelStartTime.ToString()}\n\r", ConsoleFunctions.LogType.INFO);
                ConsoleFunctions.PrintLogMSG($"Stack start address: 0x{CPU.GetStackStart()}\n\r", ConsoleFunctions.LogType.DEBUG);
                ConsoleFunctions.PrintLogMSG($"Kernel end address: 0x{CPU.GetEndOfKernel()}\n\r", ConsoleFunctions.LogType.DEBUG);
                ConsoleFunctions.PrintLogMSG($"EBP register: 0x{CPU.GetEBPValue()}\n\r", ConsoleFunctions.LogType.DEBUG);
                ConsoleFunctions.PrintLogMSG($"MBI address: 0x{Cosmos.Core.Multiboot.Multiboot2.GetMBIAddress()}\n\r", ConsoleFunctions.LogType.DEBUG);
                ConsoleFunctions.PrintLogMSG($"VBE available: {Cosmos.Core.Multiboot.Multiboot2.IsVBEAvailable}\n\r", ConsoleFunctions.LogType.DEBUG);
                ConsoleFunctions.PrintLogMSG($"PS/2 status I/O port: 0x{Cosmos.Core.IOGroup.PS2Controller.Status}\n\r", ConsoleFunctions.LogType.DEBUG);
                ConsoleFunctions.PrintLogMSG($"HydrixLIB Version: {HydrixLibVersion}\n\r", ConsoleFunctions.LogType.INFO);

                // Commented out because it causes crashes (I probably fucked it up lmao)
                // Zero memory so the system starts in a known state
                /*ConsoleFunctions.PrintLogMSG($"Zeroing memory from {CPU.GetEndOfKernel()} -> {TotalInstalledRAM * 1024f}...\n\r", ConsoleFunctions.LogType.INFO);

                for (uint i = 65535; i < TotalInstalledRAM * 1024f; i += 65535)
                {
                    if (CPU.GetEndOfKernel() + i > TotalInstalledRAM * 1024f) 
                    {
                        //var NumberOfBytesToEnd = (uint)(TotalInstalledRAM * 1024f) - (CPU.GetEndOfKernel() + i);

                        //ConsoleFunctions.PrintLogMSG($"Zeroing memory block {CPU.GetEndOfKernel() + i} -> {CPU.GetEndOfKernel() + i + NumberOfBytesToEnd} ({NumberOfBytesToEnd} bytes)...\n\r", ConsoleFunctions.LogType.INFO);
                        //CPU.ZeroFill(CPU.GetEndOfKernel() + i, NumberOfBytesToEnd);
                        break;
                    }
                    else
                    {
                        ConsoleFunctions.PrintLogMSG($"Zeroing memory block {CPU.GetEndOfKernel() + i} -> {CPU.GetEndOfKernel() + i + 65535} ({(uint)(TotalInstalledRAM * 1024f) - (CPU.GetEndOfKernel() + i)} bytes remaining)...\n\r", ConsoleFunctions.LogType.INFO);
                        CPU.ZeroFill(CPU.GetEndOfKernel() + i, 65535);
                    }
                }*/

                // Initialize ACPI
                ConsoleFunctions.PrintLogMSG($"Initializing ACPI...\n\r", ConsoleFunctions.LogType.INFO);
                ACPI.Start();

                // Set the keyboard layout (this may help with some keyboards acting funky)
                ConsoleFunctions.PrintLogMSG($"Setting keyboard layout (US-Standard)...\n\r", ConsoleFunctions.LogType.INFO);
                Sys.KeyboardManager.SetKeyLayout(new Sys.ScanMaps.USStandardLayout());

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
                            ConsoleFunctions.PrintLogMSG($"Searching disk #{FS.Disks.IndexOf(Disk)}...\n\r", ConsoleFunctions.LogType.DEBUG);

                            if (Disk.Partitions.Count > 0)
                            {
                                if (Disk.Partitions[0].HasFileSystem && String.IsNullOrEmpty(Disk.Partitions[0].RootPath) == false)
                                {
                                    ConsoleFunctions.PrintLogMSG($"Searching partition #0...\n\r", ConsoleFunctions.LogType.DEBUG);
                                    FSExists = true;
                                    Directory.SetCurrentDirectory(Disk.Partitions[0].RootPath);
                                    ConsoleFunctions.PrintLogMSG($"Working directory is: \"{Disk.Partitions[0].RootPath}\"\n\r", ConsoleFunctions.LogType.DEBUG);
                                    break;
                                }
                            }

                            else
                            {
                                ConsoleFunctions.PrintLogMSG("The disk doesn't have a root path.\n\r", ConsoleFunctions.LogType.WARNING);
                            }
                        }
                    }
                    else
                    {
                        ConsoleFunctions.PrintLogMSG("No functioning FAT32/UDF formatted disks are installed, You won't be able to save anything.\n\n\rPress any key to continue.", ConsoleFunctions.LogType.WARNING);
                        Terminal.ReadKey(true);
                        Terminal.WriteLine();
                    }
                }
                catch(Exception EX)
                {
                    ConsoleFunctions.PrintLogMSG($"Disk init error: {EX.Message}\n\n\rPress any key to continue.", ConsoleFunctions.LogType.ERROR);
                    Terminal.ReadKey();
                }

                // Load the system configuration
                if (FSExists == true)
                {
                    ConsoleFunctions.PrintLogMSG($"Attempting to read configuration file...\n\r", ConsoleFunctions.LogType.INFO);
                    var FoundCFGFile = false;
                    var UseDHCP = true;

                    foreach (var Disk in FS.Disks)
                    {
                        ConsoleFunctions.PrintLogMSG($"Searching disk #{FS.Disks.IndexOf(Disk)}...\n\r", ConsoleFunctions.LogType.DEBUG);

                        var PartitionCount = -1;

                        if (FoundCFGFile == true)
                        {
                            break;
                        }

                        foreach (var Partition in Disk.Partitions)
                        {
                            ConsoleFunctions.PrintLogMSG($"Searching partition #{PartitionCount}...\n\r", ConsoleFunctions.LogType.DEBUG);
                            PartitionCount++;

                            if (Partition.HasFileSystem == false)
                            {
                                ConsoleFunctions.PrintLogMSG($"Partition #{PartitionCount} on disk #{FS.Disks.IndexOf(Disk)} has no filesystem.\n\r", ConsoleFunctions.LogType.WARNING);
                                continue;
                            }

                            BootMediumPath = Disk.Partitions[0].RootPath;
                            BootMediumIndex = FS.Disks.IndexOf(Disk);

                            if (File.Exists(Path.Join(Partition.RootPath, SysConfigFile)) == false)
                            {
                                ConsoleFunctions.PrintLogMSG($"The configuration file \"{SysConfigFile}\" doesn't exist in \"{Partition.RootPath}\".\n\r", ConsoleFunctions.LogType.WARNING);
                                continue;
                            }

                            FoundCFGFile = true;
                            SysConfigFile = Path.Join(Partition.RootPath, SysConfigFile);
                            ConsoleFunctions.PrintLogMSG($"Found system configuration file on partition #{PartitionCount} ({Partition.RootPath}, disk #{FS.Disks.IndexOf(Disk)}).\n\r", ConsoleFunctions.LogType.INFO);
                            NanoXMLDocument SystemConfigXML = new NanoXMLDocument(File.ReadAllText(SysConfigFile));
                            Username = SystemConfigXML.RootNode.Value;

                            foreach(var SubNode in SystemConfigXML.RootNode.SubNodes)
                            {
                                ConsoleFunctions.PrintLogMSG($"Found sub node: \"{SubNode.Name}\" with value \"{SubNode.Value}\".\n\r", ConsoleFunctions.LogType.DEBUG);

                                if (SubNode.Name == "Username")
                                {
                                    Username = SubNode.Value;
                                }

                                else if (SubNode.Name == "DefaultCWD")
                                {
                                    if(Directory.Exists(SubNode.Value) == false)
                                    {
                                        ConsoleFunctions.PrintLogMSG($"The directory \"{SubNode.Value}\" doesn't exist.\n\r", ConsoleFunctions.LogType.ERROR);
                                        continue;
                                    }

                                    Directory.SetCurrentDirectory(SubNode.Value);
                                }

                                else if (SubNode.Name == "SystemIPAddress")
                                {
                                    if (SubNode.Value == "Auto")
                                    {
                                        UseDHCP = true;
                                    }
                                    else if (Network.IsIPv4AddressValid(SubNode.Value))
                                    {
                                        UseDHCP = false;
                                    }
                                    else
                                    {
                                        ConsoleFunctions.PrintLogMSG($"Invalid system IP address value: \"{SubNode.Value}\"\n\r", ConsoleFunctions.LogType.ERROR);
                                    }
                                }

                                else if (SubNode.Name == "AutoconfigNetwork")
                                {
                                    if(SubNode.Value == "True")
                                    {
                                        Network.Init(UseDHCP);
                                    }
                                }

                                else if (SubNode.Name == "AutostartGUI")
                                {
                                    AutostartGUI = SubNode.Value == "True";
                                }
                                
                                else
                                {
                                    ConsoleFunctions.PrintLogMSG($"Invalid subnode name: \"{SubNode.Name}\".\n\r", ConsoleFunctions.LogType.WARNING);
                                }
                            }

                            break;
                        }
                    }
                }
                else
                {
                    ConsoleFunctions.PrintLogMSG($"There is no filesystem, so the configuration file will not be checked.\n\r", ConsoleFunctions.LogType.WARNING);
                }

                // Check for pending installations
                /*foreach (var Disk in FS.Disks)
                {
                    if (Disk.Partitions.Count > 0)
                    {
                        byte[] BlockData = new byte[512];
                        int DiskIndex = FS.Disks.IndexOf(Disk);

                        Disk.Partitions[0].Host.ReadBlock(1, 1, ref BlockData);

                        if (BlockData[0] == InstallationMarker)
                        {
                            ConsoleFunctions.PrintLogMSG($"Found pending installation on disk #{DiskIndex}\n\r", ConsoleFunctions.LogType.INFO);
                            OSInstaller.Init(true, DiskIndex);
                        }
                    }
                }*/
                
                // Instantiate the GUI class
                ConsoleFunctions.PrintLogMSG("Instantiating the GUI...\n\r", ConsoleFunctions.LogType.INFO);
                UI = new UserInterface();

                // Collect any garbage that we created
                ConsoleFunctions.PrintLogMSG("Calling the garbage collector...\n\r", ConsoleFunctions.LogType.INFO);
                Heap.Collect();

                // Clear the console
                Terminal.Clear();

                // Print the OS name and version
                ConsoleFunctions.PrintLogMSG($"[===== {OSName} {OSVersion} - {OSDate} =====]\n\r" +
                    $"By: {OSAuthor} (see Contributors.txt for more)\n\r" +
                    $"Kernel started at {KernelStartTime.ToString()} (BTR took {(DateTime.Now.Second - KernelStartTime.Second).ToString()} seconds).\n\r" +
                    $"Terminal size: {Terminal.Width}x{Terminal.Height} ({UserInterface.ScreenWidth}x{UserInterface.ScreenHeight}, {TerminalDriver.Replace("Canvas", "")})\n\n\r", ConsoleFunctions.LogType.NONE);

                if (AutostartGUI)
                {
                    UI.Init();
                }
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
                HTerminal.ColoredWrite("@", Color.StackOverflowWhite);
                HTerminal.ColoredWrite($"{Hostname}", Color.Cyan);
                HTerminal.ColoredWrite($"[{Directory.GetCurrentDirectory()}] ", Color.Green);
                HTerminal.ColoredWrite($"{CMDPrompt} ", Color.Red);
                Terminal.ForegroundColor = GrapeGL.Graphics.Color.StackOverflowWhite;

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
                    if (Arguments.Length > 1)
                    {
                        Help.ShowHelp(Arguments[1]);
                    }
                    else
                    {
                        Help.ShowHelp();
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
                        Terminal.WriteLine($"[== {NIC.Name} ({NIC.NameID}) ==]\nMAC: {NIC.MACAddress.ToString()}\nIs ready: {NIC.Ready.ToString()}\n\n\r");
                    }

                    break;

                // Ping a device on a network using it's IPv4 address
                case "ping":
                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"An IP address must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    Ping.PingAddress(Arguments[1]);
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
                    if (Arguments.Length < 2)
                    {
                        ConsoleFunctions.PrintLogMSG($"A configuration method must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    switch(Arguments[1])
                    {
                        case "--dhcp":
                            Network.Init();
                            break;

                        default:
                            ConsoleFunctions.PrintLogMSG($"Invalid configuration mode: \"{Arguments[1]}\"\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                    }

                    break;



                // ** FILESYSTEM **
                // Install the OS to a hard disk
                case "install":
                    OSInstaller.Init();
                    break;

                // Make a file
                case "mkf":
                    try
                    {
                        if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        FSUtilities.CreateFile(PathUtils.ListToPath(Arguments.ToList(), true));
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Make a directory
                case "mkdir":
                    try
                    {
                        if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        FSUtilities.CreateDirectory(PathUtils.ListToPath(Arguments.ToList(), true));
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Delete a file or directory
                case "rm":
                    try
                    {
                        bool DeleteDir = false;
                        int PathArgIndex = 1;

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

                        FSUtilities.RemoveItem(PathUtils.ListToPath(Arguments.ToList(), true), DeleteDir);
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }
                    
                    break;

                // Copy a file to a new destination
                case "cp":
                case "copy":
                    try
                    {
                        if (Arguments.Length <= 2 || string.IsNullOrWhiteSpace(Arguments[1]) || string.IsNullOrWhiteSpace(Arguments[2]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file and directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        FSUtilities.CopyItem(Arguments[1], Arguments[2]);
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Move a file to a new destination
                case "mv":
                case "move":
                    try
                    {
                        if (Arguments.Length <= 2 || string.IsNullOrWhiteSpace(Arguments[1]) || string.IsNullOrWhiteSpace(Arguments[2]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file and directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        FSUtilities.MoveItem(Arguments[1], Arguments[2]);
                    }
                    catch(Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Get information about each installed (and functioning) disk
                case "diskinfo":
                    foreach (var Disk in FS.Disks)
                    {
                        var DiskType = "Unknown";

                        if (Disk.Type == Cosmos.HAL.BlockDevice.BlockDeviceType.HardDrive)
                        {
                            DiskType = "HDD";
                        }
                        else if (Disk.Type == Cosmos.HAL.BlockDevice.BlockDeviceType.RemovableCD)
                        {
                            DiskType = "CD/DVD";
                        }
                        else if (Disk.Type == Cosmos.HAL.BlockDevice.BlockDeviceType.Removable)
                        {
                            DiskType = "Removable";
                        }

                        Terminal.WriteLine($"[== DISK #{FS.Disks.IndexOf(Disk)} ({DiskType}) ==]");

                        for (int i = 0; i < Disk.Partitions.Count; i++)
                        {
                            Terminal.WriteLine("Partition #: " + (i + 1));
                            Terminal.WriteLine("Block Size: " + Disk.Partitions[i].Host.BlockSize + " bytes");
                            Terminal.WriteLine("Block Partitions: " + Disk.Partitions[i].Host.BlockCount);
                            Terminal.WriteLine("Size: " + Disk.Partitions[i].Host.BlockCount * Disk.Partitions[i].Host.BlockSize / 1024 / 1024 + " MB");
                            Terminal.WriteLine($"Root path: {Disk.Partitions[i].RootPath}\n");
                        }

                        if (Disk.Partitions.Count <= 0)
                            Terminal.WriteLine();
                    }

                    Terminal.WriteLine();
                    break;

                // Format the first partition of a disk
                case "format":
                    try
                    {
                        Terminal.Write("Choose a disk to format >> ");
                        FSUtilities.FormatDisk(Convert.ToInt32(Terminal.ReadLine()));

                        Terminal.WriteLine("\nYou must restart your computer for changes to take effect.");
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Change the current working directory
                case "cd":
                    // Check to make sure arguments exist
                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A directory name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    FSUtilities.ChangeDirectory(PathUtils.ListToPath(Arguments.ToList(), true));
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

                            if (Entry.mSize > 1024)
                            {
                                HTerminal.ColoredWriteLine($"{Entry.mSize / 1024d} KB", Color.Magenta);
                            }
                            else if (Entry.mSize > 0)
                            {
                                HTerminal.ColoredWriteLine($"{Entry.mSize} B", Color.Magenta);
                            }
                            else
                            {
                                HTerminal.ColoredWriteLine("0 B", Color.Magenta);
                            }
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
                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"You must provide a string.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    Terminal.WriteLine($"\"{PathUtils.GetValidPath(Arguments[1])}\"\n\r");
                    break;

                // Print a file's contents to the console
                case "cat":
                    try
                    {
                        if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        var FilePath = Path.GetFullPath(PathUtils.GetValidPath(Arguments[1]));

                        if (File.Exists(Path.GetFullPath(FilePath)) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{FilePath}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
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
                        if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        var FilePath = Path.GetFullPath(PathUtils.GetValidPath(Arguments[1]));                        

                        if (File.Exists(Path.GetFullPath(FilePath)) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{FilePath}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        Hex.PrintBase16(File.ReadAllBytes(Path.GetFullPath(FilePath)));
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                // Get ELF executable properties
                case "elf":
                    try
                    {
                        if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        var ELFPath = PathUtils.GetValidPath(Arguments[1]);

                        if (File.Exists(ELFPath) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{ELFPath}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        var ELFFile = new ELFInfo(ELFPath);

                        if (ELFFile.IsElfExecutable() == false)
                        {
                            ConsoleFunctions.PrintLogMSG("This file is not an ELF executable.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        var ELFProperties = ELFFile.GetExecutableProperties();

                        Terminal.WriteLine($"Magic number:    {ELFProperties[0]}");
                        Terminal.WriteLine($"Bit format:      {ELFProperties[1]}");
                        Terminal.WriteLine($"Endianness:      {ELFProperties[2]}");
                        Terminal.WriteLine($"Target OS ABI:   {ELFProperties[3]} ({ELFProperties[4]})");
                        Terminal.WriteLine($"ISA:             {ELFProperties[5]} ({ELFProperties[6]})");
                        Terminal.WriteLine($"Entry point:     {ELFProperties[7]}");
                        Terminal.WriteLine($"Version:         {ELFProperties[8]} (0x{ELFProperties[8]})\n\r");
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }
                    
                    break;

                // Get the checksum of a file
                case "checksum":
                    try
                    {
                        if (Arguments.Length <= 2 || string.IsNullOrWhiteSpace(Arguments[1]) || string.IsNullOrWhiteSpace(Arguments[2]))
                        {
                            ConsoleFunctions.PrintLogMSG($"A file name and checksum method must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        var FilePath = Path.GetFullPath(PathUtils.GetValidPath(Arguments[2]));

                        if (File.Exists(Path.GetFullPath(FilePath)) == false)
                        {
                            ConsoleFunctions.PrintLogMSG($"The file \"{FilePath}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                            break;
                        }

                        switch (Arguments[1])
                        {
                            case "--MD5":
                                Terminal.WriteLine($"{MD5.hash(File.ReadAllText(Path.GetFullPath(FilePath)))}\n\r");
                                break;

                            case "--SHA256":
                                Terminal.WriteLine($"{SHA256.ComputeHash(File.ReadAllBytes(Path.GetFullPath(FilePath)))}\n\r");
                                break;

                            case "--CRC16_ARC":
                                Terminal.WriteLine($"{CRC.CRC16_ARC(File.ReadAllBytes(Path.GetFullPath(FilePath)))}\n\r");
                                break;
                            
                            // Causes the kernel to explode lol
                            /*case "--CRC16_MODBUS":
                                Terminal.WriteLine($"{CRC.CRC16_MODBUS(File.ReadAllBytes(Path.GetFullPath(FilePath)))}\n\r");
                                break;*/                       

                            default:
                                ConsoleFunctions.PrintLogMSG($"Invalid checksum method: \"{Arguments[1]}\"\n\r", ConsoleFunctions.LogType.ERROR);
                                break;
                        }
                    }
                    catch (Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                case "edit":
                    try
                    {
                        if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                        {
                            MIV.MIV.StartMIV(null);
                        }
                        else
                        {
                            MIV.MIV.StartMIV(Path.GetFullPath(PathUtils.GetValidPath(Arguments[1])));
                        }
                    }
                    catch(Exception EX)
                    {
                        ConsoleFunctions.PrintLogMSG($"{EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                    }

                    break;

                case "lua":
                    if (Arguments.Length <= 1 || string.IsNullOrWhiteSpace(Arguments[1]))
                    {
                        ConsoleFunctions.PrintLogMSG($"A file name must be specified.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    var LuaPath = PathUtils.GetValidPath(Arguments[1]);

                    if (File.Exists(LuaPath) == false)
                    {
                        ConsoleFunctions.PrintLogMSG($"The file \"{LuaPath}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                        break;
                    }

                    Lua.RunLuaScript(LuaPath);
                    //Terminal.WriteLine();
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
                            ConsoleFunctions.PrintLogMSG("Shutdown failed!", ConsoleFunctions.LogType.ERROR);
                            break;

                        case "-r":
                            ConsoleFunctions.PrintLogMSG("Rebooting...\n\n\r", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(250);
                            Sys.Power.Reboot();
                            ConsoleFunctions.PrintLogMSG("Shutdown failed!", ConsoleFunctions.LogType.ERROR);
                            break;

                        case "-as":
                            ConsoleFunctions.PrintLogMSG("Shutting down (ACPI)...\n\n\r", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(250);
                            ACPI.Shutdown();
                            ConsoleFunctions.PrintLogMSG("ACPI shutdown failed!", ConsoleFunctions.LogType.ERROR);
                            break;

                        case "-ar":
                            ConsoleFunctions.PrintLogMSG("Rebooting (ACPI)...\n\n\r", ConsoleFunctions.LogType.INFO);
                            Thread.Sleep(250);
                            ACPI.Reboot();
                            ConsoleFunctions.PrintLogMSG("ACPI reboot failed!", ConsoleFunctions.LogType.ERROR);
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
                        $"USED: {UsedRAM}/{TotalInstalledRAM} KB ({MathHelpers.TruncateToDecimalPlace((UsedRAM / TotalInstalledRAM) * 100f, 4)}%)\n" +
                        $"Allocated objects{HeapSmall.GetAllocatedObjectCount()}\n");
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

                // Print operating system information
                case "about":
                    ConsoleFunctions.PrintLogMSG($"{OSName} {OSVersion} - {OSDate}\n\r" +
                        $"By: {OSAuthor} (see Contributors.txt for more)\n\r" +
                        $"Kernel started at {KernelStartTime.ToString()}.\n\r" +
                        $"Terminal size: {Terminal.Width}x{Terminal.Height} ({UserInterface.ScreenWidth}x{UserInterface.ScreenHeight}, {TerminalDriver.Replace("Canvas", "")})\n\n\r", ConsoleFunctions.LogType.NONE);
                    break;



                // ** EXTRA **
                // Print what the user entered as an argument
                case "echo":
                case "print":
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

