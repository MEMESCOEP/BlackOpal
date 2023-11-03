

using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP;
using Cosmos.System.Network.IPv4.UDP.DNS;
using System;
using IO;

namespace CosmosOS_Learning
{
    internal class NTPClient
    {
        /* VARIABLES */
        const string NTPServerAddress = "pool.ntp.org";
        const int NTPPort = 123;

        /* FUNCTIONS */
        public static DateTime GetNetworkTime()
        { 
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var NTPData = new byte[48];

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            ConsoleFunctions.PrintLogMSG($"Getting IP address of: \"{NTPServerAddress}\"...\n", ConsoleFunctions.LogType.INFO);

            // Create a new DNS client so we can get the IP address of an NTP server
            var DNSClient = new DnsClient();

            // Connect to a DNS server
            DNSClient.Connect(new Address(8, 8, 8, 8));

            // Ask for the IP or the NTP server            
            DNSClient.SendAsk(NTPServerAddress);

            // Get the NTP server's ip address
            var NTPServerIP = new EndPoint(DNSClient.Receive(), NTPPort);
            ConsoleFunctions.PrintLogMSG($"Got IP: \"{NTPServerIP.Address.ToString()}\"\n", ConsoleFunctions.LogType.INFO);

            //var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            var socket = new UdpClient();

            // Connect to the NTP server
            ConsoleFunctions.PrintLogMSG($"Connecting to NTP server...\n", ConsoleFunctions.LogType.INFO);
            socket.Connect(NTPServerIP.Address, NTPPort);

            // Set the Leap Indicator, Version Number and Mode values
            NTPData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            ConsoleFunctions.PrintLogMSG($"Sending data...\n", ConsoleFunctions.LogType.INFO);
            socket.Send(NTPData);

            ConsoleFunctions.PrintLogMSG($"Receiving data...\n", ConsoleFunctions.LogType.INFO);
            socket.Receive(ref NTPServerIP);
            socket.Close();

            ConsoleFunctions.PrintLogMSG($"Calculating date & time...\n", ConsoleFunctions.LogType.INFO);

            // Get the seconds part
            ulong intPart = BitConverter.ToUInt32(NTPData, serverReplyTime);

            // Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(NTPData, serverReplyTime + 4);

            // Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            // Get the time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0)).AddMilliseconds(milliseconds);

            return networkDateTime;
            //return DateTime.Now;
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}
