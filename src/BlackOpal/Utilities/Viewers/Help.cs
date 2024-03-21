using PrismAPI.Graphics;

namespace BlackOpal.Utilities.Viewers
{
    internal class Help
    {
        /* FUNCTIONS */
        public static void ShowHelp(string Command = "")
        {
            // Check if there are any arguments
            if (string.IsNullOrEmpty(Command))
            {
                /// #############################
                /// Azureian: Please try to keep each header in the help message the same length, it looks better, thanks! (Optionally, you can make it divisible by 2 and add/remove '-' to both sides)
                /// Current length: 8 characters
                /// #############################
                Kernel.HTerminal.ColoredWriteLine("Black Opal Help", Color.Green);
                Kernel.HTerminal.ColoredWriteLine("Commands:", Color.Green);

                // Write header Graphics
                Kernel.HTerminal.ColoredWriteLine("  ------------------------------GRAPHICS------------------------------", Color.GoogleBlue);
                Kernel.HTerminal.ColoredWriteLine("  gui - Start the GUI", Color.GoogleBlue);
                Kernel.HTerminal.ColoredWriteLine("  --------------------------------------------------------------------", Color.GoogleBlue);
                Kernel.Terminal.WriteLine();

                Kernel.HTerminal.ColoredWriteLine("  ------------------------------TERMINAL------------------------------", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  clear/cls - Clear the console", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  help - Display this help message", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  echo/print - Print a message to the console", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  power - Shut down or restart the computer", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  sysinfo - Get system information", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  raminfo - Get RAM information", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  diskinfo - Get disk information", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  about - Display OS information", Color.GoogleGreen);
                Kernel.HTerminal.ColoredWriteLine("  --------------------------------------------------------------------", Color.GoogleGreen);
                Kernel.Terminal.WriteLine();

                Kernel.HTerminal.ColoredWriteLine("  ------------------------------NETWORKING-----------------------------", Color.GoogleYellow);
                Kernel.HTerminal.ColoredWriteLine("  netinfo - Get network information", Color.GoogleYellow);
                Kernel.HTerminal.ColoredWriteLine("  netinit - Initialize the NIC", Color.GoogleYellow);
                Kernel.HTerminal.ColoredWriteLine("  nicinfo - Get NIC information", Color.GoogleYellow);
                Kernel.HTerminal.ColoredWriteLine("  ping - Ping a device on the network", Color.GoogleYellow);
                Kernel.HTerminal.ColoredWriteLine("  ntp - Get the current date/time from an NTP server", Color.GoogleYellow);
                Kernel.HTerminal.ColoredWriteLine("  ---------------------------------------------------------------------", Color.GoogleYellow);
                Kernel.Terminal.WriteLine();

                Kernel.HTerminal.ColoredWriteLine("  ------------------------------FILE MGMT------------------------------", Color.GoogleRed); // File Management
                Kernel.HTerminal.ColoredWriteLine("  mkf - Make a file", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  mkdir - Make a directory", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  rm - Delete a file or directory", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  cd - Change the current working directory", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  dir/ls - Get a directory listing", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  cat - Print a file's contents to the console", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  edit - Edit a text file", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  format - format an entire disk", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  cp/copy - copy a file or directory to a new destination", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  mv/move - move a file or directory to a new destination", Color.GoogleRed);
                Kernel.HTerminal.ColoredWriteLine("  ---------------------------------------------------------------------", Color.GoogleRed);
                Kernel.Terminal.Write("Press any key to continue.");
                Kernel.Terminal.ReadKey(true);
                Kernel.Terminal.Clear();

                Kernel.HTerminal.ColoredWriteLine("  ------------------------------UTILITIES------------------------------", Color.Cyan); // Utilities
                Kernel.HTerminal.ColoredWriteLine("  hex - print a file's contents in base 16 hexadecimal", Color.Cyan);
                Kernel.HTerminal.ColoredWriteLine("  checksum - get the checksum of a file.", Color.Cyan);
                Kernel.HTerminal.ColoredWriteLine("  validpath - convert a string into a valid path", Color.Cyan);
                Kernel.HTerminal.ColoredWriteLine("  ---------------------------------------------------------------------", Color.Cyan);
                Kernel.Terminal.WriteLine();

                Kernel.HTerminal.ColoredWriteLine("  ------------------------------DEBUG CMD------------------------------", Color.Magenta);
                Kernel.HTerminal.ColoredWriteLine("  panic - Force a kernel panic for debugging", Color.Magenta);
                Kernel.HTerminal.ColoredWriteLine("  ---------------------------------------------------------------------", Color.Magenta);
                Kernel.Terminal.WriteLine();

                Kernel.HTerminal.ColoredWrite("Type 'help <command>' for more information on a specific command.\n\n", Color.Green);
            }
            else
            {
                // Check if the argument is a valid command
                switch (Command)
                {
                    case "clear":
                    case "cls":
                        Kernel.HTerminal.ColoredWrite("clear/cls - Clear the console\n", Color.Yellow);
                        break;

                    case "help":
                        Kernel.HTerminal.ColoredWrite("help - Display this help message\n", Color.Yellow);
                        break;

                    case "about":
                        Kernel.HTerminal.ColoredWrite("about - Display OS information\n", Color.Yellow);
                        break;

                    case "echo":
                        Kernel.HTerminal.ColoredWrite("echo/print <message> - Print a message to the console\n", Color.Yellow);
                        break;

                    case "power":
                        Kernel.HTerminal.ColoredWrite("power <power option> - Shut down or restart the computer\n", Color.Yellow);
                        break;

                    case "sysinfo":
                        Kernel.HTerminal.ColoredWrite("sysinfo - Get system information\n", Color.Yellow);
                        break;

                    case "raminfo":
                        Kernel.HTerminal.ColoredWrite("raminfo - Get RAM information\n", Color.Yellow);
                        break;

                    case "diskinfo":
                        Kernel.HTerminal.ColoredWrite("diskinfo - Get disk information\n", Color.Yellow);
                        break;

                    case "netinfo":
                        Kernel.HTerminal.ColoredWrite("netinfo - Get network information\n", Color.Yellow);
                        break;

                    case "netinit":
                        Kernel.HTerminal.ColoredWrite("netinit - Initialize the NIC\n", Color.Yellow);
                        break;

                    case "nicinfo":
                        Kernel.HTerminal.ColoredWrite("nicinfo - Get NIC information\n", Color.Yellow);
                        break;

                    case "ping":
                        Kernel.HTerminal.ColoredWrite("ping <IPv4 address> - Ping a device on the network\n", Color.Yellow);
                        break;

                    case "ntp":
                        Kernel.HTerminal.ColoredWrite("ntp - Get the current date/time from an NTP server\n", Color.Yellow);
                        break;

                    case "mkf":
                        Kernel.HTerminal.ColoredWrite("mkf <Filename> - Make a file\n", Color.Yellow);
                        break;

                    case "mkdir":
                        Kernel.HTerminal.ColoredWrite("mkdir <Dir name> - Make a directory\n", Color.Yellow);
                        break;

                    case "rm":
                        Kernel.HTerminal.ColoredWrite("rm <Filename> - Delete a file or directory\n", Color.Yellow);
                        break;

                    case "cd":
                        Kernel.HTerminal.ColoredWrite("cd <Dir name> - Change the current working directory\n", Color.Yellow);
                        break;

                    case "dir":
                    case "ls":
                        Kernel.HTerminal.ColoredWrite("dir/ls <dir name; optional> - Get a directory listing\n", Color.Yellow);
                        break;

                    case "cat":
                        Kernel.HTerminal.ColoredWrite("cat <Filename> - Print a file's contents to the console\n", Color.Yellow);
                        break;

                    case "hex":
                        Kernel.HTerminal.ColoredWrite("hex <Filename> - print a file's contents in base 16 hexadecimal\n", Color.Yellow);
                        break;
                    
                    case "validpath":
                        Kernel.HTerminal.ColoredWrite("validpath <String> - convert a string into a valid path\n", Color.Yellow);
                        break;

                    case "edit":
                        Kernel.HTerminal.ColoredWrite("edit <Filename> - Edit a text file\n", Color.Yellow);
                        break;
                    
                    case "format":
                        Kernel.HTerminal.ColoredWrite("format - format an entire disk\"\n", Color.Yellow);
                        break;

                    case "panic":
                        Kernel.HTerminal.ColoredWrite("panic - Force a kernel panic for debugging\n", Color.Yellow);
                        break;

                    default:
                        Kernel.HTerminal.ColoredWrite($"Invalid command: \"{Command}\"\n", Color.Red);
                        break;

                }

                Kernel.Terminal.WriteLine();
            }
        }
    }
}