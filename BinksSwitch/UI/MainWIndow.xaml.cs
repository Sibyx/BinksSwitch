using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            CamTable.DataContext = CurrentApp.SwitchInstance.CamTable;

            CurrentApp.SwitchInstance.CamChange += RefreshCamTable;
        }

        private void StartSwitchClick(object sender, RoutedEventArgs e)
        {
            foreach (Device device in DeviceTable.SelectedItems)
            {
                device.Open();
            }
        }

        private void RefreshCamTable(object sender, EventArgs eventArgs)
        {
            CurrentApp.Dispatcher?.Invoke(() =>
            {
                CamTable.Items.Refresh();
            });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            CurrentApp.SwitchInstance.Exit();
        }
    }
}
