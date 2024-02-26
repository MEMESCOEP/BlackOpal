using Cosmos.Core;
using Cosmos.HAL;
using IO.CMD;
using PrismAPI.Graphics;

namespace BlackOpal
{
    internal class KernelPanic
    {
        public static void Panic(string Msg, string ErrorCode)
        {
            Kernel.Terminal.ForegroundColor = Color.StackOverflowWhite;
            Kernel.HTerminal.ColoredWriteLine("[===== KERNEL PANIC =====]", Color.StackOverflowWhite);
            ConsoleFunctions.PrintLogMSG($"{Msg}\n\rError code: ", ConsoleFunctions.LogType.FATAL);
            Kernel.HTerminal.ColoredWriteLine(ErrorCode, Color.GoogleRed);
            Kernel.HTerminal.ColoredWrite("\n\rThe system has been halted.", Color.StackOverflowWhite);
            PCSpeaker.Beep(500, 250);
            CPU.mInterruptsEnabled = false;
            while (true)
                CPU.Halt();
        }
    }
}
