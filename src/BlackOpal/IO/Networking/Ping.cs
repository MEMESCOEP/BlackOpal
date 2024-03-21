using System.Threading;
using IO.Networking;
using IO.CMD;

namespace BlackOpal.IO.Networking
{
    internal class Ping
    {
        public static void PingAddress(string Address)
        {
            if (Network.IsConfigured() == false)
            {
                ConsoleFunctions.PrintLogMSG($"A NIC must be properly configured before networking can be used.\n\n\r", ConsoleFunctions.LogType.ERROR);
                return;
            }

            if (Network.IsIPv4AddressValid(Address) == false)
            {
                ConsoleFunctions.PrintLogMSG($"\"{Address}\" is not a valid IP address.\n\n\r", ConsoleFunctions.LogType.ERROR);
                return;
            }

            Kernel.Terminal.WriteLine($"Pinging \"{Address}\":");
            int SuccessCounter = 0;

            for (int i = 0; i < 4; i++)
            {
                float PingTime = Network.ICMPPing(Network.StringToAddress(Address));

                if (PingTime >= 0)
                {
                    SuccessCounter++;
                    Kernel.Terminal.WriteLine($"\t[{i + 1}] Ping succeeded in {PingTime} millisecond(s).");
                    Thread.Sleep(250);
                }

                else
                {
                    Kernel.Terminal.WriteLine($"\tPing failed.");
                }
            }

            Kernel.Terminal.WriteLine($"{SuccessCounter}/4 pings succeeded. ({(float)(SuccessCounter / 4f) * 100f}%)\n");

        }
    }
}
