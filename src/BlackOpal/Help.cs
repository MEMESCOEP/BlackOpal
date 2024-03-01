using HydrixLIB;
using PrismAPI.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackOpal
{
    internal class Help
    {
        public static void ShowHelp(string Command = "")
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

                case "echo":
                    Kernel.HTerminal.ColoredWrite("echo - Print a message to the console\n", Color.Yellow);
                    break;

                case "power":
                    Kernel.HTerminal.ColoredWrite("power - Shut down or restart the computer\n", Color.Yellow);
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
                    Kernel.HTerminal.ColoredWrite("ping - Ping a device on the network\n", Color.Yellow);
                    break;

                case "ntp":
                    Kernel.HTerminal.ColoredWrite("ntp - Get the current date/time from an NTP server\n", Color.Yellow);
                    break;

                case "mkf":
                    Kernel.HTerminal.ColoredWrite("mkf - Make a file\n", Color.Yellow);
                    break;

                case "mkdir":
                    Kernel.HTerminal.ColoredWrite("mkdir - Make a directory\n", Color.Yellow);
                    break;

                case "rm":
                    Kernel.HTerminal.ColoredWrite("rm - Delete a file or directory\n", Color.Yellow);
                    break;

                case "cd":
                    Kernel.HTerminal.ColoredWrite("cd - Change the current working directory\n", Color.Yellow);
                    break;

                case "dir":
                case "ls":
                    Kernel.HTerminal.ColoredWrite("dir/ls - Get a directory listing\n", Color.Yellow);
                    break;

                case "cat":
                    Kernel.HTerminal.ColoredWrite("cat - Print a file's contents to the console\n", Color.Yellow);
                    break;

                default:
                    Kernel.HTerminal.ColoredWrite($"Invalid command: \"Command\"\n", Color.Red);
                    break;

            }
        }
    }
}