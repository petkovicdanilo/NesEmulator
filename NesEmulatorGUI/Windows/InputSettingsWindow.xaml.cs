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

namespace NesEmulatorGUI.Windows
{
    /// <summary>
    /// Interaction logic for InputSettingsWindow.xaml
    /// </summary>
    public partial class InputSettingsWindow : Window
    {
        public InputSettingsWindow()
        {
            InitializeComponent();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ControllerManager.Instance.SetDefaultMappings();

            Controller1Control.RestoreDefault();
            Controller2Control.RestoreDefault();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            ControllerManager.Instance.FlushMappingsBuffer();
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ControllerManager.Instance.DestroyMappingsBuffer();
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ControllerManager.Instance.DestroyMappingsBuffer();
        }
    }
}
