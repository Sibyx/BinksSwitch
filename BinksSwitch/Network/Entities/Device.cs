using System;
using System.Threading.Tasks;
using PacketDotNet;
using SharpPcap;
using SharpPcap.WinPcap;

namespace BinksSwitch.Network.Entities
{
    public class Device
    {
        public bool IsOpened { get; private set; }
        public string Name { get; }
        public int Sent { get; private set; }
        public int Received { get; private set; }
        private WinPcapDevice _captureDevice;

        public event EventHandler<EthernetPacket> PacketReceived = null;

        public Device(WinPcapDevice captureDevice, EventHandler<EthernetPacket> eventHandler)
        {
            PacketReceived += eventHandler;

            this.IsOpened = false;
            this.Sent = 0;
            this.Received = 0;
            this.Name = captureDevice.Interface.FriendlyName;
            this._captureDevice = captureDevice;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public bool Open()
        {
            if (!IsOpened)
            {
                _captureDevice.OnPacketArrival += PacketArrival;
                _captureDevice.Open(OpenFlags.Promiscuous | OpenFlags.NoCaptureLocal, 10);
                _captureDevice.StartCapture();
                IsOpened = true;

                return true;
            }

            return false;
        }

        public bool Close()
        {
            if (IsOpened)
            {
                _captureDevice.StopCapture();
                Console.WriteLine(_captureDevice.Statistics.ToString());
                _captureDevice.Close();
                IsOpened = false;
                return true;
            }

            return false;
        }

        private void PacketArrival(object sender, CaptureEventArgs e)
        {
            var packet = Packet.ParsePacket(LinkLayers.Ethernet, e.Packet.Data);

            if (packet is EthernetPacket)
            {
                EthernetPacket eth = (EthernetPacket) packet;
                Task.Run((() => { PacketReceived.Invoke(this, eth); }));
                Received++;
            }
        }

        public bool SendPacket(EthernetPacket eth)
        {
            if (IsOpened)
            {
                _captureDevice.SendPacket(eth.Bytes);
                Sent++;
                return true;
            }

            return false;
        }

    }
}
