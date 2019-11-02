using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using BinksSwitch.Network;

namespace BinksSwitch
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public NetworkSwitch SwitchInstance { get; } = new NetworkSwitch();
    }
}
