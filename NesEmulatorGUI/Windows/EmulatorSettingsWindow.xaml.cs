using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
    /// Interaction logic for EmulatorSettingsWindow.xaml
    /// </summary>
    public partial class EmulatorSettingsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private WindowSize _size = WindowSize.x3;

        public WindowSize WindowSizeSelected
        {
            get
            {
                return _size;
            }

            set
            {
                _size = value;
                OnPropertyChanged();
            }
        }

        private MainWindow mainWindow;

        public EmulatorSettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            DataContext = this;
            this.mainWindow = mainWindow;
            WindowSizeSelected = (WindowSize) mainWindow.NesScale;
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.NesScale = (int)WindowSizeSelected;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public enum WindowSize
    {
        x1 = 1,
        x2 = 2,
        x3 = 3,
        x4 = 4,
    };
}
