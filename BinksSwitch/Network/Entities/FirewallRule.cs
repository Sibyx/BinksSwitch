using System;
using System.Collections.Generic;
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

        public Operation RuleOperation { get; set; }
        [CanBeNull] public Direction? RuleDirection { get; set; }
        [CanBeNull] public PhysicalAddress SourceMac { get; set; }
        [CanBeNull] public IPAddress SourceIp { get; set; }
        [CanBeNull] public PhysicalAddress DestinationMac { get; set; }
        [CanBeNull] public IPAddress  DestinationIp{ get; set; }
        [CanBeNull] public Protocol? RuleProtocol { get; set; }
        [CanBeNull] public ushort? SourcePort { get; set; }
        [CanBeNull] public ushort? DestinationPort { get; set; }

        private bool resolveValidationStack(Stack<Func<bool>> stack)
        {
            foreach (Func<bool> rule in stack)
            {
                if (!rule())
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsMatch(EthernetPacket packet)
        {
            var validationStack = new Stack<Func<bool>>();

            if (SourceMac != null)
            {
                validationStack.Push(() => SourceMac.Equals(packet.SourceHardwareAddress));
            }

            if (DestinationMac != null)
            {
                validationStack.Push(() => DestinationMac.Equals(packet.DestinationHardwareAddress));
            }

            var arp = packet.Extract<ArpPacket>();
            if (arp != null && RuleProtocol.Equals(Protocol.Arp))
            {
                return resolveValidationStack(validationStack);
            }

            var lldp = packet.Extract<LldpPacket>();
            if (lldp != null && RuleProtocol.Equals(Protocol.Lldp))
            {
                return resolveValidationStack(validationStack);
            }


            var ipv4 = packet.Extract<IPv4Packet>();
            if (ipv4 != null)
            {
                if (SourceIp != null)
                {
                    validationStack.Push(() => SourceIp.Equals(ipv4.SourceAddress));
                }

                if (DestinationIp != null)
                {
                    validationStack.Push(() => DestinationIp.Equals(ipv4.DestinationAddress));
                }

                if (RuleProtocol.Equals(Protocol.Ipv4))
                {
                    return resolveValidationStack(validationStack);
                }
            }

            var ipv6 = packet.Extract<IPv6Packet>();
            if (ipv6 != null)
            {
                if (SourceIp != null)
                {
                    validationStack.Push(() => SourceIp.Equals(ipv6.SourceAddress));
                }

                if (DestinationIp != null)
                {
                    validationStack.Push(() => DestinationIp.Equals(ipv6.DestinationAddress));
                }

                if (RuleProtocol.Equals(Protocol.Ipv6))
                {
                    return resolveValidationStack(validationStack);
                }
            }

            var icmpv4 = packet.Extract<IcmpV4Packet>();
            if (icmpv4 != null && RuleProtocol.Equals(Protocol.Icmpv4))
            {
                if (DestinationPort != null)
                {
                    validationStack.Push(() => (((ushort)icmpv4.TypeCode) / 256) == DestinationPort);
                }

                return resolveValidationStack(validationStack);
            }

            var icmpv6 = packet.Extract<IcmpV4Packet>();
            if (icmpv6 != null && RuleProtocol.Equals(Protocol.Icmpv6))
            {
                if (DestinationPort != null)
                {
                    validationStack.Push(() => (((ushort)icmpv6.TypeCode) / 256) == DestinationPort);
                }

                return resolveValidationStack(validationStack);
            }

            var tcp = packet.Extract<TcpPacket>();
            if (tcp != null && RuleProtocol.Equals(Protocol.Tcp))
            {
                if (SourcePort != null)
                {
                    validationStack.Push(() => SourcePort.Equals(tcp.SourcePort));
                }

                if (DestinationPort != null)
                {
                    validationStack.Push(() => DestinationPort.Equals(tcp.DestinationPort));
                }

                return resolveValidationStack(validationStack);
            }

            var udp = packet.Extract<UdpPacket>();
            if (udp != null && RuleProtocol.Equals(Protocol.Udp))
            {
                if (SourcePort != null)
                {
                    validationStack.Push(() => SourcePort.Equals(udp.SourcePort));
                }

                if (DestinationPort != null)
                {
                    validationStack.Push(() => DestinationPort.Equals(udp.DestinationPort));
                }

                return resolveValidationStack(validationStack);
            }

            return false;
        }
    }
}
