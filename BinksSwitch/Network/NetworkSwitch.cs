using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using BinksSwitch.Annotations;
using BinksSwitch.Network.Entities;
using PacketDotNet;
using SharpPcap.WinPcap;

namespace BinksSwitch.Network
{
    public class NetworkSwitch
    {
        public List<Device> Devices { get; } = new List<Device>();
        public ConcurrentDictionary<string, CamRecord> CamTable { get; } = new ConcurrentDictionary<string, CamRecord>();
        public event EventHandler<EventArgs> CamChange = null;

        private readonly Timer _clock = new Timer(Properties.Settings.Default.SwitchClockRate);

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
                    CamTable.TryRemove(physicalAddress, out _);
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
                CamTable.TryAdd(eth.SourceHardwareAddress.ToString(), new CamRecord(senderDevice, eth.SourceHardwareAddress.ToString()));
            }
        }

        public void Start([ItemCanBeNull] IList devices)
        {
            _clock.Start();

            foreach (Device device in devices)
            {
                device?.Open();
            }
        }

        public void Stop()
        {
            _clock.Stop();

            foreach (var device in Devices)
            {
                device.Close();
            }

            ClearCam();
        }

        public void ClearCam()
        {
            CamTable.Clear();
            CamChange?.Invoke(this, null);
        }
    }
}
