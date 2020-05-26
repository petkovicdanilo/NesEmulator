using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NesEmulatorGUI.Commands
{
    public static class HelpCommands
    {
        public static readonly RoutedUICommand About = new RoutedUICommand
        (
            "About",
            "About",
            typeof(HelpCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.A, ModifierKeys.Control)
            }
        );
    }
}
