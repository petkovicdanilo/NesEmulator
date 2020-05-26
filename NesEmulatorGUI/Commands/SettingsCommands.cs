using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NesEmulatorGUI.Commands
{
    public static class SettingsCommands
    {
        public static readonly RoutedUICommand InputSettings = new RoutedUICommand
        (
            "InputSettings",
            "InputSettings",
            typeof(SettingsCommands),
            new InputGestureCollection()
            {
            new KeyGesture(Key.I, ModifierKeys.Control | ModifierKeys.Shift)
            }
        );

        public static readonly RoutedUICommand WindowSettings = new RoutedUICommand
        (
            "WindowSettings",
            "WindowSettings",
            typeof(SettingsCommands),
            new InputGestureCollection()
            {
            new KeyGesture(Key.W, ModifierKeys.Control | ModifierKeys.Shift)
            }
        );
    }
}
