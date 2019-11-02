using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BinksSwitch.Network.Entities;
using PacketDotNet;
using SharpPcap.WinPcap;

namespace BinksSwitch.Network
{
    public class NetworkSwitch
    {
        public List<Device> Devices { get; } = new List<Device>();
        public Dictionary<string, CamRecord> CamTable { get; } = new Dictionary<string, CamRecord>();
        public event EventHandler<EventArgs> CamChange = null;

        private readonly Timer _clock = new Timer(1000);

        public NetworkSwitch()
        {
            var devices = WinPcapDeviceList.Instance;

            foreach (var device in devices)
            {
                if (device.Interface.FriendlyName != null)
                {
                    Devices.Add(new Device(device, PacketArrival));
                }
            }

            _clock.Elapsed += ClockTickEvent;
            _clock.Start();
        }

        private void ClockTickEvent(object source, ElapsedEventArgs e)
        {
            if (CamTable.Count == 0)
            {
                return;
            }

            foreach (var physicalAddress in CamTable.Keys.ToList())
            {
                if (CamTable[physicalAddress].TimeToDie())
                {
                    CamTable.Remove(physicalAddress);
                }
            }

            CamChange?.Invoke(this, e);
        }

        private void PacketArrival(object sender, EthernetPacket eth)
        {
            var senderDevice = (Device) sender;

            if (CamTable.ContainsKey(eth.DestinationHardwareAddress.ToString()))
            {
                var record = CamTable[eth.DestinationHardwareAddress.ToString()];

                if (record.Device.Name != senderDevice.Name)
                {
                    record.Device.SendPacket(eth);
                }
            }
            else
            {

                foreach (var device in Devices.Where(device => device.IsOpened))
                {
                    if (device.Name != senderDevice.Name)
                    {
                        device.SendPacket(eth);
                    }
                }
            }

            if (CamTable.ContainsKey(eth.SourceHardwareAddress.ToString()))
            {
                CamTable[eth.SourceHardwareAddress.ToString()].Refresh(senderDevice);
            }
            else
            {
                CamTable.Add(eth.SourceHardwareAddress.ToString(), new CamRecord(senderDevice, eth.SourceHardwareAddress.ToString()));
            }
        }

        public void Exit()
        {
            _clock.Stop();
            foreach (var device in Devices)
            {
                device.Close();
            }
        }
    }
}
