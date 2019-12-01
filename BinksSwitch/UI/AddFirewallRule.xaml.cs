using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using BinksSwitch.Network.Entities;

namespace BinksSwitch.UI
{
    /// <summary>
    /// Interaction logic for AddFirewallRule.xaml
    /// </summary>
    public partial class AddFirewallRule
    {
        private readonly Device _device;

        public AddFirewallRule(Device device)
        {
            _device = device;
            InitializeComponent();
        }

        private static void InlineTry(Action action)
        {
            try
            {
                action();
            }
            catch
            {
                // ignored
            }
        }

        private void AddClick(object sender, RoutedEventArgs e)
        {
            var rule = new FirewallRule();

            if (OperationComboBox.SelectedItem is ComboBoxItem operationItem)
            {
                rule.RuleOperation = Enum.TryParse((string) operationItem.Tag, out FirewallRule.Operation operation) ? operation : FirewallRule.Operation.Deny;
            }

            if (DirectionComboBox.SelectedItem is ComboBoxItem directionItem && Enum.TryParse((string)directionItem.Tag, out Direction direction))
            {
                rule.RuleDirection = direction;
            }

            if (ProtocolComboBox.SelectedItem is ComboBoxItem protocolItem && Enum.TryParse((string)protocolItem.Tag, out Protocol protocol))
            {
                rule.RuleProtocol = protocol;
            }

            InlineTry(() => rule.DestinationMac = PhysicalAddress.Parse(DestinationMacTextBox.Text));
            InlineTry(() => rule.DestinationIp = IPAddress.Parse(DestinationIpTextBox.Text));
            InlineTry(() => rule.SourceMac = PhysicalAddress.Parse(SourceMacTextBox.Text));
            InlineTry(() => rule.SourceIp = IPAddress.Parse(SourceIpTextBox.Text));
            InlineTry(() => rule.DestinationPort = DestinationPortTextBox.Text.Length != 0 ? Convert.ToUInt16(DestinationPortTextBox.Text) : (ushort?) null);
            InlineTry(() => rule.SourcePort = SourcePortTextBox.Text.Length != 0 ? Convert.ToUInt16(SourcePortTextBox.Text) : (ushort?) null);

            _device.FirewallRules.Add(rule);
            this.Close();
        }
    }
}
