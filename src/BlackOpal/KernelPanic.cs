using Cosmos.Core;
using Cosmos.HAL;
using IO.CMD;
using PrismAPI.Graphics;
using System;
using System.Threading;

namespace BlackOpal
{
    internal class KernelPanic
    {
        public static void Panic(string Msg, string ErrorCode)
        {
            var IntErrorCode = Convert.ToInt32(ErrorCode);

            Kernel.Terminal.ForegroundColor = Color.StackOverflowWhite;
            Kernel.HTerminal.ColoredWriteLine("[===== KERNEL PANIC =====]", Color.StackOverflowWhite);
            ConsoleFunctions.PrintLogMSG($"{Msg}\n\rError code: ", ConsoleFunctions.LogType.FATAL);

            if (IntErrorCode < 0)
            {
                Kernel.HTerminal.ColoredWrite("-", Color.GoogleRed);
                IntErrorCode = Math.Abs(IntErrorCode);
            }

            Kernel.HTerminal.ColoredWriteLine($"0x{IntErrorCode:X}", Color.GoogleRed);
            Kernel.HTerminal.ColoredWrite("\n\rThe system has been halted.", Color.StackOverflowWhite);
            PCSpeaker.Beep(400, 1000);

            while (true)
            {
                CPU.Halt();
            } 
        }
    }
}
