using System;
using System.Net;
using System.Net.NetworkInformation;
using BinksSwitch.Annotations;
using PacketDotNet;

namespace BinksSwitch.Network.Entities
{
    public class FirewallRule
    {
        public enum Operation
        {
            Deny,
            Permit
        }

        private const ushort AnyPort = 0;

        public Operation RuleOperation { get; set; }
        public Direction? RuleDirection { get; set; }
        [CanBeNull] public PhysicalAddress SourceMac { get; set; }
        [CanBeNull] public IPAddress SourceIp { get; set; }
        [CanBeNull] public PhysicalAddress DestinationMac { get; set; }
        [CanBeNull] public IPAddress  DestinationIp{ get; set; }
        public Protocol? RuleProtocol { get; set; }
        public ushort SourcePort { get; set; }
        public ushort DestinationPort { get; set; }

        public bool IsMatch(EthernetPacket packet)
        {
            if (SourceMac != null && !SourceMac.Equals(packet.SourceHardwareAddress))
            {
                return false;
            }

            if (DestinationMac != null && DestinationMac.Equals(packet.DestinationHardwareAddress))
            {
                return false;
            }

            var arp = packet.Extract<ArpPacket>();
            if (arp != null && RuleProtocol.Equals(Protocol.Arp))
            {
                return true;
            }

            var lldp = packet.Extract<LldpPacket>();
            if (lldp != null && RuleProtocol.Equals(Protocol.Lldp))
            {
                return true;
            }

            var ipv4 = packet.Extract<IPv4Packet>();
            if (ipv4 != null)
            {
                if (SourceIp != null && !SourceIp.Equals(ipv4.SourceAddress))
                {
                    return false;
                }

                if (DestinationIp != null && DestinationIp.Equals(ipv4.DestinationAddress))
                {
                    return false;
                }

                if (RuleProtocol.Equals(Protocol.Ipv4))
                {
                    return true;
                }
            }

            var ipv6 = packet.Extract<IPv6Packet>();
            if (ipv6 != null)
            {
                if (SourceIp != null && !SourceIp.Equals(ipv6.SourceAddress))
                    return false;

                if (DestinationIp != null && DestinationIp.Equals(ipv6.DestinationAddress))
                    return false;

                if (RuleProtocol.Equals(Protocol.Ipv6))
                {
                    return true;
                }
            }

            var icmpv4 = packet.Extract<IcmpV4Packet>();
            if (icmpv4 != null && RuleProtocol.Equals(Protocol.Icmpv4))
            {
                return (DestinationPort == 0 || (((ushort)icmpv4.TypeCode) / 256) == DestinationPort);
            }

            var icmpv6 = packet.Extract<IcmpV4Packet>();
            if (icmpv6 != null && RuleProtocol.Equals(Protocol.Icmpv6))
            {
                return (DestinationPort == 0 || (((ushort)icmpv6.TypeCode) / 256) == DestinationPort);
            }

            var tcp = packet.Extract<TcpPacket>();
            if (tcp != null && RuleProtocol.Equals(Protocol.Tcp))
            {
                if (SourcePort != AnyPort && !SourcePort.Equals(tcp.SourcePort))
                    return false;

                return DestinationPort == AnyPort || DestinationPort.Equals(tcp.DestinationPort);
            }

            var udp = packet.Extract<UdpPacket>();
            if (udp != null && RuleProtocol.Equals(Protocol.Udp))
            {
                if (SourcePort != AnyPort && !SourcePort.Equals(udp.SourcePort))
                    return false;

                return DestinationPort == AnyPort || DestinationPort.Equals(udp.DestinationPort);
            }

            return false;
        }
    }
}
