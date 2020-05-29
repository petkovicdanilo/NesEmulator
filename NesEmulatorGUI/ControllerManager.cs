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
    public class ControllerManager
    {
        public static ControllerManager Instance { get; private set; } = new ControllerManager();

        private ControllerManager()
        {
            CreateMappingsBuffer();
            SetDefaultMappings();
            FlushMappingsBuffer();
        }

        public Controller[] Controllers { get; set; } = new Controller[2]
        {
            new Controller(),
            new Controller()
        };

        public Dictionary<Key, ControllerKeyInfo> Mappings = new Dictionary<Key, ControllerKeyInfo>();
        public Dictionary<Key, ControllerKeyInfo> MappingsBuffer;

        public bool Controller2Enabled { get; set; } = true;

        public class ControllerKeyInfo 
        {
            public Controller Controller;
            public ControllerKeys ControllerKey;
        }

        public void SetDefaultMappings()
        {
            AddKeyToMappingBuffer(0, ControllerKeys.Left, Key.Left);
            AddKeyToMappingBuffer(0, ControllerKeys.Right, Key.Right);
            AddKeyToMappingBuffer(0, ControllerKeys.Up, Key.Up);
            AddKeyToMappingBuffer(0, ControllerKeys.Down, Key.Down);
            AddKeyToMappingBuffer(0, ControllerKeys.Start, Key.Q);
            AddKeyToMappingBuffer(0, ControllerKeys.Select, Key.W);
            AddKeyToMappingBuffer(0, ControllerKeys.A, Key.A);
            AddKeyToMappingBuffer(0, ControllerKeys.B, Key.S);

            AddKeyToMappingBuffer(1, ControllerKeys.Left, Key.NumPad4);
            AddKeyToMappingBuffer(1, ControllerKeys.Right, Key.NumPad6);
            AddKeyToMappingBuffer(1, ControllerKeys.Up, Key.NumPad8);
            AddKeyToMappingBuffer(1, ControllerKeys.Down, Key.NumPad5);
            AddKeyToMappingBuffer(1, ControllerKeys.Start, Key.NumPad7);
            AddKeyToMappingBuffer(1, ControllerKeys.Select, Key.NumPad9);
            AddKeyToMappingBuffer(1, ControllerKeys.A, Key.NumPad1);
            AddKeyToMappingBuffer(1, ControllerKeys.B, Key.NumPad2);
        }

        public void SetKeyPressed(Key key, bool value)
        {
            if(Mappings.ContainsKey(key))
            {
                var controllerKey = Mappings[key].ControllerKey;
                var controller = Mappings[key].Controller;

                if(!Controller2Enabled && controller == Controllers[1])
                {
                    // don't detect controller 2 keys when it is not enabled
                    return;
                }

                Mappings[key].Controller.SetKeyStatus(controllerKey, value);
            }
        }

        public void AddKeyToMappingBuffer(int controllerIndex, ControllerKeys controllerKey, Key newKey)
        {
            var controller = Controllers[controllerIndex];

            // remove old mappings
            var toRemove =
                MappingsBuffer
                    .Where(pair => (pair.Value.Controller == controller 
                                && pair.Value.ControllerKey == controllerKey) || pair.Key == newKey)
                    .Select(pair => pair.Key)
                    .ToList();

            foreach (var key in toRemove)
            {
                MappingsBuffer.Remove(key);
            }

            MappingsBuffer.Add(
                newKey, 
                new ControllerKeyInfo
                {
                    Controller = controller,
                    ControllerKey = controllerKey
                }
            );
        }

        public void CreateMappingsBuffer()
        {
            MappingsBuffer = new Dictionary<Key, ControllerKeyInfo>(Mappings);
        }

        public void FlushMappingsBuffer()
        {
            Mappings = MappingsBuffer;
            DestroyMappingsBuffer();
        }

        public void DestroyMappingsBuffer()
        {
            MappingsBuffer = null;
        }
    }
}
