using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using BinksSwitch.Network.Entities;

namespace BinksSwitch.UI
{
    /// <summary>
    /// Interaction logic for MainWIndow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }
    }
}
