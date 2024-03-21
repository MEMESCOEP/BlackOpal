using BlackOpal.Utilities.Calculations.Checksum;
using BlackOpal.Utilities.Converters;
using IO.CMD;
using PrismAPI.Graphics;
using System;
using System.IO;

namespace BlackOpal.IO.Filesystem
{
    internal class FSUtilities
    {
        // Create a file with the specified name, in the specified directory
        public static void CreateFile(string FilePath)
        {
            var NewFileName = PathUtils.GetValidPath(FilePath);

            if (Path.GetFileName(NewFileName).Split('.')[0].Length > 8)
            {
                ConsoleFunctions.PrintLogMSG($"The filename cannot be greater than 8 characters.\n\n\r", ConsoleFunctions.LogType.ERROR);
                return;
            }

            if (Path.GetFileName(NewFileName).Split('.')[1].Length > 3)
            {
                ConsoleFunctions.PrintLogMSG($"The file extension cannot be greater than w characters.\n\n\r", ConsoleFunctions.LogType.ERROR);
                return;
            }

            if (File.Exists(NewFileName) == true)
            {
                ConsoleFunctions.PrintLogMSG($"The file \"{NewFileName}\" already exists.\n\n\r", ConsoleFunctions.LogType.ERROR);
                return;
            }

            NewFileName = Path.GetFullPath(NewFileName);
            Kernel.FS.CreateFile(NewFileName);
        }

        // Create a directory with the specified name, in the specified directory
        public static void CreateDirectory(string FilePath)
        {
            var NewDirName = PathUtils.GetValidPath(FilePath);

            if (Directory.Exists(NewDirName) == true)
            {
                ConsoleFunctions.PrintLogMSG($"The directory \"{NewDirName}\" already exists.\n\n\r", ConsoleFunctions.LogType.ERROR);
                return;
            }

            NewDirName = Path.GetFullPath(NewDirName);
            Kernel.FS.CreateDirectory(NewDirName);
        }

        // Copy a filesystem item to a new destination
        public static void CopyItem(string Source, string Destination, bool IsDirectory = false)
        {
            if (IsDirectory)
            {
                var DirectoryToCopy = PathUtils.GetValidPath(Source);
                var DestinationDirectory = PathUtils.GetValidPath(Destination);

                Kernel.Terminal.WriteLine($"\"{DirectoryToCopy}\" -> \"{DestinationDirectory}\"...");

                if (Directory.Exists(DestinationDirectory) == true)
                {
                    ConsoleFunctions.PrintLogMSG($"The source directory \"{DirectoryToCopy}\" already exists in \"{Path.GetDirectoryName(DestinationDirectory)}\".\n\n\r", ConsoleFunctions.LogType.ERROR);
                    return;
                }

                if (Directory.Exists(Path.GetDirectoryName(DestinationDirectory)) == false)
                {
                    ConsoleFunctions.PrintLogMSG($"The destination directory \"{Path.GetDirectoryName(DestinationDirectory)}\" doesn't exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                    return;
                }

                //Directory.Copy(DirectoryToCopy, Destination);
            }
            else
            {
                var FileToCopy = PathUtils.GetValidPath(Source);
                var DestinationFile = PathUtils.GetValidPath(Path.Combine(Destination, Path.GetFileName(FileToCopy)));

                Kernel.Terminal.WriteLine($"\"{FileToCopy}\" -> \"{DestinationFile}\"...");

                if (File.Exists(FileToCopy) == false)
                {
                    throw new Exception($"The source file \"{FileToCopy}\" doesn't exist.");
                }

                if (File.Exists(DestinationFile) == true)
                {
                    throw new Exception($"The source file \"{FileToCopy}\" already exists in \"{Path.GetDirectoryName(DestinationFile)}\".");
                }

                if (Directory.Exists(Path.GetDirectoryName(DestinationFile)) == false)
                {
                    throw new Exception($"The destination directory \"{Path.GetDirectoryName(DestinationFile)}\" doesn't exist.");
                }

                File.Copy(FileToCopy, DestinationFile);

                Kernel.Terminal.WriteLine("Verifying data integrity...");
                if (SHA256.ComputeHash(File.ReadAllBytes(FileToCopy)) != SHA256.ComputeHash(File.ReadAllBytes(DestinationFile)))
                {
                    File.Delete(DestinationFile);
                    throw new Exception($"Copy failed. Source and destination file checksums are not equal.");
                }
            }

            Kernel.Terminal.WriteLine("Done.\n\r");
        }

        // Move a filesystem item to a new destination
        public static void MoveItem(string Source, string Destination, bool IsDirectory = false)
        {
            if (IsDirectory)
            {
                var DirectoryToCopy = PathUtils.GetValidPath(Source);
                var DestinationDirectory = PathUtils.GetValidPath(Destination);

                Kernel.Terminal.WriteLine($"\"{DirectoryToCopy}\" -> \"{DestinationDirectory}\"...");

                if (Directory.Exists(DestinationDirectory) == true)
                {
                    ConsoleFunctions.PrintLogMSG($"The source directory \"{DirectoryToCopy}\" already exists in \"{Path.GetDirectoryName(DestinationDirectory)}\".\n\n\r", ConsoleFunctions.LogType.ERROR);
                    return;
                }

                if (Directory.Exists(Path.GetDirectoryName(DestinationDirectory)) == false)
                {
                    ConsoleFunctions.PrintLogMSG($"The destination directory \"{Path.GetDirectoryName(DestinationDirectory)}\" doesn't exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                    return;
                }

                //Directory.Copy(DirectoryToCopy, Destination);
            }
            else
            {
                var FileToMove = PathUtils.GetValidPath(Source);
                var DestinationFile = PathUtils.GetValidPath(Path.Combine(Destination, Path.GetFileName(FileToMove)));

                Kernel.Terminal.WriteLine($"\"{FileToMove}\" -> \"{DestinationFile}\"...");

                if (File.Exists(FileToMove) == false)
                {
                    ConsoleFunctions.PrintLogMSG($"The source file \"{FileToMove}\" doesn't exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                    return;
                }

                if (File.Exists(DestinationFile) == true)
                {
                    ConsoleFunctions.PrintLogMSG($"The source file \"{FileToMove}\" already exists in \"{Path.GetDirectoryName(DestinationFile)}\".\n\n\r", ConsoleFunctions.LogType.ERROR);
                    return;
                }

                if (Directory.Exists(Path.GetDirectoryName(DestinationFile)) == false)
                {
                    ConsoleFunctions.PrintLogMSG($"The destination directory \"{Path.GetDirectoryName(DestinationFile)}\" doesn't exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                    return;
                }

                File.Copy(FileToMove, Destination);

                Kernel.Terminal.WriteLine("Verifying data integrity...");
                if (SHA256.ComputeHash(File.ReadAllBytes(FileToMove)) != SHA256.ComputeHash(File.ReadAllBytes(DestinationFile)))
                {
                    ConsoleFunctions.PrintLogMSG($"Move failed. Source and destination file checksums are not equal.\n\n\r", ConsoleFunctions.LogType.ERROR);
                    File.Delete(DestinationFile);
                    return;
                }

                File.Delete(FileToMove);
            }
            
            Kernel.Terminal.WriteLine("Done.\n\r");
        }

        // Remove a file or directory with the specified name, in the specified directory
        public static void RemoveItem(string FilePath, bool IsDirectory = false)
        {
            var NewPathName = PathUtils.GetValidPath(FilePath);

            NewPathName = Path.GetFullPath(NewPathName);

            if (File.Exists(NewPathName))
            {
                File.Delete(NewPathName);
            }
            else if (Directory.Exists(NewPathName) && IsDirectory)
            {
                Directory.Delete(NewPathName);
            }
            else
            {
                ConsoleFunctions.PrintLogMSG($"The item \"{NewPathName}\" doesn't exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
            }
        }

        // Change the current working directory to the specified directory
        public static void ChangeDirectory(string FilePath)
        {
            // Get a valid path from the above path
            var Dir = PathUtils.GetValidPath(FilePath);

            // Make sure the valid directory exists
            if (Directory.Exists(Dir) == false)
            {
                ConsoleFunctions.PrintLogMSG($"The directory \"{Dir}\" does not exist.\n\n\r", ConsoleFunctions.LogType.ERROR);
                return;
            }

            // Change to the new directory
            Dir = Path.GetFullPath(Dir);
            Directory.SetCurrentDirectory(Dir);
        }

        public static void FormatDisk(int DiskIndex, bool QuickFormat = true)
        {
            for(int PartitionIndex = 0; PartitionIndex < Kernel.FS.Disks[DiskIndex].Partitions.Count; PartitionIndex++)
            {
                Kernel.Terminal.Write($"Deleting partition #{PartitionIndex}... ");
                Kernel.FS.Disks[DiskIndex].DeletePartition(PartitionIndex);
                Kernel.HTerminal.ColoredWriteLine("[DONE]", Color.Green);
            }

            // Create a new partition
            Kernel.Terminal.Write("Creating partition #0... ");
            Kernel.FS.Disks[DiskIndex].CreatePartition(Kernel.FS.Disks[DiskIndex].Size / 1024 / 1024);
            Kernel.HTerminal.ColoredWriteLine("[DONE]", Color.Green);

            // Format a partition
            Kernel.Terminal.Write("Formatting partition #0 (FAT32)... ");
            Kernel.FS.Disks[DiskIndex].FormatPartition(0, "FAT32", QuickFormat);
            Kernel.HTerminal.ColoredWriteLine("[DONE]", Color.Green);
            Kernel.Terminal.WriteLine($"Root path is: \"{Kernel.FS.Disks[DiskIndex].Partitions[0].RootPath}\".\n\r");

            // Reinitialize the VFS so formatting changes take effect without a reboot
            Kernel.Terminal.Write("Reinitializing VFS... ");
            Kernel.FS.Disks.Clear();
            Kernel.FS.Initialize(true);
            Kernel.BootMediumPath = Kernel.FS.Disks[Kernel.BootMediumIndex].Partitions[0].RootPath;
            Kernel.HTerminal.ColoredWriteLine("[DONE]", Color.Green);
        }
    }
}
