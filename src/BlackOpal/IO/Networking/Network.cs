using Cosmos.HAL;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.System.Network.IPv4.UDP.DHCP;
using System.Linq;
using System;

namespace IO.Networking
{
    internal class Network
    {
        /* VARIABLES */

        /* FUNCTIONS */
        public static bool DHCPAutoconfig()
        {
            if (NetworkDevice.Devices.Count <= 0)
                return false;

            using (var xClient = new DHCPClient())
            {
                // Send a DHCP Discover packet
                // This will automatically set the IP configuration after a DHCP response
                xClient.SendDiscoverPacket();
                return true;
            }
        }

        public static NetworkDevice ManualConfig(string DeviceName, string IPAddress, string SubnetMask, string DefaultGateway)
        {
            try
            {
                NetworkDevice nic = NetworkDevice.GetDeviceByName(DeviceName);
                IPConfig.Enable(nic, Address.Parse(IPAddress), Address.Parse(SubnetMask), Address.Parse(DefaultGateway));

                return nic;
            }
            catch
            {
                return null;
            }
        }

        public static int ICMPPing(Address IPAddress, int Timeout = 5000)
        {
            EndPoint endPoint = new(IPAddress, 0);

            using (var xClient = new ICMPClient())
            {
                xClient.Connect(endPoint.Address);

                // Send an ICMP Echo message
                xClient.SendEcho();

                // Receive ICMP Response (return elapsed time / timeout if no response)
                int time = xClient.Receive(ref endPoint, Timeout);
                return time;
            }
        }

        public static bool IsConfigured()
        {
            return NetworkConfiguration.CurrentNetworkConfig != null && NetworkConfiguration.CurrentNetworkConfig.Device.Ready;
        }

        public static bool IsIPv4AddressValid(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        public static Address StringToAddress(string IPAddress)
        {
            try
            {
                return Address.Parse(IPAddress);
            }
            catch
            {
                return new Address(0, 0, 0, 0);
            }
        }
    }
}
