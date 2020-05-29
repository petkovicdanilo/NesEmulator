using NesEmulatorGUI.Controls;
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
using static NesLib.Devices.Controller;

namespace NesEmulatorGUI.Windows
{
    /// <summary>
    /// Interaction logic for PressKeyWindow.xaml
    /// </summary>
    public partial class PressKeyWindow : Window
    {
        private ControllerSettings controllerSettings;

        public int ControllerIndex { get; set; }
        public ControllerKeys ControllerKey { get; set; }

        public PressKeyWindow
            (ControllerSettings controllerSettings)
        {
            InitializeComponent();
            this.controllerSettings = controllerSettings;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // get all bindings that include pressed key
            var matchedMappings = ControllerManager.Instance.MappingsBuffer.Where(map => map.Key == e.Key);

            if (matchedMappings.Count() > 0) 
            {
                var controller = ControllerManager.Instance.Controllers[ControllerIndex];
                var mapping = matchedMappings.First();

                // if pressed key is mapped to selected controller and controller key
                // leave dialog opened
                if (mapping.Value.Controller == controller 
                    && mapping.Value.ControllerKey == ControllerKey)
                {
                    return;
                }

                // invalid mapping attempt, key is already mapped to another controller key
                MessageBox.Show
                (
                    "Key is already mapped.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            // add mapping to controller settings
            // so that it can set value in its own list for displaying 
            // and also to controller manager
            controllerSettings.AddKeyMapping(ControllerKey, e.Key);
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MappingInfo.Text = $"Controller {ControllerIndex + 1}\nKey: {ControllerKey}";
        }
    }
}
