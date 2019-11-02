using System;
using System.Collections.Generic;
using System.Linq;
using BinksSwitch.Network.Entities;
using PacketDotNet;
using SharpPcap.WinPcap;

namespace BinksSwitch.Network
{
    public class NetworkSwitch
    {
        public List<Device> ActiveDevices { get; set; } = new List<Device>();
        public List<Device> Devices { get; } = new List<Device>();
        public Dictionary<string, CamRecord> CamTable { get; } = new Dictionary<string, CamRecord>();

        public NetworkSwitch()
        {
            var devices = WinPcapDeviceList.Instance;

            foreach (var device in devices)
            {
                Devices.Add(new Device(device, PacketArrival));
            }
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

                foreach (var device in ActiveDevices)
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
                CamTable.Add(eth.SourceHardwareAddress.ToString(), new CamRecord(senderDevice));
            }
        }

        public void Exit()
        {
            foreach (var device in Devices)
            {
                device.Close();
                ActiveDevices.Remove(device);
            }
        }
    }
}
