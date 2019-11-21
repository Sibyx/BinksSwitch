using System.Windows;

namespace BinksSwitch.UI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow
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
