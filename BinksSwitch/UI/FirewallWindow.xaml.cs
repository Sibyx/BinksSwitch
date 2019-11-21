using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BinksSwitch.Network.Entities;

namespace BinksSwitch.UI
{
    /// <summary>
    /// Interaction logic for FirewallWindow.xaml
    /// </summary>
    public partial class FirewallWindow
    {
        private readonly Device _device;

        public FirewallWindow(Device device)
        {
            InitializeComponent();
            _device = device;

            DeviceNameText.Text = _device.Name;
            this.Title = $"Firewall rules ({_device}) - BinksSwitch";
            FirewallRulesTable.DataContext = _device.FirewallRules;
        }

        private void AddRowClick(object sender, RoutedEventArgs e)
        {
            var addRuleWindow = new AddFirewallRule(_device);
            addRuleWindow.ShowDialog();
            FirewallRulesTable.Items.Refresh();
        }

        private void RemoveFirewallRuleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                var row = sender as DataGridRow;
                _device.FirewallRules.Remove(row?.DataContext as FirewallRule);
                FirewallRulesTable.Items.Refresh();
            }
        }
    }
}
