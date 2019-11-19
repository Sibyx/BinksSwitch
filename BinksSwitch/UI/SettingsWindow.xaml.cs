using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BinksSwitch.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default["AppName"] = AppNameText.Text;
            Properties.Settings.Default["SwitchClockRate"] = uint.Parse(SwitchClockRateText.Text);
            Properties.Settings.Default["CamRecordTTL"] = uint.Parse(CamRecordTtlText.Text);
            Properties.Settings.Default["SyslogDeviceIP"] = SyslogDeviceIpText.Text;
            Properties.Settings.Default["SyslogServer"] = SyslogServerText.Text;
            Properties.Settings.Default["SyslogServerPort"] = ushort.Parse(SyslogServerPortText.Text);
            Properties.Settings.Default.Save();

            this.Close();
        }
    }
}
