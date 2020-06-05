using System;

namespace NesLib.Devices.PpuEntities.Registers
{
    [Serializable]
    public class ShiftRegister
    {
        public UInt16 Register { get; set; }

        public void Shift()
        {
            Register <<= 1;
        }

        public void Load(byte data)
        {
            Register = (UInt16)(Register & 0xFF00 | data);
        }
    }
}
