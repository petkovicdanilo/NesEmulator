using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NesEmulatorGUI.Commands
{
    public static class FileCommands
    {
        public static readonly RoutedUICommand LoadGame = new RoutedUICommand
        (
            "LoadGame",
            "LoadGame",
            typeof(FileCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.O, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand SaveState = new RoutedUICommand
        (
            "SaveState",
            "SaveState",
            typeof(FileCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand LoadState = new RoutedUICommand
        (
            "LoadState",
            "LoadState",
            typeof(FileCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.L, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand Exit = new RoutedUICommand
        (
            "Exit",
            "Exit",
            typeof(FileCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.F4, ModifierKeys.Alt)
            }
        );
    }
}
