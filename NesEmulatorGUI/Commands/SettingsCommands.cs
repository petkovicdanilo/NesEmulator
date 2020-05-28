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

        public static readonly RoutedUICommand EmulatorSettings = new RoutedUICommand
        (
            "EmulatorSettings",
            "EmulatorSettings",
            typeof(SettingsCommands),
            new InputGestureCollection()
            {
            new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift)
            }
        );
    }
}
