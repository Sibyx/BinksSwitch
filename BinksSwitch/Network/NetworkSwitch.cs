using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPcap;

namespace BinksSwitch.Network
{
    public class NetworkSwitch
    {
        public NetworkSwitch()
        {
            var devices = CaptureDeviceList.Instance;

            foreach (var device in devices)
            {
                Console.WriteLine(device);
            }
        }
    }
}
