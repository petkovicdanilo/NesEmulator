using NesLib.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static NesLib.Devices.Controller;

namespace NesEmulatorGUI
{
    public static class ControllerManager
    {
        public static Controller[] controllers = new Controller[2]
        {
            new Controller(),
            new Controller()
        };

        private static Dictionary<Key, Mapping> mappings = new Dictionary<Key, Mapping>(); 

        static ControllerManager()
        {
            AddKeyMapping(0, ControllerKeys.Left, Key.Left);
            AddKeyMapping(0, ControllerKeys.Right, Key.Right);
            AddKeyMapping(0, ControllerKeys.Up, Key.Up);
            AddKeyMapping(0, ControllerKeys.Down, Key.Down);
            AddKeyMapping(0, ControllerKeys.Start, Key.Q);
            AddKeyMapping(0, ControllerKeys.Select, Key.W);
            AddKeyMapping(0, ControllerKeys.A, Key.A);
            AddKeyMapping(0, ControllerKeys.B, Key.S);

            AddKeyMapping(1, ControllerKeys.Left, Key.NumPad4);
            AddKeyMapping(1, ControllerKeys.Right, Key.NumPad6);
            AddKeyMapping(1, ControllerKeys.Up, Key.NumPad8);
            AddKeyMapping(1, ControllerKeys.Down, Key.NumPad5);
            AddKeyMapping(1, ControllerKeys.Start, Key.NumPad7);
            AddKeyMapping(1, ControllerKeys.Select, Key.NumPad9);
            AddKeyMapping(1, ControllerKeys.A, Key.NumPad1);
            AddKeyMapping(1, ControllerKeys.B, Key.NumPad2);
        }

        private class Mapping 
        {
            public Controller Controller;
            public ControllerKeys ControllerKey;
        }

        public static void SetKeyPressed(Key key, bool value)
        {
            if(mappings.ContainsKey(key))
            {
                var controllerKey = mappings[key].ControllerKey;
                mappings[key].Controller.SetKeyStatus(controllerKey, value);
            }
        }

        public static void AddKeyMapping(int controllerIndex, ControllerKeys controllerKey, Key newKey)
        {
            mappings.Add(
                newKey, 
                new Mapping
                {
                    Controller = controllers[controllerIndex],
                    ControllerKey = controllerKey
                }
            );
        }
    }
}
