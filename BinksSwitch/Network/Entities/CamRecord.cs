using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinksSwitch.Network.Entities
{
    public class CamRecord
    {
        public Device Device { get; private set; }
        public int TTL { get; private set; }
        private readonly object _camLock = new object();

        public CamRecord(Device device)
        {
            this.Device = device;
            this.TTL = 30;
        }

        public void Refresh(Device device)
        {
            lock (_camLock)
            {
                Device = device;
                TTL = 30;
            }
        }

        public bool TimeToDie()
        {
            lock (_camLock)
            {
                return Convert.ToBoolean(TTL--);
            }
        }
    }
}
