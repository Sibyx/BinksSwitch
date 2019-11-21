using System.ComponentModel;

namespace BinksSwitch.Network.Entities
{
    public enum Protocol
    {
        [Description("TCP")] Tcp,
        [Description("UDP")] Udp,
        [Description("ICMPv4")] Icmpv4,
        [Description("ICMPv6")] Icmpv6,
        [Description("IPv4")] Ipv4,
        [Description("IPv6")] Ipv6,
        [Description("ARP")] Arp,
        [Description("LLDP")] Lldp
    }
}
