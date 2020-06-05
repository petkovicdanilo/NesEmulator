using System;

namespace NesLib.Devices
{
    public interface IPpuBusDevice
    {
        byte PpuRead(UInt16 address);
        void PpuWrite(UInt16 address, byte data);
    }
}
