using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using PacketDotNet;
using SharpPcap;
using SharpPcap.WinPcap;

namespace BinksSwitch.Network.Entities
{
    public class Device : INotifyPropertyChanged
    {
        private bool _isOpened;
        private string _name;
        private int _sent;
        private int _received;
        private readonly WinPcapDevice _captureDevice;

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EthernetPacket> PacketReceived;

        public bool IsOpened
        {
            get => _isOpened;
            private set
            {
                _isOpened = value;
                NotifyPropertyChanged("IsOpened");
            }
        }


        public string Name
        {
            get => _name;
            private set
            {
                _name = value;
                NotifyPropertyChanged("Name");
            }
        }


        public int Sent { 
            get => _sent;
            private set
            {
                _sent = value;
                NotifyPropertyChanged("Sent");
            }
        }


        public int Received
        {
            get => _received;
            private set
            {
                _received = value;
                NotifyPropertyChanged("Received");
            }
        }

        public Dictionary<string, Dictionary<Direction, ulong>> Statistics { get; } = new Dictionary<string, Dictionary<Direction, ulong>>
        {
            {"TCP", new Dictionary<Direction, ulong>() 
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            },
            {"UDP", new Dictionary<Direction, ulong>()
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            },
            {"ICMPv4", new Dictionary<Direction, ulong>()
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            },
            {"ICMPv6", new Dictionary<Direction, ulong>()
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            },
            {"IPv4", new Dictionary<Direction, ulong>()
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            },
            {"IPv6", new Dictionary<Direction, ulong>()
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            },
            {"ARP", new Dictionary<Direction, ulong>()
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            },
            {"LLDP", new Dictionary<Direction, ulong>()
                {
                    {Direction.In, 0 },
                    {Direction.Out, 0 }
                }
            }
        };

        public Device(WinPcapDevice captureDevice, EventHandler<EthernetPacket> eventHandler)
        {
            PacketReceived += eventHandler;

            this.IsOpened = false;
            this.Sent = 0;
            this.Received = 0;
            this.Name = captureDevice.Interface.FriendlyName;
            this._captureDevice = captureDevice;
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                Task.Run((() => { PacketReceived?.Invoke(this, eth); }));
                Received++;
                this.ProcessStatistics(Direction.In, eth);
            }
        }

        public bool SendPacket(EthernetPacket eth)
        {
            if (IsOpened)
            {
                _captureDevice.SendPacket(eth.Bytes);
                Sent++;
                this.ProcessStatistics(Direction.Out, eth);
                return true;
            }

            return false;
        }

        public bool Broadcast(IPv4Packet payload)
        {
            if (!IsOpened)
                return false;

            var ethernetDestinationHwAddress = System.Net.NetworkInformation.PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF");
            var ethernetPacket = new EthernetPacket(this._captureDevice.MacAddress,
                ethernetDestinationHwAddress,
                EthernetType.None) {PayloadPacket = payload};

            _captureDevice.SendPacket(ethernetPacket.Bytes);
            Sent++;

            return true;
        }

        private void ProcessStatistics(Direction direction, EthernetPacket packet)
        {
            if (packet.Extract<TcpPacket>() != null)
            {
                Statistics["TCP"][direction]++;
            }

            if (packet.Extract<UdpPacket>() != null)
            {
                Statistics["UDP"][direction]++;
            }

            if (packet.Extract<IcmpV4Packet>() != null)
            {
                Statistics["ICMPv4"][direction]++;
            }

            if (packet.Extract<IcmpV6Packet>() != null)
            {
                Statistics["ICMPv6"][direction]++;
            }

            if (packet.Extract<IPv4Packet>() != null)
            {
                Statistics["IPv4"][direction]++;
            }

            if (packet.Extract<IPv6Packet>() != null)
            {
                Statistics["ICMPv6"][direction]++;
            }

            if (packet.Extract<ArpPacket>() != null)
            {
                Statistics["ARP"][direction]++;
            }

            if (packet.Extract<LldpPacket>() != null)
            {
                Statistics["LLDP"][direction]++;
            }

            NotifyPropertyChanged("Statistics");
        }

    }
}
