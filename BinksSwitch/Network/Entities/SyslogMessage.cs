using System;
using System.Text;
using System.Threading;
using PacketDotNet;
using PacketDotNet.Utils;

namespace BinksSwitch.Network.Entities
{
    public enum Severity
    {
        Emergency,
        Alert,
        Critical,
        Error,
        Warning,
        Notice,
        Informational,
        Debug
    }

    class SyslogMessage
    {
        private readonly Severity _severity;
        private readonly string _message;
        private const short FacilityCode = 3;
        private readonly string _timestamp;

        public byte[] Bytes => Encoding.ASCII.GetBytes(this.ToString());

        public SyslogMessage(Severity severity, string message)
        {
            this._severity = severity;
            this._message = message;
            this._timestamp = string.Concat(DateTime.UtcNow.ToString("s"), "Z");

        }

        public override string ToString()
        {
            var priority = FacilityCode * 8 + this._severity;
            
            // https://tools.ietf.org/html/rfc5424#section-6.1
            // <PRIORITY>VERSION TIMESTAMP HOSTNAME APP-NAME PROCID MSGID - - MESSAGE
            return $"<{priority}>1 {_timestamp} {Properties.Settings.Default.SyslogDeviceIP} {Thread.CurrentThread.ManagedThreadId} - - {_message}";
        }

        public static implicit operator IPv4Packet(SyslogMessage syslogMessage)
        {
            var udpPacket = new UdpPacket(Properties.Settings.Default.SyslogDevicePort,
                Properties.Settings.Default.SyslogServerPort)
            {
                PayloadDataSegment = new ByteArraySegment(syslogMessage.Bytes)
            };

            var ipSourceAddress = System.Net.IPAddress.Parse(Properties.Settings.Default.SyslogDeviceIP);
            var ipDestinationAddress = System.Net.IPAddress.Parse(Properties.Settings.Default.SyslogServer);
            
            var ipPacket = new IPv4Packet(ipSourceAddress, ipDestinationAddress) {PayloadPacket = udpPacket};
            ipPacket.UpdateIPChecksum();

            return ipPacket;
        }
    }
}
