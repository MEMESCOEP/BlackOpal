using Cosmos.Core.Memory;
using PrismAPI.Graphics;
using System;

namespace BlackOpal.Utilities.Converters
{
    internal class Hex
    {
        // Convert a byte array to Base 16 formatted terminal output
        public static void PrintBase16(byte[] Stream)
        {
            // Temporary variables
            string ByteHex = "00";
            int LineNumber = 1;

            // Print the first line number
            Kernel.HTerminal.ColoredWrite($"{LineNumber}: ", Color.GoogleGreen);

            // Loop through each byte in the file
            for (long ByteIndex = 0; ByteIndex < Stream.Length; ByteIndex++)
            {
                if (Console.KeyAvailable)
                {
                    if (Kernel.Terminal.ReadKey().Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }

                // Convert the byte to hex
                ByteHex = $"{Convert.ToInt32(Stream[ByteIndex]):X}";

                // Add a leading zero if the byte is <= 15 (this is for formatting purposes)
                if (Convert.ToInt32(ByteHex, 16) <= 15)
                {
                    ByteHex = ByteHex.Insert(0, "0");
                }

                // Print the hex byte
                Kernel.Terminal.Write($"{ByteHex} ");

                // If more than 12 characters have been printed on the current line, start a new line
                if ((ByteIndex + 1) % 16 == 0 && ByteIndex > 0 && Stream.Length != 16)
                {
                    LineNumber++;
                    Kernel.Terminal.WriteLine();
                    Kernel.HTerminal.ColoredWrite($"{LineNumber}: ", Color.GoogleGreen);

                    // Collect garbage so the OS doesn't crash
                    Heap.Collect();
                }
            }

            Kernel.Terminal.WriteLine("\n\r");
        }

        // Convert a byte array to Base 16 string
        public static string Base16String(byte[] Stream)
        {
            // Temporary variables
            string FinalString = "";

            // Loop through each byte in the file
            for (long ByteIndex = 0; ByteIndex < Stream.Length; ByteIndex++)
            {
                if (Console.KeyAvailable)
                {
                    if (Kernel.Terminal.ReadKey().Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }

                // Convert the byte to hex
                FinalString += $"{Convert.ToInt32(Stream[ByteIndex]):X}";


            }

            return FinalString;
        }
    }
}
