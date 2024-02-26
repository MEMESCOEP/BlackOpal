/* [===== CONSOLE FUNCTIONS =====] */
/* DIRECTIVES */
using BlackOpal;
using Cosmos.HAL;
using PrismAPI.Graphics;
using System;

/* NAMESPACES */
namespace IO.CMD
{
    /* CLASSES */
    internal class ConsoleFunctions
    {
        /* VARIABLES */
        // Enums
        public enum LogType
        {
            WARNING,
            DEBUG,
            ERROR,
            FATAL,
            INFO,
            NONE
        }

        /* FUNCTIONS */
        // This will print a message to the console with the specified log type. I would use Enum.ToString() here, but it's not currently
        // implemented in Cosmos as of 10-22-2023
        public static void PrintLogMSG(string msg, LogType type)
        {
            if (Kernel.Terminal != null)
                Kernel.Terminal.ForegroundColor = Color.White;

            switch (type)
            {
                case LogType.WARNING:
                    if (Kernel.Terminal != null)
                    {
                        Kernel.Terminal.Write("[");
                        Kernel.HTerminal.ColoredWrite("WARN", Color.Yellow);
                        Kernel.Terminal.Write($"] >> {msg}");
                    }
                        
                    SerialPort.SendString($"[WARN] >> {msg}");
                    break;

                case LogType.DEBUG:
                    if (Kernel.Terminal != null)
                    {
                        Kernel.Terminal.Write("[");
                        Kernel.HTerminal.ColoredWrite("DEBUG", Color.Cyan);
                        Kernel.Terminal.Write($"] >> {msg}");
                    }

                    SerialPort.SendString($"[DEBUG] >> {msg}");
                    break;

                case LogType.ERROR:
                    if (Kernel.Terminal != null)
                    {
                        Kernel.Terminal.Write("[");
                        Kernel.HTerminal.ColoredWrite("ERROR", Color.Red);
                        Kernel.Terminal.Write($"] >> {msg}");
                    }                        
                    
                    SerialPort.SendString($"[ERROR] >> {msg}");

                    break;

                case LogType.FATAL:
                    if (Kernel.Terminal != null)
                    {
                        Kernel.Terminal.Write("[");
                        Kernel.HTerminal.ColoredWrite("FATAL", Color.RubyRed);
                        Kernel.Terminal.Write($"] >> {msg}");
                    }
                    
                    SerialPort.SendString($"[FATAL] >> {msg}");

                    break;

                case LogType.INFO:
                    SerialPort.SendString($"[INFO] >> {msg}");

                    if (Kernel.Terminal != null)
                        Kernel.Terminal.Write($"[INFO] >> {msg}");

                    break;

                case LogType.NONE:
                default:
                    SerialPort.SendString(msg);

                    if (Kernel.Terminal != null)
                        Kernel.Terminal.Write(msg);

                    break;
            }

            if (Kernel.Terminal != null)
                Kernel.Terminal.ForegroundColor = Kernel.TerminalColor;
        }
    }
}
