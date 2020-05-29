using NesEmulatorGUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static NesLib.Devices.Controller;

namespace NesEmulatorGUI.Controls
{
    /// <summary>
    /// Interaction logic for ControllerSettings.xaml
    /// </summary>
    public partial class ControllerSettings : UserControl
    {
        public int ControllerIndex { get; set; }

        public Dictionary<ControllerKeys, Key> Mappings { get; set; }

        public ControllerSettings()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMappings();
        }

        private void LoadMappings()
        {
            var controller = ControllerManager.Instance.Controllers[ControllerIndex];
            Mappings = ControllerManager.Instance.MappingsBuffer
                .Where(map => map.Value.Controller == controller)
                .ToDictionary(map => map.Value.ControllerKey, map => map.Key);

            dataGrid.ItemsSource = Mappings;

            dataGrid.Columns[0].Header = "Controller key";
            dataGrid.Columns[1].Header = "Mapped key";

            // let columns take up all space
            dataGrid.Columns[0].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
            dataGrid.Columns[1].Width = new DataGridLength(1, DataGridLengthUnitType.Star);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = (Button)sender;
            Enum.TryParse(senderButton.Content.ToString(), out ControllerKeys targetButton);

            PressKeyWindow pressKeyWindow = new PressKeyWindow(this)
            {
                Owner = Window.GetWindow(this),
                ControllerIndex = ControllerIndex,
                ControllerKey = targetButton,
            };

            pressKeyWindow.ShowDialog();
        }

        public void AddKeyMapping(ControllerKeys controllerKey, Key newKey)
        {
            // add to local list
            Mappings[controllerKey] = newKey;
            dataGrid.Items.Refresh();

            // add to controller manager buffer
            ControllerManager.Instance.AddKeyToMappingBuffer(ControllerIndex, controllerKey, newKey);
        }

        public void RestoreDefault()
        {
            LoadMappings();
            dataGrid.Items.Refresh();
        }
    }
}
