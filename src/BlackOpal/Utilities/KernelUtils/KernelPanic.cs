using Cosmos.Core;
using Cosmos.HAL;
using System;
using IO.CMD;
using PrismAPI.Graphics;

namespace BlackOpal.Utilities.KernelUtils
{
    internal class KernelPanic
    {
        // Print an error message and halt the system
        public static void Panic(string Msg, string ErrorCode)
        {
            var IntErrorCode = Convert.ToInt32(ErrorCode);

            Kernel.Terminal.ForegroundColor = Color.StackOverflowWhite;
            Kernel.HTerminal.ColoredWrite("[===== ", Color.StackOverflowWhite);
            Kernel.HTerminal.ColoredWrite("KERNEL PANIC", Color.Red);
            Kernel.HTerminal.ColoredWriteLine(" =====]", Color.StackOverflowWhite);
            ConsoleFunctions.PrintLogMSG($"{Msg}\n\rError code: ", ConsoleFunctions.LogType.FATAL);

            if (IntErrorCode < 0)
            {
                Kernel.HTerminal.ColoredWrite("-", Color.GoogleRed);
                IntErrorCode = Math.Abs(IntErrorCode);
            }

            Kernel.HTerminal.ColoredWriteLine($"0x{IntErrorCode:X}\n\r", Color.GoogleRed);
            Kernel.HTerminal.ColoredWrite("The system has been halted, please restart the computer.", Color.StackOverflowWhite);
            //PCSpeaker.Beep(400, 1000);

            while (true)
            {
                Cosmos.System.Kernel.PrintDebug($"[KERNEL PANIC] >> {Msg} (0x{IntErrorCode:X})");
                Cosmos.Core.Global.debugger.Break();
                CPU.Halt();
            }
        }
    }
}
