using Cosmos.System.FileSystem;
using Cosmos.Core.Memory;
using System.IO;
using System;
using IO.CMD;
using BlackOpal.IO.Filesystem;
using PrismAPI.Graphics;

namespace BlackOpal.Utilities.Installers
{
    public class OSInstaller
    {
        public static void Init(bool SecondStage = false, int SecondStageDisk = -1)
        {
            if (SecondStage)
            {
                // Copy files from the installation media to the formatted partition
                if (Directory.Exists(Kernel.BootMediumPath) == false)
                {
                    throw new DirectoryNotFoundException($"The boot medium path (\"{Kernel.BootMediumPath}\") doesn't exist.");
                }

                Kernel.Terminal.Write($"Copying files from \"{Kernel.BootMediumPath}\" -> \"{Kernel.FS.Disks[SecondStageDisk].Partitions[0].RootPath}\"... ");

                foreach (var FileToCopy in Kernel.FS.GetDirectoryListing(Kernel.BootMediumPath))
                {
                    if (File.Exists(FileToCopy.mFullPath))
                    {
                        try
                        {
                            FSUtilities.CopyItem(Path.Combine(Kernel.BootMediumPath, FileToCopy.mName), Kernel.FS.Disks[SecondStageDisk].Partitions[0].RootPath);
                        }
                        catch (Exception EX)
                        {
                            Exit();
                            ConsoleFunctions.PrintLogMSG($"Installation failed: {EX.Message}\n\n\r", ConsoleFunctions.LogType.ERROR);
                            return;
                        }
                    }
                }

                Kernel.HTerminal.ColoredWriteLine("[DONE]", Color.Green);
                Kernel.Terminal.WriteLine("Installation finished!\n\n\r");
                return;
            }

            Kernel.Terminal.BackgroundColor = Color.StackOverflowBlack;
            Kernel.Terminal.ForegroundColor = Color.StackOverflowWhite;
            Kernel.Terminal.Clear();
            Kernel.Terminal.WriteLine($"[===== {Kernel.OSName} INSTALLER =====]");
            Kernel.Terminal.WriteLine("Choose an option:\n1. Install\n2. Quit\n");

            int DiskIndex = 0;
            string Choice = "";

            // Get the user's choice
            while (true)
            {
                Kernel.Terminal.Write(">> ");
                Choice = Kernel.Terminal.ReadKey().KeyChar.ToString();

                if (Choice == "1")
                {
                    break;
                }
                else if (Choice == "2")
                {
                    Exit();
                    return;
                }
                else
                {
                    Kernel.Terminal.WriteLine();
                    ConsoleFunctions.PrintLogMSG($"\nInvalid choice: \"{Choice}\"\n\n\r", ConsoleFunctions.LogType.ERROR);
                }

                Heap.Collect();
            }

            // Get the disk to install to
            Kernel.Terminal.WriteLine($"\n\nChoose a disk to install to:");

            foreach (var Disk in Kernel.FS.Disks)
            {
                if (Disk.Partitions.Count > 0 && Disk.Partitions[0].HasFileSystem)
                {
                    Kernel.Terminal.WriteLine($"{DiskIndex}. {Disk.Partitions[0].RootPath}");
                }
                else
                {
                    Kernel.Terminal.WriteLine($"{DiskIndex}. No filesystem ({Disk.Size / 1024} KB)");
                }

                DiskIndex++;
            }

            Kernel.Terminal.WriteLine();

            while (true)
            {
                Kernel.Terminal.Write(">> ");
                Choice = Kernel.Terminal.ReadKey().KeyChar.ToString();

                if (Int32.TryParse(Choice, out DiskIndex))
                {
                    Kernel.Terminal.WriteLine($"\nSelected disk #{DiskIndex}.");
                    break;
                }
                else
                {
                    Kernel.Terminal.WriteLine();
                    ConsoleFunctions.PrintLogMSG($"\nInvalid choice: \"{Choice}\"\n\n\r", ConsoleFunctions.LogType.ERROR);
                }

                if (DiskIndex > Kernel.FS.Disks.Count - 1)
                {
                    ConsoleFunctions.PrintLogMSG($"\nInvalid choice: \"{Choice}\"\n\n\r", ConsoleFunctions.LogType.ERROR);
                }

                Heap.Collect();
            }

            // Format and partition the disk
            FSUtilities.FormatDisk(DiskIndex, true);

            // Write the installation marker to the disk right after the MBR
            var BlockData = new byte[512];

            BlockData[0] = (byte)Kernel.InstallationMarker;
            Kernel.FS.Disks[DiskIndex].Partitions[0].Host.WriteBlock(1, 1, ref BlockData);

            Kernel.Terminal.WriteLine("The system must reboot in order for the formatting changes to take effect.\nInstallation will continue once the system reboots.\n\nPress any key to restart.");
            Kernel.Terminal.ReadKey();
            Cosmos.System.Power.Reboot();
        }   

        private static void Exit()
        {
            Kernel.Terminal.BackgroundColor = Kernel.TerminalColor;
            Kernel.Terminal.BackgroundColor = Color.Black;
            Kernel.Terminal.ForegroundColor = Color.StackOverflowWhite;
            Kernel.Terminal.Clear();
        }
    }
}
