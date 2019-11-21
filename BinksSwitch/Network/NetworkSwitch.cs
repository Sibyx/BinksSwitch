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
        public event EventHandler<EventArgs> CamChange;

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
                    this.Log(new SyslogMessage(Severity.Debug, $"{physicalAddress} expired in CAM"));
                }
            }

            if (CamTable.Count == 0)
            {
                this.Log(new SyslogMessage(Severity.Alert, "CAM table is empty"));
            }

            CamChange?.Invoke(this, e);
        }

        private void PacketArrival(object sender, EthernetPacket eth)
        {
            var senderDevice = (Device) sender;

            if (!senderDevice.PassedFirewallRules(Direction.In, eth))
            {
                this.Log(new SyslogMessage(Severity.Debug, $"Packet thrown away by firewall on input"));
                return;
            }

            if (CamTable.ContainsKey(eth.DestinationHardwareAddress.ToString()))
            {
                var record = CamTable[eth.DestinationHardwareAddress.ToString()];

                if (record.Device.Name != senderDevice.Name)
                {
                    if (record.Device.PassedFirewallRules(Direction.Out, eth))
                    {
                        record.Device.SendPacket(eth);
                    }
                    else
                    {
                        this.Log(new SyslogMessage(Severity.Debug, $"Packet thrown away by firewall on output"));
                    }
                }
            }
            else
            {

                foreach (var device in Devices.Where(device => device.IsOpened))
                {
                    if (device.Name != senderDevice.Name)
                    {
                        if (device.PassedFirewallRules(Direction.Out, eth))
                        {
                            device.SendPacket(eth);
                        }
                        else
                        {
                            this.Log(new SyslogMessage(Severity.Debug, $"Packet thrown away by firewall on output"));
                        }
                    }
                }
            }

            if (CamTable.ContainsKey(eth.SourceHardwareAddress.ToString()))
            {
                var record = CamTable[eth.SourceHardwareAddress.ToString()];
                if (senderDevice != record.Device)
                {
                    this.Log(new SyslogMessage(Severity.Warning, $"{eth.SourceHardwareAddress} changed from {record.Device} to {senderDevice}"));
                }
                CamTable[eth.SourceHardwareAddress.ToString()].Refresh(senderDevice);
            }
            else
            {
                this.Log(new SyslogMessage(Severity.Informational, $"New CAM record for {eth.SourceHardwareAddress} on {senderDevice}"));
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

            this.Log(new SyslogMessage(Severity.Notice, "BinksSwitch started! Mesa Jar Jar Binks."));
        }

        public void Stop()
        {
            _clock.Stop();

            foreach (var device in Devices)
            {
                device.Close();
            }

            ClearCam();

            this.Log(new SyslogMessage(Severity.Notice, "BinksSwitch stopped! Ouch! Ouch!"));
        }

        public void ClearCam()
        {
            CamTable.Clear();
            CamChange?.Invoke(this, null);
            this.Log(new SyslogMessage(Severity.Alert, "CAM table have been manually flushed"));
        }

        private void Log(SyslogMessage message)
        {
            foreach (var device in Devices.Where(device => device.IsOpened))
            {
                device.Broadcast(message);
            }

            Console.WriteLine(message);
        }
    }
}
