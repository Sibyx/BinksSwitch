using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BinksSwitch.Network.Entities;

namespace BinksSwitch.UI
{
    /// <summary>
    /// Interaction logic for MainWIndow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static App CurrentApp => (App) Application.Current;


        public MainWindow()
        {
            InitializeComponent();
            DeviceTable.DataContext = CurrentApp.SwitchInstance.Devices;
            CamTable.DataContext = CurrentApp.SwitchInstance.CamTable.Values.ToList();

            CurrentApp.SwitchInstance.CamChange += RefreshCamTable;
        }

        private void StartSwitchClick(object sender, RoutedEventArgs e)
        {
            StartSwitchButton.IsEnabled = false;
            StopSwitchButton.IsEnabled = true;
            
            CurrentApp.SwitchInstance.Start(DeviceTable.SelectedItems);

            StatisticsTable.DataContext = CurrentApp.SwitchInstance.Devices.Where(device => device.IsOpened);
        }

        private void RefreshCamTable(object sender, EventArgs eventArgs)
        {
            CurrentApp.Dispatcher?.Invoke(() =>
            {
                CamTable.DataContext = CurrentApp.SwitchInstance.CamTable.Values.ToList();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            CurrentApp.SwitchInstance.Stop();
        }

        private void StopSwitchClick(object sender, RoutedEventArgs e)
        {
            StartSwitchButton.IsEnabled = true;
            StopSwitchButton.IsEnabled = false;

            CurrentApp.SwitchInstance.Stop();
        }

        private void ClearCamClick(object sender, RoutedEventArgs e)
        {
            CurrentApp.SwitchInstance.ClearCam();
        }

        private void SettingsClick(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void DeviceRecordDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                var row = sender as DataGridRow;
                var firewallWindow = new FirewallWindow(row?.DataContext as Device);
                firewallWindow.ShowDialog();
            }
        }
    }
}
