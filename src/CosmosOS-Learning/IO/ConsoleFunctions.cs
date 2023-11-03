/* [===== CONSOLE FUNCTIONS =====] */
/* DIRECTIVES */
using System;

/* NAMESPACES */
namespace IO
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
        // This will print a message to the console with the specified log type. I would use Enum.ToString() here, buit it's not currently
        // implemented in Cosmos as of 10-22-2023
        public static void PrintLogMSG(string msg, LogType type)
        {
            switch (type)
            {
                case LogType.WARNING:
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("WARN");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"] >> {msg}");
                    break;

                case LogType.DEBUG:
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("DEBUG");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"] >> {msg}");
                    break;

                case LogType.ERROR:
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"] >> {msg}");
                    break;

                case LogType.FATAL:
                    Console.Write("[");
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("FATAL");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write($"] >> {msg}");
                    break;

                case LogType.INFO:
                    Console.Write($"[INFO] >> {msg}");
                    break;

                case LogType.NONE:
                    Console.Write(msg);
                    break;

                default:
                    Console.Write(msg);
                    break;
            }
        }
    }
}
