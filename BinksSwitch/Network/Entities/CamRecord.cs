using System;
using System.ComponentModel;
using System.Configuration;

namespace BinksSwitch.Network.Entities
{
    public class CamRecord : INotifyPropertyChanged
    {
        private Device _device;
        private int _ttl;
        private string _physicalAddress;

        public event PropertyChangedEventHandler PropertyChanged;

        public Device Device
        {
            get => _device;
            private set
            {
                _device = value;
                NotifyPropertyChanged("Device");
            }
        }

        public int TTL
        {
            get => _ttl;
            private set
            {
                _ttl = value;
                NotifyPropertyChanged("TTL");
            }
        }

        public string PhysicalAddress
        {
            get => _physicalAddress;
            private set
            {
                _physicalAddress = value;
                NotifyPropertyChanged("PhysicalAddress");
            }
        }
        
        private readonly object _camLock = new object();

        public CamRecord(Device device, string physicalAddress)
        {
            this.Device = device;
            this.PhysicalAddress = physicalAddress;
            this.TTL = Convert.ToInt32(ConfigurationManager.AppSettings["CamRecordTTL"]);
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Refresh(Device device)
        {
            lock (_camLock)
            {
                Device = device;
                TTL = Convert.ToInt32(ConfigurationManager.AppSettings["CamRecordTTL"]);
            }
        }

        public bool TimeToDie()
        {
            lock (_camLock)
            {
                return !Convert.ToBoolean(TTL--);
            }
        }
    }
}
