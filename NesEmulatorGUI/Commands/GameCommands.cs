using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NesEmulatorGUI.Commands
{
    public static class GameCommands
    {
        public static readonly RoutedUICommand Resume = new RoutedUICommand
        (
            "Resume",
            "Resume",
            typeof(GameCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.P, ModifierKeys.Control | ModifierKeys.Shift)
            }
        );

        public static readonly RoutedUICommand Pause = new RoutedUICommand
        (
            "Pause",
            "Pause",
            typeof(GameCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.P, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand Reset = new RoutedUICommand
        (
            "Reset",
            "Reset",
            typeof(GameCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.R, ModifierKeys.Control)
            }
        );

        public static readonly RoutedUICommand Screenshot = new RoutedUICommand
        (
            "Screenshot",
            "Screenshot",
            typeof(GameCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Alt)
            }
        );
    }
}
