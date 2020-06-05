using System;

namespace NesLib.Devices
{
    public interface ICpuBusDevice
    {
        byte CpuRead(UInt16 address, bool debugMode);
        void CpuWrite(UInt16 address, byte data);
    }
}
