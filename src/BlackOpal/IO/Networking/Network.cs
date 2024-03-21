using Cosmos.System.Network.IPv4.UDP.DHCP;
using Cosmos.System.Network.Config;
using Cosmos.System.Network.IPv4;
using Cosmos.HAL;
using System.Linq;
using System;
using IO.CMD;
using BlackOpal;

namespace IO.Networking
{
    internal class Network
    {
        /* VARIABLES */

        /* FUNCTIONS */
        // Automatically configure the NIC via DHCP
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

        // Manually configure the NIC
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

        // Ping the specified IP address using ICMP
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

        // Check if any NICs are configured
        public static bool IsConfigured()
        {
            return NetworkConfiguration.CurrentNetworkConfig != null && NetworkConfiguration.CurrentNetworkConfig.Device.Ready;
        }

        // Validate an IPv4 address
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

        // Convert a string to an IPv4 address
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

        // Initialize a NIC
        public static void Init(bool DHCP = true)
        {
            if (DHCP)
            {
                if (NetworkDevice.Devices.Count == 0)
                {
                    ConsoleFunctions.PrintLogMSG("The network could not be configured because there are no supported NICs installed.\n\n\r", ConsoleFunctions.LogType.INFO);
                    return;
                }

                ConsoleFunctions.PrintLogMSG("Attempting to obtain an IPv4 address via DHCP...\n\r", ConsoleFunctions.LogType.INFO);

                if (DHCPAutoconfig() == false)
                {
                    ConsoleFunctions.PrintLogMSG("DHCP autoconfiguration failed.\n\r", ConsoleFunctions.LogType.ERROR);
                }

                // If DHCP worked, print the configuration information
                if (IsConfigured())
                {
                    Kernel.Terminal.WriteLine($"[== NET CONFIG ==]\n" +
                        $"IP: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.IPAddress.ToString()}\n" +
                        $"SUBNET: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.SubnetMask.ToString()}\n" +
                        $"DEFAULT GATEWAY: {NetworkConfiguration.CurrentNetworkConfig.IPConfig.DefaultGateway.ToString()}\n" +
                        $"MAC: {NetworkConfiguration.CurrentNetworkConfig.Device.MACAddress.ToString()}\n" +
                        $"DEVICE NAME: {NetworkConfiguration.CurrentNetworkConfig.Device.Name.ToString()}\n" +
                        $"ID: {NetworkConfiguration.CurrentNetworkConfig.Device.NameID.ToString()}\n");
                }
                else
                {
                    ConsoleFunctions.PrintLogMSG("DHCP autoconfiguration failed: The device is not ready or an invalid IP address was assigned.\n\n\r", ConsoleFunctions.LogType.ERROR);
                }
            }
        }
    }
}
